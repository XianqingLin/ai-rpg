using AI_RPG.Application.DTOs;

namespace AI_RPG.Application.Interfaces;

/// <summary>
/// 用户应用服务接口
/// </summary>
public interface IUserAppService
{
    /// <summary>
    /// 用户注册
    /// </summary>
    Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 用户登录
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有用户列表
    /// </summary>
    Task<IReadOnlyList<UserSummaryDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃用户列表
    /// </summary>
    Task<IReadOnlyList<UserSummaryDto>> GetActiveUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新用户信息
    /// </summary>
    Task<UserDto> UpdateUserAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新用户名
    /// </summary>
    Task<UserDto> UpdateUsernameAsync(string userId, UpdateUsernameRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新邮箱
    /// </summary>
    Task<UserDto> UpdateEmailAsync(string userId, UpdateEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新密码
    /// </summary>
    Task<bool> UpdatePasswordAsync(string userId, UpdatePasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 激活用户
    /// </summary>
    Task<UserDto> ActivateUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 停用用户
    /// </summary>
    Task<UserDto> DeactivateUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除用户
    /// </summary>
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查用户名是否可用
    /// </summary>
    Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查邮箱是否可用
    /// </summary>
    Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default);
}
