using AI_RPG.Application.DTOs;

namespace AI_RPG.Application.Interfaces;

/// <summary>
/// 会话应用服务接口
/// </summary>
public interface ISessionAppService
{
    /// <summary>
    /// 创建会话
    /// </summary>
    Task<SessionDto> CreateSessionAsync(CreateSessionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取会话详情
    /// </summary>
    Task<SessionDto?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的会话列表
    /// </summary>
    Task<IReadOnlyList<SessionSummaryDto>> GetUserSessionsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 玩家加入会话
    /// </summary>
    Task<SessionDto> JoinSessionAsync(JoinSessionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加 NPC
    /// </summary>
    Task<NPCDto> AddNPCAsync(string sessionId, AddNPCRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 移除 NPC
    /// </summary>
    Task RemoveNPCAsync(string sessionId, string npcId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 切换场景
    /// </summary>
    Task<SessionDto> SwitchSceneAsync(string sessionId, SwitchSceneRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 开始会话
    /// </summary>
    Task<SessionDto> StartSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 暂停会话
    /// </summary>
    Task<SessionDto> PauseSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 结束会话
    /// </summary>
    Task EndSessionAsync(string sessionId, CancellationToken cancellationToken = default);
}
