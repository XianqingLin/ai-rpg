using AI_RPG.Domain.ValueObjects;

namespace AI_RPG.Domain.Entities;

public sealed class User
{
    public UserId Id { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// 显示名称（可选，默认为用户名）
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>
    /// 头像URL
    /// </summary>
    public string? AvatarUrl { get; private set; }

    private User()
    {
        Id = null!; // 使用 nullable 禁用警告
        Username = null!;
        Email = null!;
        PasswordHash = null!;
    }

    public User(
        UserId id,
        string username,
        string email,
        string passwordHash,
        string? displayName = null,
        string? avatarUrl = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Username = ValidateUsername(username);
        Email = ValidateEmail(email);
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        DisplayName = displayName;
        AvatarUrl = avatarUrl;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public static User Create(
        string username,
        string email,
        string passwordHash,
        string? displayName = null,
        string? avatarUrl = null)
    {
        return new User(
            UserId.New(),
            username,
            email,
            passwordHash,
            displayName,
            avatarUrl);
    }

    /// <summary>
    /// 从数据库重建用户（供仓储使用）
    /// </summary>
    public static User Reconstitute(
        UserId id,
        string username,
        string email,
        string passwordHash,
        string? displayName,
        string? avatarUrl,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        DateTime? lastLoginAt)
    {
        return new User
        {
            Id = id,
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            DisplayName = displayName,
            AvatarUrl = avatarUrl,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            LastLoginAt = lastLoginAt
        };
    }

    public void UpdateInfo(string? displayName = null, string? avatarUrl = null)
    {
        if (displayName != null)
            DisplayName = displayName;

        if (avatarUrl != null)
            AvatarUrl = avatarUrl;

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateUsername(string newUsername)
    {
        Username = ValidateUsername(newUsername);
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateEmail(string newEmail)
    {
        Email = ValidateEmail(newEmail);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public string GetDisplayName() => DisplayName ?? Username;

    private static string ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));

        if (username.Length < 3 || username.Length > 50)
            throw new ArgumentException("Username must be between 3 and 50 characters", nameof(username));

        return username.Trim();
    }

    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        // 基础邮箱格式验证
        var trimmedEmail = email.Trim().ToLowerInvariant();
        if (!trimmedEmail.Contains('@') || !trimmedEmail.Contains('.'))
            throw new ArgumentException("Invalid email format", nameof(email));

        return trimmedEmail;
    }
}
