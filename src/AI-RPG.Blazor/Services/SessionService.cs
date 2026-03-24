using System.Net.Http.Json;
using AI_RPG.Application.DTOs;

namespace AI_RPG.Blazor.Services;

public class SessionService
{
    private readonly HttpClient _httpClient;

    public SessionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // 创建会话
    public async Task<SessionDto?> CreateAsync(CreateSessionRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/sessions", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SessionDto>();
        }
        return null;
    }

    // 获取会话详情
    public async Task<SessionDto?> GetAsync(string sessionId)
    {
        return await _httpClient.GetFromJsonAsync<SessionDto>($"api/sessions/{sessionId}");
    }

    // 获取用户会话列表
    public async Task<List<SessionSummaryDto>> GetUserSessionsAsync(string userId)
    {
        var result = await _httpClient.GetFromJsonAsync<List<SessionSummaryDto>>($"api/sessions/user/{userId}");
        return result ?? new List<SessionSummaryDto>();
    }

    // 玩家加入会话
    public async Task<SessionDto?> JoinAsync(string sessionId, JoinSessionRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/sessions/{sessionId}/join", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SessionDto>();
        }
        return null;
    }

    // 开始会话
    public async Task<SessionDto?> StartAsync(string sessionId)
    {
        var response = await _httpClient.PostAsync($"api/sessions/{sessionId}/start", null);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SessionDto>();
        }
        return null;
    }

    // 暂停会话
    public async Task<SessionDto?> PauseAsync(string sessionId)
    {
        var response = await _httpClient.PostAsync($"api/sessions/{sessionId}/pause", null);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SessionDto>();
        }
        return null;
    }

    // 结束会话
    public async Task<bool> EndAsync(string sessionId)
    {
        var response = await _httpClient.PostAsync($"api/sessions/{sessionId}/end", null);
        return response.IsSuccessStatusCode;
    }

    // 添加 NPC
    public async Task<NPCDto?> AddNPCAsync(string sessionId, AddNPCRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/sessions/{sessionId}/npcs", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<NPCDto>();
        }
        return null;
    }

    // 移除 NPC
    public async Task<bool> RemoveNPCAsync(string sessionId, string npcId)
    {
        var response = await _httpClient.DeleteAsync($"api/sessions/{sessionId}/npcs/{npcId}");
        return response.IsSuccessStatusCode;
    }

    // 切换场景
    public async Task<SessionDto?> SwitchSceneAsync(string sessionId, SwitchSceneRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/sessions/{sessionId}/scene", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SessionDto>();
        }
        return null;
    }
}
