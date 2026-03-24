using System.Net.Http.Json;
using AI_RPG.Application.DTOs;

namespace AI_RPG.Blazor.Services;

public class DialogueService
{
    private readonly HttpClient _httpClient;

    public DialogueService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // 发送消息
    public async Task<DialogueResponseDto?> SendMessageAsync(SendMessageRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/sessions/{request.SessionId}/dialogue", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DialogueResponseDto>();
                return result;
            }
            else
            {
                // 读取错误内容
                var errorContent = await response.Content.ReadAsStringAsync();
                return new DialogueResponseDto
                {
                    Success = false,
                    ErrorMessage = $"HTTP {(int)response.StatusCode}: {errorContent}"
                };
            }
        }
        catch (Exception ex)
        {
            return new DialogueResponseDto
            {
                Success = false,
                ErrorMessage = $"请求异常: {ex.Message}"
            };
        }
    }

    // 发送消息（简化版）
    public async Task<DialogueResponseDto?> SendMessageAsync(string sessionId, string playerId, string message)
    {
        var request = new SendMessageRequest
        {
            SessionId = sessionId,
            PlayerId = playerId,
            Message = message
        };
        return await SendMessageAsync(request);
    }

    // 获取对话历史
    public async Task<List<DialogueTurnDto>> GetHistoryAsync(string sessionId, int count = 50)
    {
        var result = await _httpClient.GetFromJsonAsync<List<DialogueTurnDto>>($"api/sessions/{sessionId}/dialogue/history?count={count}");
        return result ?? new List<DialogueTurnDto>();
    }

    // 获取对话历史（使用请求对象）
    public async Task<List<DialogueTurnDto>> GetHistoryAsync(GetHistoryRequest request)
    {
        return await GetHistoryAsync(request.SessionId, request.Count);
    }

    // 流式发送消息（SSE）
    public async IAsyncEnumerable<string> StreamMessageAsync(SendMessageRequest request)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"api/sessions/{request.SessionId}/dialogue/stream")
        {
            Content = JsonContent.Create(request)
        };

        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        
        if (response.IsSuccessStatusCode)
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    yield return line;
                }
            }
        }
    }

    // 流式发送消息（简化版）
    public IAsyncEnumerable<string> StreamMessageAsync(string sessionId, string playerId, string message)
    {
        var request = new SendMessageRequest
        {
            SessionId = sessionId,
            PlayerId = playerId,
            Message = message
        };
        return StreamMessageAsync(request);
    }
}
