using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using AI_RPG.AICapabilities.LLM;
using AI_RPG.AICapabilities.Tools;

namespace AI_RPG.AICapabilities.Agents;

/// <summary>
/// ReAct Agent实现 - 思考-行动-观察循环
/// </summary>
public sealed class ReActAgent : IAgent
{
    public AgentConfig Config { get; }
    public AgentState State { get; private set; } = AgentState.Idle;

    private readonly ILLMClient _llmClient;
    private readonly IToolExecutor _toolExecutor;
    private readonly ITokenManager _tokenManager;
    private readonly ILogger<ReActAgent> _logger;
    private readonly ChatHistory _chatHistory;

    public ReActAgent(
        AgentConfig config,
        ILLMClient llmClient,
        IToolExecutor toolExecutor,
        ITokenManager tokenManager,
        ILogger<ReActAgent> logger)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
        _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        _toolExecutor = toolExecutor ?? throw new ArgumentNullException(nameof(toolExecutor));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _chatHistory = new ChatHistory();

        // 设置系统提示
        if (!string.IsNullOrEmpty(config.SystemPrompt))
        {
            _chatHistory.AddSystemMessage(config.SystemPrompt);
        }
        else
        {
            _chatHistory.AddSystemMessage(GetDefaultSystemPrompt());
        }
    }

    public async Task<AgentOutput> RunAsync(AgentInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var stopwatch = Stopwatch.StartNew();
        State = AgentState.Thinking;

        var toolCalls = new List<ToolCallRecord>();
        var totalTokens = 0;
        var iteration = 0;

        try
        {
            _logger.LogInformation("ReActAgent '{AgentName}' started processing message", Config.Name);

            // 添加用户消息
            _chatHistory.AddUserMessage(input.Message);

            while (iteration < Config.MaxIterations)
            {
                iteration++;
                _logger.LogDebug("Iteration {Iteration}/{MaxIterations}", iteration, Config.MaxIterations);

                // 调用LLM
                State = AgentState.Thinking;
                var executionSettings = new PromptExecutionSettings();
                executionSettings.ExtensionData ??= new Dictionary<string, object>();
                executionSettings.ExtensionData["temperature"] = Config.Temperature;
                executionSettings.ExtensionData["max_tokens"] = Config.MaxTokens;

                var response = await _llmClient.SendChatAsync(
                    _chatHistory,
                    executionSettings,
                    cancellationToken);

                totalTokens += _tokenManager.EstimateTokens(response);

                // 解析响应
                var (thought, action, finalAnswer) = ParseResponse(response);

                // 如果有最终答案，返回
                if (!string.IsNullOrEmpty(finalAnswer))
                {
                    _chatHistory.AddAssistantMessage(response);
                    State = AgentState.Completed;

                    stopwatch.Stop();
                    _logger.LogInformation("ReActAgent '{AgentName}' completed in {ElapsedMs}ms",
                        Config.Name, stopwatch.ElapsedMilliseconds);

                    return new AgentOutput
                    {
                        Content = finalAnswer,
                        IsSuccess = true,
                        StepCount = iteration,
                        TokenUsed = totalTokens,
                        ExecutionTime = stopwatch.Elapsed,
                        ToolCalls = toolCalls,
                        Metadata = new Dictionary<string, object?>
                        {
                            ["thought"] = thought,
                            ["agentName"] = Config.Name
                        }
                    };
                }

                // 如果有行动，执行工具
                if (!string.IsNullOrEmpty(action))
                {
                    State = AgentState.ExecutingTool;

                    var toolStopwatch = Stopwatch.StartNew();
                    var (toolName, toolInput) = ParseAction(action);

                    _logger.LogDebug("Executing tool '{ToolName}'", toolName);

                    string toolOutput;
                    bool toolSuccess;

                    try
                    {
                        toolOutput = await _toolExecutor.ExecuteAsync(toolName, toolInput, cancellationToken);
                        toolSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        toolOutput = $"Error: {ex.Message}";
                        toolSuccess = false;
                        _logger.LogWarning(ex, "Tool '{ToolName}' execution failed", toolName);
                    }

                    toolStopwatch.Stop();

                    toolCalls.Add(new ToolCallRecord
                    {
                        ToolName = toolName,
                        Input = toolInput,
                        Output = toolOutput,
                        ExecutionTime = toolStopwatch.Elapsed,
                        IsSuccess = toolSuccess
                    });

                    // 构建观察结果并添加到历史
                    var observation = $"Observation: {toolOutput}";
                    var fullResponse = $"{response}\n{observation}";
                    _chatHistory.AddAssistantMessage(fullResponse);

                    totalTokens += _tokenManager.EstimateTokens(observation);
                }
                else
                {
                    // 没有行动也没有最终答案，可能是格式问题
                    _chatHistory.AddAssistantMessage(response);
                    _logger.LogWarning("Agent response without action or final answer");
                }
            }

            // 达到最大迭代次数
            State = AgentState.Error;
            stopwatch.Stop();

            return new AgentOutput
            {
                Content = "未能找到答案，已达到最大迭代次数",
                IsSuccess = false,
                StepCount = iteration,
                TokenUsed = totalTokens,
                ExecutionTime = stopwatch.Elapsed,
                ToolCalls = toolCalls,
                ErrorMessage = "Reached maximum iterations"
            };
        }
        catch (Exception ex)
        {
            State = AgentState.Error;
            stopwatch.Stop();

            _logger.LogError(ex, "Error in ReActAgent '{AgentName}'", Config.Name);

            return new AgentOutput
            {
                Content = "执行过程中发生错误",
                IsSuccess = false,
                StepCount = iteration,
                TokenUsed = totalTokens,
                ExecutionTime = stopwatch.Elapsed,
                ToolCalls = toolCalls,
                ErrorMessage = ex.Message
            };
        }
    }

    public async IAsyncEnumerable<string> RunStreamingAsync(
        AgentInput input,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        // 流式执行简化版：先执行Run获取结果，然后逐字输出
        // 实际实现可以更复杂，支持真正的流式思考过程
        var result = await RunAsync(input, cancellationToken);

        foreach (var chunk in ChunkText(result.Content, 5))
        {
            yield return chunk;
            await Task.Delay(10, cancellationToken); // 模拟流式延迟
        }
    }

    public void Reset()
    {
        _chatHistory.Clear();
        State = AgentState.Idle;

        if (!string.IsNullOrEmpty(Config.SystemPrompt))
        {
            _chatHistory.AddSystemMessage(Config.SystemPrompt);
        }
        else
        {
            _chatHistory.AddSystemMessage(GetDefaultSystemPrompt());
        }

        _logger.LogDebug("ReActAgent '{AgentName}' reset", Config.Name);
    }

    private string GetDefaultSystemPrompt()
    {
        var tools = Config.Tools.Count > 0
            ? string.Join("\n", Config.Tools.Select(t => $"- {t}"))
            : "无";

        return $"""
你是一个智能游戏助手，请通过思考(Thought)和行动(Action)来帮助玩家。

可用工具：
{tools}

请按以下格式回复：
Thought: [你的思考过程]
Action: [工具名称]([参数])

或者当你有最终答案时：
Thought: [你的思考过程]
Final Answer: [最终答案]
""";
    }

    private (string? Thought, string? Action, string? FinalAnswer) ParseResponse(string response)
    {
        string? thought = null;
        string? action = null;
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
        }

        return (thought, action, finalAnswer);
    }

    private (string ToolName, string ToolInput) ParseAction(string action)
    {
        // 尝试解析: ToolName(param) 或 ToolName param
        var match = Regex.Match(action, @"(\w+)\s*\((.*)\)");
        if (match.Success)
        {
            return (match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim());
        }

        // 尝试无括号格式
        var parts = action.Split(' ', 2);
        if (parts.Length >= 1)
        {
            return (parts[0], parts.Length > 1 ? parts[1] : string.Empty);
        }

        return (action, string.Empty);
    }

    private static IEnumerable<string> ChunkText(string text, int chunkSize)
    {
        for (int i = 0; i < text.Length; i += chunkSize)
        {
            yield return text.Substring(i, Math.Min(chunkSize, text.Length - i));
        }
    }
}
