using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using AI_RPG.AICapabilities.LLM;
using AI_RPG.AICapabilities.Strategies.Models;
using AI_RPG.AICapabilities.Tools;

namespace AI_RPG.AICapabilities.Strategies;

/// <summary>
/// ReAct策略配置
/// </summary>
public sealed class ReActConfig
{
    /// <summary>
    /// 最大迭代次数
    /// </summary>
    public int MaxIterations { get; set; } = 5;

    /// <summary>
    /// ReAct提示模板
    /// </summary>
    public string PromptTemplate { get; set; } = """
你是一个游戏AI助手，请通过思考(Thought)和行动(Action)来解决问题。

可用工具：
{{tools}}

请按以下格式回复：
Thought: [你的思考过程]
Action: [工具名称]([参数])

或者当你有最终答案时：
Thought: [你的思考过程]
Final Answer: [最终答案]

历史记录：
{{history}}

问题：{{input}}
""";
}

/// <summary>
/// ReAct生成策略实现
/// </summary>
public sealed class ReActStrategy : IGenerationStrategy
{
    public string Name => "ReAct";
    public string Description => "推理+行动循环策略，通过交替思考和使用工具来解决问题";

    private readonly ILLMClient _llmClient;
    private readonly IToolExecutor _toolExecutor;
    private readonly ITokenManager _tokenManager;
    private readonly ILogger<ReActStrategy> _logger;
    private readonly ReActConfig _config;

    public ReActStrategy(
        ILLMClient llmClient,
        IToolExecutor toolExecutor,
        ITokenManager tokenManager,
        ILogger<ReActStrategy> logger,
        ReActConfig? config = null)
    {
        _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        _toolExecutor = toolExecutor ?? throw new ArgumentNullException(nameof(toolExecutor));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config ?? new ReActConfig();
    }

    public async Task<StrategyResult> ExecuteAsync(
        string input,
        StrategyContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var stopwatch = Stopwatch.StartNew();
        var steps = new List<ReActStep>();
        var totalTokens = 0;
        var iteration = 0;

        try
        {
            var history = new System.Text.StringBuilder();

            while (iteration < _config.MaxIterations)
            {
                iteration++;
                _logger.LogDebug("ReAct iteration {Iteration}", iteration);

                // 构建提示
                var prompt = BuildPrompt(input, context, history.ToString());

                // 调用LLM
                var response = await _llmClient.SendMessageAsync(prompt, cancellationToken: cancellationToken);
                totalTokens += _tokenManager.EstimateTokens(prompt) + _tokenManager.EstimateTokens(response);

                // 解析响应
                var parseResult = ParseResponse(response, iteration);

                // 记录思考步骤
                if (!string.IsNullOrEmpty(parseResult.Thought))
                {
                    steps.Add(new ReActStep
                    {
                        StepNumber = iteration,
                        Type = ReActStepType.Thought,
                        Content = parseResult.Thought
                    });
                    history.AppendLine($"Thought: {parseResult.Thought}");
                }

                // 检查是否是最终答案
                if (parseResult.IsFinalAnswer)
                {
                    steps.Add(new ReActStep
                    {
                        StepNumber = iteration,
                        Type = ReActStepType.FinalAnswer,
                        Content = parseResult.FinalAnswer!
                    });

                    stopwatch.Stop();

                    return new StrategyResult
                    {
                        IsSuccess = true,
                        Content = parseResult.FinalAnswer,
                        TokenUsed = totalTokens,
                        StepCount = iteration,
                        ExecutionTime = stopwatch.Elapsed,
                        Metadata = new Dictionary<string, object?>
                        {
                            ["steps"] = steps,
                            ["strategy"] = Name
                        }
                    };
                }

                // 执行工具
                if (parseResult.RequiresAction && !string.IsNullOrEmpty(parseResult.ToolName))
                {
                    steps.Add(new ReActStep
                    {
                        StepNumber = iteration,
                        Type = ReActStepType.Action,
                        Content = parseResult.Action!,
                        ToolName = parseResult.ToolName,
                        ToolParameters = ParseToolParameters(parseResult.ToolInput)
                    });

                    var toolResult = await ExecuteToolAsync(parseResult.ToolName, parseResult.ToolInput, cancellationToken);
                    
                    var observation = $"Observation: {toolResult}";
                    steps.Add(new ReActStep
                    {
                        StepNumber = iteration,
                        Type = ReActStepType.Observation,
                        Content = toolResult
                    });

                    history.AppendLine($"Action: {parseResult.Action}");
                    history.AppendLine(observation);
                }
                else
                {
                    // 没有行动也没有最终答案，可能是格式错误
                    _logger.LogWarning("ReAct response without action or final answer: {Response}", response);
                    break;
                }
            }

            // 达到最大迭代次数
            stopwatch.Stop();
            return new StrategyResult
            {
                IsSuccess = false,
                Content = steps.LastOrDefault(s => s.Type == ReActStepType.Thought)?.Content,
                TokenUsed = totalTokens,
                StepCount = iteration,
                ExecutionTime = stopwatch.Elapsed,
                ErrorMessage = "Reached maximum iterations without final answer",
                Metadata = new Dictionary<string, object?> { ["steps"] = steps }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error executing ReAct strategy");

            return new StrategyResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message,
                TokenUsed = totalTokens,
                StepCount = iteration,
                ExecutionTime = stopwatch.Elapsed,
                Metadata = new Dictionary<string, object?> { ["steps"] = steps }
            };
        }
    }

    private string BuildPrompt(string input, StrategyContext context, string history)
    {
        var tools = string.Join("\n", context.AvailableTools.Select(t => $"- {t}"));
        if (string.IsNullOrEmpty(tools))
        {
            tools = "无可用工具";
        }

        return _config.PromptTemplate
            .Replace("{{tools}}", tools)
            .Replace("{{history}}", string.IsNullOrEmpty(history) ? "无" : history)
            .Replace("{{input}}", input);
    }

    private ReActParseResult ParseResponse(string response, int stepNumber)
    {
        string? thought = null;
        string? action = null;
        string? toolName = null;
        string? toolInput = null;
        string? finalAnswer = null;

        // 提取Thought
        var thoughtMatch = Regex.Match(response, @"Thought:\s*(.+?)(?=\n(?:Action|Final Answer):|$)", RegexOptions.Singleline);
        if (thoughtMatch.Success)
        {
            thought = thoughtMatch.Groups[1].Value.Trim();
        }

        // 提取Final Answer
        var finalMatch = Regex.Match(response, @"Final Answer:\s*(.+)$", RegexOptions.Singleline);
        if (finalMatch.Success)
        {
            finalAnswer = finalMatch.Groups[1].Value.Trim();
        }

        // 提取Action
        var actionMatch = Regex.Match(response, @"Action:\s*(.+?)(?=\n|$)");
        if (actionMatch.Success)
        {
            action = actionMatch.Groups[1].Value.Trim();

            // 解析工具名称和参数
            var toolMatch = Regex.Match(action, @"(\w+)\s*\((.*)\)");
            if (toolMatch.Success)
            {
                toolName = toolMatch.Groups[1].Value.Trim();
                toolInput = toolMatch.Groups[2].Value.Trim();
            }
            else
            {
                // 尝试无括号格式: ToolName param
                var parts = action.Split(' ', 2);
                if (parts.Length >= 1)
                {
                    toolName = parts[0];
                    toolInput = parts.Length > 1 ? parts[1] : null;
                }
            }
        }

        return new ReActParseResult
        {
            Thought = thought,
            Action = action,
            ToolName = toolName,
            ToolInput = toolInput,
            FinalAnswer = finalAnswer
        };
    }

    private Dictionary<string, object?> ParseToolParameters(string? input)
    {
        var result = new Dictionary<string, object?>();
        
        if (string.IsNullOrEmpty(input))
            return result;

        // 尝试解析JSON格式
        try
        {
            var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(input);
            if (json != null)
            {
                foreach (var (key, value) in json)
                {
                    result[key] = value.ValueKind switch
                    {
                        JsonValueKind.String => value.GetString(),
                        JsonValueKind.Number => value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => value.ToString()
                    };
                }
                return result;
            }
        }
        catch { /* 忽略JSON解析错误 */ }

        // 简单解析: key=value,key2=value2
        var pairs = input.Split(',');
        foreach (var pair in pairs)
        {
            var kv = pair.Split('=', 2);
            if (kv.Length == 2)
            {
                result[kv[0].Trim()] = kv[1].Trim();
            }
        }

        return result;
    }

    private async Task<string> ExecuteToolAsync(string toolName, string? input, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _toolExecutor.ExecuteAsync(toolName, input ?? string.Empty, cancellationToken);
            return result ?? "工具执行完成，无返回值";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool {ToolName}", toolName);
            return $"工具执行错误: {ex.Message}";
        }
    }
}
