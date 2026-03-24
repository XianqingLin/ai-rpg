using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AI_RPG.AICapabilities.LLM;

/// <summary>
/// LLM客户端接口 - 提供统一的LLM调用抽象
/// </summary>
public interface ILLMClient
{
    /// <summary>
    /// 客户端名称
    /// </summary>
    string ClientName { get; }

    /// <summary>
    /// 模型名称
    /// </summary>
    string ModelName { get; }

    /// <summary>
    /// 获取Semantic Kernel实例
    /// </summary>
    Kernel Kernel { get; }

    /// <summary>
    /// 获取聊天完成服务
    /// </summary>
    IChatCompletionService GetChatCompletionService();

    /// <summary>
    /// 发送单轮消息并获取回复
    /// </summary>
    /// <param name="message">用户消息</param>
    /// <param name="executionSettings">执行设置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>AI回复内容</returns>
    Task<string> SendMessageAsync(
        string message,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送聊天历史并获取回复
    /// </summary>
    /// <param name="chatHistory">聊天历史</param>
    /// <param name="executionSettings">执行设置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>AI回复内容</returns>
    Task<string> SendChatAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 流式发送消息
    /// </summary>
    /// <param name="chatHistory">聊天历史</param>
    /// <param name="executionSettings">执行设置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>流式回复片段</returns>
    IAsyncEnumerable<StreamingChatMessageContent> SendStreamingAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// LLM响应结果
/// </summary>
public sealed class LLMResponse
{
    /// <summary>
    /// 回复内容
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// 使用的Token数（输入）
    /// </summary>
    public int InputTokens { get; init; }

    /// <summary>
    /// 使用的Token数（输出）
    /// </summary>
    public int OutputTokens { get; init; }

    /// <summary>
    /// 总Token数
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;

    /// <summary>
    /// 模型名称
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// 完成原因
    /// </summary>
    public string? FinishReason { get; init; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; init; }
}
