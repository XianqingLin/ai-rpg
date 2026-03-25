using AI_RPG.Application.DTOs;
using AI_RPG.Application.Interfaces;
using AI_RPG.Application.Mappings;
using AI_RPG.Domain.Entities;
using AI_RPG.Domain.Repositories;
using AI_RPG.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AI_RPG.Application.Services;

/// <summary>
/// 用户应用服务实现
/// </summary>
public sealed class UserAppService : IUserAppService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserAppService> _logger;

    public UserAppService(
        IUserRepository userRepository,
        ILogger<UserAppService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Registering new user: {Username}", request.Username);

        // 检查用户名是否已存在
        if (await _userRepository.ExistsUsernameAsync(request.Username, cancellationToken))
            throw new InvalidOperationException($"Username '{request.Username}' is already taken");

        // 检查邮箱是否已存在
        if (await _userRepository.ExistsEmailAsync(request.Email, cancellationToken))
            throw new InvalidOperationException($"Email '{request.Email}' is already registered");

        // 哈希密码
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 创建用户
        var user = User.Create(
            request.Username,
            request.Email,
            passwordHash,
            request.DisplayName);

        await _userRepository.AddAsync(user, cancellationToken);

        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

        return user.ToDto();
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("User login attempt: {UsernameOrEmail}", request.UsernameOrEmail);

        // 尝试通过用户名或邮箱查找用户
        User? user = null;
        if (request.UsernameOrEmail.Contains('@'))
        {
            user = await _userRepository.GetByEmailAsync(request.UsernameOrEmail, cancellationToken);
        }
        else
        {
            user = await _userRepository.GetByUsernameAsync(request.UsernameOrEmail, cancellationToken);
        }

        if (user == null)
        {
            _logger.LogWarning("Login failed: user not found - {UsernameOrEmail}", request.UsernameOrEmail);
            return new LoginResponse
            {
                Success = false,
                Message = "Invalid username/email or password"
            };
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: user is inactive - {UserId}", user.Id);
            return new LoginResponse
            {
                Success = false,
                Message = "Account is deactivated"
            };
        }

        // 验证密码
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: invalid password - {UserId}", user.Id);
            return new LoginResponse
            {
                Success = false,
                Message = "Invalid username/email or password"
            };
        }

        // 记录登录
        user.RecordLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

        return new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            User = user.ToDto()
        };
    }

    public async Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        var user = await _userRepository.GetByIdAsync(new UserId(userId), cancellationToken);
        return user?.ToDto();
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);

        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        return user?.ToDto();
    }

    public async Task<IReadOnlyList<UserSummaryDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(u => u.ToSummaryDto()).ToList();
    }

    public async Task<IReadOnlyList<UserSummaryDto>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetActiveUsersAsync(cancellationToken);
        return users.Select(u => u.ToSummaryDto()).ToList();
    }

    public async Task<UserDto> UpdateUserAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Updating user info: {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(new UserId(userId), cancellationToken)
            ?? throw new InvalidOperationException($"User {userId} not found");

        user.UpdateInfo(request.DisplayName, request.AvatarUrl);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User info updated: {UserId}", userId);

        return user.ToDto();
    }

    public async Task<UserDto> UpdateUsernameAsync(string userId, UpdateUsernameRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Updating username for user: {UserId}", userId);

        // 检查新用户名是否已存在
        if (await _userRepository.ExistsUsernameAsync(request.NewUsername, cancellationToken))
            throw new InvalidOperationException($"Username '{request.NewUsername}' is already taken");

        var user = await _userRepository.GetByIdAsync(new UserId(userId), cancellationToken)
            ?? throw new InvalidOperationException($"User {userId} not found");

        user.UpdateUsername(request.NewUsername);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Username updated for user: {UserId}", userId);

        return user.ToDto();
    }

    public async Task<UserDto> UpdateEmailAsync(string userId, UpdateEmailRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Updating email for user: {UserId}", userId);

        // 检查新邮箱是否已存在
        if (await _userRepository.ExistsEmailAsync(request.NewEmail, cancellationToken))
            throw new InvalidOperationException($"Email '{request.NewEmail}' is already registered");

        var user = await _userRepository.GetByIdAsync(new UserId(userId), cancellationToken)
            ?? throw new InvalidOperationException($"User {userId} not found");

        user.UpdateEmail(request.NewEmail);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Email updated for user: {UserId}", userId);

        return user.ToDto();
    }

    public async Task<bool> UpdatePasswordAsync(string userId, UpdatePasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Updating password for user: {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(new UserId(userId), cancellationToken)
            ?? throw new InvalidOperationException($"User {userId} not found");

        // 验证当前密码
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Password update failed: invalid current password - {UserId}", userId);
            return false;
        }

        // 更新密码
        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatePassword(newPasswordHash);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Password updated for user: {UserId}", userId);
        return true;
    }

    public async Task<UserDto> ActivateUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        _logger.LogInformation("Activating user: {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(new UserId(userId), cancellationToken)
            ?? throw new InvalidOperationException($"User {userId} not found");

        user.Activate();
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User activated: {UserId}", userId);

        return user.ToDto();
    }

    public async Task<UserDto> DeactivateUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        _logger.LogInformation("Deactivating user: {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(new UserId(userId), cancellationToken)
            ?? throw new InvalidOperationException($"User {userId} not found");

        user.Deactivate();
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User deactivated: {UserId}", userId);

        return user.ToDto();
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        _logger.LogInformation("Deleting user: {UserId}", userId);

        await _userRepository.DeleteAsync(new UserId(userId), cancellationToken);

        _logger.LogInformation("User deleted: {UserId}", userId);
    }

    public async Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);

        return !await _userRepository.ExistsUsernameAsync(username, cancellationToken);
    }

    public async Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(email);

        return !await _userRepository.ExistsEmailAsync(email, cancellationToken);
    }
}
