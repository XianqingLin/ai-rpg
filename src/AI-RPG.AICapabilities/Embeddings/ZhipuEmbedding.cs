using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI_RPG.AICapabilities.Embeddings;

/// <summary>
/// 智谱AI Embedding实现
/// </summary>
public sealed class ZhipuEmbedding : IEmbeddingProvider, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ZhipuEmbedding> _logger;
    private readonly ZhipuEmbeddingOptions _options;

    private const string DefaultBaseUrl = "https://open.bigmodel.cn/api/paas/v4";
    private const string EmbeddingEndpoint = "/embeddings";

    public string ProviderName => "Zhipu";

    public int Dimensions => _options.Dimensions;

    public ZhipuEmbedding(
        HttpClient httpClient,
        IOptions<ZhipuEmbeddingOptions> options,
        ILogger<ZhipuEmbedding> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.BaseUrl ?? DefaultBaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var results = await GenerateEmbeddingsAsync([text], cancellationToken);
        return results.Count > 0 ? results[0] : throw new InvalidOperationException("Failed to generate embedding");
    }

    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(texts);

        if (texts.Count == 0)
            return [];

        try
        {
            var request = new ZhipuEmbeddingRequest
            {
                Model = _options.Model,
                Input = texts.ToList(),
                Dimensions = _options.Dimensions
            };

            var json = JsonSerializer.Serialize(request, ZhipuJsonContext.Default.ZhipuEmbeddingRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug("Sending embedding request to Zhipu API, texts count: {Count}, dimensions: {Dimensions}",
                texts.Count, _options.Dimensions);

            var response = await _httpClient.PostAsync(EmbeddingEndpoint, content, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Zhipu Embedding API error: {StatusCode}, {Response}",
                    response.StatusCode, responseJson);
                throw new HttpRequestException($"Embedding API error: {response.StatusCode}");
            }

            var apiResponse = JsonSerializer.Deserialize(responseJson, ZhipuJsonContext.Default.ZhipuEmbeddingResponse);

            if (apiResponse?.Data is null || apiResponse.Data.Count == 0)
            {
                throw new InvalidOperationException("Empty embedding response from API");
            }

            // 按索引排序确保顺序一致
            var embeddings = apiResponse.Data
                .OrderBy(d => d.Index)
                .Select(d => d.Embedding?.ToArray() ?? [])
                .ToList();

            _logger.LogDebug("Successfully generated {Count} embeddings", embeddings.Count);

            return embeddings;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Zhipu Embedding API");
            throw;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// API请求/响应模型

public sealed class ZhipuEmbeddingRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public List<string> Input { get; set; } = [];

    [JsonPropertyName("dimensions")]
    public int Dimensions { get; set; } = 512;
}

public sealed class ZhipuEmbeddingResponse
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("data")]
    public List<ZhipuEmbeddingData>? Data { get; set; }

    [JsonPropertyName("usage")]
    public ZhipuEmbeddingUsage? Usage { get; set; }
}

public sealed class ZhipuEmbeddingData
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("embedding")]
    public List<float>? Embedding { get; set; }
}

public sealed class ZhipuEmbeddingUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

[JsonSerializable(typeof(ZhipuEmbeddingRequest))]
[JsonSerializable(typeof(ZhipuEmbeddingResponse))]
public partial class ZhipuJsonContext : JsonSerializerContext { }
