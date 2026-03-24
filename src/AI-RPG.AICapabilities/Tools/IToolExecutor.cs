using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AI_RPG.AICapabilities.Tools;

/// <summary>
/// 工具执行器接口
/// </summary>
public interface IToolExecutor
{
    /// <summary>
    /// 执行工具
    /// </summary>
    /// <param name="toolName">工具名称</param>
    /// <param name="parameters">参数（JSON字符串或键值对）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行结果</returns>
    Task<string> ExecuteAsync(
        string toolName,
        string parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行工具（参数字典版本）
    /// </summary>
    Task<string> ExecuteAsync(
        string toolName,
        IDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量执行工具
    /// </summary>
    Task<IReadOnlyList<ToolExecutionResult>> ExecuteBatchAsync(
        IReadOnlyList<ToolExecutionRequest> requests,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 工具执行请求
/// </summary>
public sealed class ToolExecutionRequest
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public required string ToolName { get; init; }

    /// <summary>
    /// 参数
    /// </summary>
    public required IDictionary<string, object?> Parameters { get; init; }

    /// <summary>
    /// 请求ID
    /// </summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();
}

/// <summary>
/// 工具执行结果
/// </summary>
public sealed class ToolExecutionResult
{
    /// <summary>
    /// 请求ID
    /// </summary>
    public required string RequestId { get; init; }

    /// <summary>
    /// 工具名称
    /// </summary>
    public required string ToolName { get; init; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// 结果内容
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 执行时间
    /// </summary>
    public TimeSpan ExecutionTime { get; init; }

    /// <summary>
    /// 执行时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// 工具执行器实现
/// </summary>
public sealed class ToolExecutor : IToolExecutor
{
    private readonly IToolRegistry _toolRegistry;
    private readonly ILogger<ToolExecutor> _logger;

    public ToolExecutor(
        IToolRegistry toolRegistry,
        ILogger<ToolExecutor> logger)
    {
        _toolRegistry = toolRegistry ?? throw new ArgumentNullException(nameof(toolRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> ExecuteAsync(
        string toolName,
        string parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolName);

        // 尝试解析JSON参数
        IDictionary<string, object?> paramDict;
        try
        {
            if (string.IsNullOrWhiteSpace(parameters))
            {
                paramDict = new Dictionary<string, object?>();
            }
            else
            {
                var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(parameters);
                paramDict = json?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => ConvertJsonElement(kvp.Value) as object
                ) ?? new Dictionary<string, object?>();
            }
        }
        catch (JsonException)
        {
            // 如果不是JSON，作为单个参数处理
            paramDict = new Dictionary<string, object?>
            {
                ["input"] = parameters
            };
        }

        return await ExecuteAsync(toolName, paramDict, cancellationToken);
    }

    public async Task<string> ExecuteAsync(
        string toolName,
        IDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolName);
        ArgumentNullException.ThrowIfNull(parameters);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogDebug("Executing tool '{ToolName}'", toolName);

            var tool = _toolRegistry.GetTool(toolName);
            if (tool == null)
            {
                throw new KeyNotFoundException($"Tool '{toolName}' not found");
            }

            var result = await tool.ExecuteAsync(parameters, cancellationToken);
            
            stopwatch.Stop();
            _logger.LogDebug("Tool '{ToolName}' executed successfully in {ElapsedMs}ms", 
                toolName, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error executing tool '{ToolName}'", toolName);
            throw;
        }
    }

    public async Task<IReadOnlyList<ToolExecutionResult>> ExecuteBatchAsync(
        IReadOnlyList<ToolExecutionRequest> requests,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requests);

        var results = new List<ToolExecutionResult>();
        var tasks = requests.Select(async request =>
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var content = await ExecuteAsync(
                    request.ToolName, 
                    request.Parameters, 
                    cancellationToken);
                
                stopwatch.Stop();

                return new ToolExecutionResult
                {
                    RequestId = request.RequestId,
                    ToolName = request.ToolName,
                    IsSuccess = true,
                    Content = content,
                    ExecutionTime = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                return new ToolExecutionResult
                {
                    RequestId = request.RequestId,
                    ToolName = request.ToolName,
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    ExecutionTime = stopwatch.Elapsed
                };
            }
        });

        results.AddRange(await Task.WhenAll(tasks));
        return results;
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToList(),
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value)),
            _ => element.ToString()
        };
    }
}
