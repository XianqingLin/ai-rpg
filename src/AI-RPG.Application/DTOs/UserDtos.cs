namespace AI_RPG.Application.DTOs;

/// <summary>
/// 用户 DTO
/// </summary>
public sealed class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// 用户摘要 DTO
/// </summary>
public sealed class UserSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// 注册请求
/// </summary>
public sealed class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}

/// <summary>
/// 登录请求
/// </summary>
public sealed class LoginRequest
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 登录响应
/// </summary>
public sealed class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserDto? User { get; set; }
}

/// <summary>
/// 更新用户信息请求
/// </summary>
public sealed class UpdateUserRequest
{
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
}

/// <summary>
/// 更新用户名请求
/// </summary>
public sealed class UpdateUsernameRequest
{
    public string NewUsername { get; set; } = string.Empty;
}

/// <summary>
/// 更新邮箱请求
/// </summary>
public sealed class UpdateEmailRequest
{
    public string NewEmail { get; set; } = string.Empty;
}

/// <summary>
/// 更新密码请求
/// </summary>
public sealed class UpdatePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
