using AI_RPG.Application.DTOs;
using AI_RPG.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AI_RPG.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserAppService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserAppService userService,
        ILogger<UsersController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region 注册与登录

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.RegisterAsync(request, cancellationToken);
            _logger.LogInformation("User registered: {UserId}", user.Id);
            return CreatedAtAction(nameof(GetUser), new { userId = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Registration failed: {Message}", ex.Message);
            return Conflict(new { Error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Registration failed: {Message}", ex.Message);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _userService.LoginAsync(request, cancellationToken);
            if (response.Success)
            {
                _logger.LogInformation("User logged in: {UserId}", response.User?.Id);
                return Ok(response);
            }
            return Unauthorized(new { Error = response.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            return BadRequest(new { Error = ex.Message });
        }
    }

    #endregion

    #region 查询操作

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetUser(
        string userId,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserAsync(userId, cancellationToken);
        if (user == null)
        {
            return NotFound(new { Error = $"User {userId} not found" });
        }
        return Ok(user);
    }

    [HttpGet("username/{username}")]
    public async Task<ActionResult<UserDto>> GetUserByUsername(
        string username,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByUsernameAsync(username, cancellationToken);
        if (user == null)
        {
            return NotFound(new { Error = $"User with username '{username}' not found" });
        }
        return Ok(user);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserSummaryDto>>> GetAllUsers(
        CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IReadOnlyList<UserSummaryDto>>> GetActiveUsers(
        CancellationToken cancellationToken)
    {
        var users = await _userService.GetActiveUsersAsync(cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// 检查用户名是否可用
    /// </summary>
    [HttpGet("check-username/{username}")]
    public async Task<ActionResult<object>> CheckUsername(
        string username,
        CancellationToken cancellationToken)
    {
        var isAvailable = await _userService.IsUsernameAvailableAsync(username, cancellationToken);
        return Ok(new { Username = username, IsAvailable = isAvailable });
    }

    /// <summary>
    /// 检查邮箱是否可用
    /// </summary>
    [HttpGet("check-email/{email}")]
    public async Task<ActionResult<object>> CheckEmail(
        string email,
        CancellationToken cancellationToken)
    {
        var isAvailable = await _userService.IsEmailAvailableAsync(email, cancellationToken);
        return Ok(new { Email = email, IsAvailable = isAvailable });
    }

    #endregion

    #region 更新操作

    /// <summary>
    /// 更新用户信息（显示名称、头像）
    /// </summary>
    [HttpPut("{userId}")]
    public async Task<ActionResult<UserDto>> UpdateUser(
        string userId,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(userId, request, cancellationToken);
            _logger.LogInformation("User updated: {UserId}", userId);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user {UserId}", userId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 更新用户名
    /// </summary>
    [HttpPut("{userId}/username")]
    public async Task<ActionResult<UserDto>> UpdateUsername(
        string userId,
        [FromBody] UpdateUsernameRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.UpdateUsernameAsync(userId, request, cancellationToken);
            _logger.LogInformation("Username updated for user: {UserId}", userId);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("already taken"))
            {
                return Conflict(new { Error = ex.Message });
            }
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update username for user {UserId}", userId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 更新邮箱
    /// </summary>
    [HttpPut("{userId}/email")]
    public async Task<ActionResult<UserDto>> UpdateEmail(
        string userId,
        [FromBody] UpdateEmailRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.UpdateEmailAsync(userId, request, cancellationToken);
            _logger.LogInformation("Email updated for user: {UserId}", userId);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("already registered"))
            {
                return Conflict(new { Error = ex.Message });
            }
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update email for user {UserId}", userId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 更新密码
    /// </summary>
    [HttpPut("{userId}/password")]
    public async Task<IActionResult> UpdatePassword(
        string userId,
        [FromBody] UpdatePasswordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _userService.UpdatePasswordAsync(userId, request, cancellationToken);
            if (success)
            {
                _logger.LogInformation("Password updated for user: {UserId}", userId);
                return NoContent();
            }
            return BadRequest(new { Error = "Current password is incorrect" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update password for user {UserId}", userId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    #endregion

    #region 状态管理

    /// <summary>
    /// 激活用户
    /// </summary>
    [HttpPost("{userId}/activate")]
    public async Task<ActionResult<UserDto>> ActivateUser(
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.ActivateUserAsync(userId, cancellationToken);
            _logger.LogInformation("User activated: {UserId}", userId);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 停用用户
    /// </summary>
    [HttpPost("{userId}/deactivate")]
    public async Task<ActionResult<UserDto>> DeactivateUser(
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.DeactivateUserAsync(userId, cancellationToken);
            _logger.LogInformation("User deactivated: {UserId}", userId);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    #endregion

    #region 删除操作

    /// <summary>
    /// 删除用户
    /// </summary>
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _userService.DeleteUserAsync(userId, cancellationToken);
            _logger.LogInformation("User deleted: {UserId}", userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user {UserId}", userId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    #endregion
}
