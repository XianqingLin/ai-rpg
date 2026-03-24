using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using AI_RPG.AICapabilities.LLM;
using AI_RPG.AICapabilities.Strategies.Models;

namespace AI_RPG.AICapabilities.Strategies;

/// <summary>
/// 反思循环生成策略
/// </summary>
public sealed class ReflectionStrategy : IGenerationStrategy
{
    public string Name => "Reflection";
    public string Description => "生成-反思-改进循环策略，通过自我反思提升输出质量";

    private readonly ILLMClient _llmClient;
    private readonly ITokenManager _tokenManager;
    private readonly ILogger<ReflectionStrategy> _logger;
    private readonly ReflectionConfig _config;

    public ReflectionStrategy(
        ILLMClient llmClient,
        ITokenManager tokenManager,
        ILogger<ReflectionStrategy> logger,
        ReflectionConfig? config = null)
    {
        _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config ?? new ReflectionConfig();
    }

    public async Task<StrategyResult> ExecuteAsync(
        string input,
        StrategyContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var stopwatch = Stopwatch.StartNew();
        var totalTokens = 0;
        var reflectionCount = 0;
        var reflections = new List<ReflectionResult>();

        try
        {
            // 初始生成
            _logger.LogDebug("Generating initial content");
            var currentContent = await GenerateInitialAsync(input, context, cancellationToken);
            totalTokens += _tokenManager.EstimateTokens(input) + _tokenManager.EstimateTokens(currentContent);

            // 反思循环
            while (reflectionCount < _config.MaxReflections)
            {
                reflectionCount++;
                _logger.LogDebug("Reflection iteration {Iteration}", reflectionCount);

                var reflection = await ReflectAsync(currentContent, cancellationToken);
                totalTokens += _tokenManager.EstimateTokens(currentContent) + _tokenManager.EstimateTokens(reflection.Reflection);
                reflections.Add(reflection);

                // 如果评分足够高，不需要改进
                if (reflection.Score >= _config.MinAcceptableScore && !reflection.NeedsImprovement)
                {
                    _logger.LogDebug("Content quality is satisfactory, score: {Score}", reflection.Score);
                    break;
                }

                // 如果需要改进且还有迭代次数
                if (reflection.NeedsImprovement && reflectionCount < _config.MaxReflections)
                {
                    _logger.LogDebug("Improving content based on reflection");
                    currentContent = await ImproveAsync(input, currentContent, reflection, cancellationToken);
                    totalTokens += _tokenManager.EstimateTokens(currentContent);
                }
                else
                {
                    break;
                }
            }

            stopwatch.Stop();

            return new StrategyResult
            {
                IsSuccess = true,
                Content = currentContent,
                TokenUsed = totalTokens,
                StepCount = reflectionCount,
                ExecutionTime = stopwatch.Elapsed,
                Metadata = new Dictionary<string, object?>
                {
                    ["reflections"] = reflections,
                    ["finalScore"] = reflections.LastOrDefault()?.Score ?? 0,
                    ["strategy"] = Name
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error executing Reflection strategy");

            return new StrategyResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message,
                TokenUsed = totalTokens,
                StepCount = reflectionCount,
                ExecutionTime = stopwatch.Elapsed,
                Metadata = new Dictionary<string, object?> { ["reflections"] = reflections }
            };
        }
    }

    private async Task<string> GenerateInitialAsync(string input, StrategyContext context, CancellationToken cancellationToken)
    {
        var prompt = $"""
请根据以下要求生成内容：

{input}

请直接输出内容，不需要额外解释。
""";

        return await _llmClient.SendMessageAsync(prompt, cancellationToken: cancellationToken);
    }

    private async Task<ReflectionResult> ReflectAsync(string content, CancellationToken cancellationToken)
    {
        var prompt = _config.ReflectionPromptTemplate.Replace("{{content}}", content);

        var response = await _llmClient.SendMessageAsync(prompt, cancellationToken: cancellationToken);

        // 尝试解析JSON响应
        try
        {
            // 提取JSON部分
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response[jsonStart..(jsonEnd + 1)];
                var result = JsonSerializer.Deserialize<ReflectionJsonResult>(json);
                
                if (result != null)
                {
                    return new ReflectionResult
                    {
                        NeedsImprovement = result.NeedsImprovement,
                        Reflection = result.Reflection ?? "无反思内容",
                        Suggestions = result.Suggestions ?? [],
                        Score = Math.Clamp(result.Score, 1, 10)
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse reflection JSON, using fallback");
        }

        // 回退：简单解析
        return ParseFallbackReflection(response);
    }

    private async Task<string> ImproveAsync(
        string originalInput,
        string currentContent,
        ReflectionResult reflection,
        CancellationToken cancellationToken)
    {
        var suggestions = string.Join("\n", reflection.Suggestions.Select((s, i) => $"{i + 1}. {s}"));

        var prompt = $"""
请根据以下反思和建议改进内容。

原始要求：
{originalInput}

当前内容：
{currentContent}

反思：
{reflection.Reflection}

改进建议：
{reflection.Suggestions}

请输出改进后的内容：
""";

        return await _llmClient.SendMessageAsync(prompt, cancellationToken: cancellationToken);
    }

    private ReflectionResult ParseFallbackReflection(string response)
    {
        var score = 5;
        var needsImprovement = false;

        // 尝试提取评分
        var scoreMatch = System.Text.RegularExpressions.Regex.Match(response, @"score[""']?\s*[:=]\s*(\d+)");
        if (scoreMatch.Success && int.TryParse(scoreMatch.Groups[1].Value, out var parsedScore))
        {
            score = Math.Clamp(parsedScore, 1, 10);
        }

        // 尝试判断是否需要改进
        if (response.Contains("需要改进", StringComparison.OrdinalIgnoreCase) ||
            response.Contains("needs improvement", StringComparison.OrdinalIgnoreCase) ||
            score < _config.MinAcceptableScore)
        {
            needsImprovement = true;
        }

        return new ReflectionResult
        {
            NeedsImprovement = needsImprovement,
            Reflection = response,
            Suggestions = [],
            Score = score
        };
    }

    private class ReflectionJsonResult
    {
        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("needsImprovement")]
        public bool NeedsImprovement { get; set; }

        [JsonPropertyName("reflection")]
        public string? Reflection { get; set; }

        [JsonPropertyName("suggestions")]
        public List<string>? Suggestions { get; set; }
    }
}
