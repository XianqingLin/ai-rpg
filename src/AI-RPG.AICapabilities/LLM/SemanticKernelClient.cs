using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AI_RPG.AICapabilities.LLM;

/// <summary>
/// 基于Semantic Kernel的LLM客户端实现
/// </summary>
public sealed class SemanticKernelClient : ILLMClient
{
    private readonly ILogger<SemanticKernelClient> _logger;

    public string ClientName => "SemanticKernel";

    public string ModelName { get; }

    public Kernel Kernel { get; }

    public SemanticKernelClient(
        Kernel kernel,
        string modelName,
        ILogger<SemanticKernelClient> logger)
    {
        Kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        ModelName = modelName ?? throw new ArgumentNullException(nameof(modelName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IChatCompletionService GetChatCompletionService()
    {
        return Kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> SendMessageAsync(
        string message,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(message);

        return await SendChatAsync(chatHistory, executionSettings, cancellationToken);
    }

    public async Task<string> SendChatAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chatHistory);

        try
        {
            _logger.LogDebug("Sending chat request to {Model}", ModelName);

            var chatService = GetChatCompletionService();
            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                Kernel,
                cancellationToken);

            var content = response.Content ?? string.Empty;

            _logger.LogDebug("Received response from {Model}, length: {Length}", 
                ModelName, content.Length);

            return content;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling LLM for model {Model}", ModelName);
            throw;
        }
    }

    public IAsyncEnumerable<StreamingChatMessageContent> SendStreamingAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chatHistory);

        try
        {
            _logger.LogDebug("Starting streaming chat request to {Model}", ModelName);

            var chatService = GetChatCompletionService();
            return chatService.GetStreamingChatMessageContentsAsync(
                chatHistory,
                executionSettings,
                Kernel,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting streaming LLM request for model {Model}", ModelName);
            throw;
        }
    }
}
