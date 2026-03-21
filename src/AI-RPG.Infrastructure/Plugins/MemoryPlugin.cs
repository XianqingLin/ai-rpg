using System.Text.Json;
using Microsoft.Extensions.Logging;
using AI_RPG.Infrastructure.Services;

namespace AI_RPG.Infrastructure.Plugins;

/// <summary>
/// 记忆插件 - 为 Semantic Kernel 提供对话历史管理能力
/// </summary>
public class MemoryPlugin
{
    private readonly ICacheService _cache;
    private readonly ILogger<MemoryPlugin> _logger;

    public MemoryPlugin(ICacheService cache, ILogger<MemoryPlugin> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// 加载对话历史
    /// </summary>
    public async Task<List<ChatMessage>> LoadConversationHistoryAsync(
        string sessionId,
        int maxRounds = 10,
        CancellationToken cancellationToken = default)
    {
        var key = $"chat:{sessionId}:history";
        var history = await _cache.GetAsync<List<ChatMessage>>(key, cancellationToken) ?? new List<ChatMessage>();

        // 只返回最近的 N 轮
        var maxMessages = maxRounds * 2; // 每轮包含用户和助手两条消息
        if (history.Count > maxMessages)
        {
            history = history.Skip(history.Count - maxMessages).ToList();
        }

        _logger.LogDebug("Loaded {Count} messages from conversation history for session {SessionId}", 
            history.Count, sessionId);

        return history;
    }

    /// <summary>
    /// 保存对话轮次
    /// </summary>
    public async Task SaveConversationTurnAsync(
        string sessionId,
        string userMessage,
        string assistantMessage,
        CancellationToken cancellationToken = default)
    {
        var key = $"chat:{sessionId}:history";
        var history = await _cache.GetAsync<List<ChatMessage>>(key, cancellationToken) ?? new List<ChatMessage>();

        history.Add(new ChatMessage { Role = "user", Content = userMessage, Timestamp = DateTime.UtcNow });
        history.Add(new ChatMessage { Role = "assistant", Content = assistantMessage, Timestamp = DateTime.UtcNow });

        // 保存到缓存（7天过期）
        await _cache.SetAsync(key, history, TimeSpan.FromDays(7), cancellationToken);

        _logger.LogDebug("Saved conversation turn for session {SessionId}, total messages: {Count}", 
            sessionId, history.Count);
    }

    /// <summary>
    /// 清除对话历史
    /// </summary>
    public async Task ClearConversationHistoryAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        var key = $"chat:{sessionId}:history";
        await _cache.RemoveAsync(key, cancellationToken);

        _logger.LogInformation("Cleared conversation history for session {SessionId}", sessionId);
    }

    /// <summary>
    /// 存储临时上下文数据
    /// </summary>
    public async Task StoreContextAsync(
        string key,
        string value,
        int expirationMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        var fullKey = $"context:{key}";
        await _cache.SetStringAsync(fullKey, value, TimeSpan.FromMinutes(expirationMinutes), cancellationToken);
    }

    /// <summary>
    /// 获取临时上下文数据
    /// </summary>
    public async Task<string?> RetrieveContextAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var fullKey = $"context:{key}";
        return await _cache.GetStringAsync(fullKey, cancellationToken);
    }
}

/// <summary>
/// 聊天消息
/// </summary>
public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
