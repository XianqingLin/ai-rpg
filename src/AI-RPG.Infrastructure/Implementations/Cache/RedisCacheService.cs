using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using AI_RPG.Infrastructure.Services;

namespace AI_RPG.Infrastructure.Implementations.Cache;

/// <summary>
/// Redis 缓存服务实现
/// </summary>
public sealed class RedisCacheService : ICacheService, IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly RedisOptions _options;
    private readonly string? _keyPrefix;

    /// <summary>
    /// 初始化 Redis 缓存服务
    /// </summary>
    public RedisCacheService(IOptions<RedisOptions> options, ILogger<RedisCacheService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _keyPrefix = _options.KeyPrefix;

        var configurationOptions = new ConfigurationOptions
        {
            EndPoints = { _options.ConnectionString },
            DefaultDatabase = _options.DefaultDatabase,
            ConnectTimeout = _options.ConnectTimeout,
            SyncTimeout = _options.SyncTimeout,
            AsyncTimeout = _options.AsyncTimeout,
            Ssl = _options.Ssl,
            SslHost = _options.SslHost,
            ConnectRetry = _options.ConnectRetry,
            AbortOnConnectFail = false
        };

        if (!string.IsNullOrEmpty(_options.Username))
        {
            configurationOptions.User = _options.Username;
        }

        if (!string.IsNullOrEmpty(_options.Password))
        {
            configurationOptions.Password = _options.Password;
        }

        _redis = ConnectionMultiplexer.Connect(configurationOptions);
        _db = _redis.GetDatabase();

        _logger.LogInformation("Redis cache service initialized: {ConnectionString}", _options.ConnectionString);
    }

    #region 字符串操作

    public async Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var value = await _db.StringGetAsync(fullKey);
        return value.IsNull ? null : value.ToString();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var json = await GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize cache value: {Key}", key);
            throw;
        }
    }

    public async Task SetStringAsync(string key, string value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var expiry = expiration ?? _options.DefaultExpiration;

        await _db.StringSetAsync(fullKey, value, expiry);
        _logger.LogDebug("Set string in Redis: {Key}", key);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await SetStringAsync(key, json, expiration, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serialize and set cache value: {Key}", key);
            throw;
        }
    }

    #endregion

    #region 删除和过期

    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        return await _db.KeyDeleteAsync(fullKey);
    }

    public async Task<long> RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var fullKeys = keys.Select(GetFullKey).Select(k => (RedisKey)k).ToArray();
        if (fullKeys.Length == 0) return 0;

        return await _db.KeyDeleteAsync(fullKeys);
    }

    public async Task<bool> SetExpirationAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        return await _db.KeyExpireAsync(fullKey, expiration);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        return await _db.KeyExistsAsync(fullKey);
    }

    #endregion

    #region 原子操作

    public async Task<bool> SetIfNotExistsAsync(string key, string value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var expiry = expiration ?? _options.DefaultExpiration;

        return await _db.StringSetAsync(fullKey, value, expiry, When.NotExists);
    }

    public async Task<string?> GetAndSetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var oldValue = await _db.StringGetSetAsync(fullKey, value);
        return oldValue.IsNull ? null : oldValue.ToString();
    }

    public async Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        return await _db.StringIncrementAsync(fullKey, value);
    }

    public async Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        return await _db.StringDecrementAsync(fullKey, value);
    }

    #endregion

    #region 分布式锁

    public async Task<bool> AcquireLockAsync(string lockKey, string lockValue, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey($"lock:{lockKey}");
        return await _db.StringSetAsync(fullKey, lockValue, expiration, When.NotExists);
    }

    public async Task<bool> ReleaseLockAsync(string lockKey, string lockValue, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey($"lock:{lockKey}");

        // 使用 Lua 脚本确保原子性：只有锁值匹配时才删除
        const string script = """
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end
            """;

        var result = await _db.ScriptEvaluateAsync(script, new RedisKey[] { fullKey }, new RedisValue[] { lockValue });
        return (bool)result;
    }

    #endregion

    #region 集合操作

    public async Task<bool> SetAddAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        return await _db.SetAddAsync(fullKey, value);
    }

    public async Task<bool> SetRemoveAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        return await _db.SetRemoveAsync(fullKey, value);
    }

    public async Task<IReadOnlyList<string>> SetMembersAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var members = await _db.SetMembersAsync(fullKey);
        return members.Select(m => m.ToString()).ToList();
    }

    public async Task<bool> SetContainsAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        return await _db.SetContainsAsync(fullKey, value);
    }

    #endregion

    #region 列表操作

    public async Task<long> ListLeftPushAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        return await _db.ListLeftPushAsync(fullKey, value);
    }

    public async Task<string?> ListRightPopAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var value = await _db.ListRightPopAsync(fullKey);
        return value.IsNull ? null : value.ToString();
    }

    public async Task<IReadOnlyList<string>> ListRangeAsync(string key, long start = 0, long stop = -1, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var values = await _db.ListRangeAsync(fullKey, start, stop);
        return values.Select(v => v.ToString()).ToList();
    }

    #endregion

    #region 哈希操作

    public async Task<bool> HashSetAsync(string key, string field, string value, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        return await _db.HashSetAsync(fullKey, field, value);
    }

    public async Task<string?> HashGetAsync(string key, string field, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var value = await _db.HashGetAsync(fullKey, field);
        return value.IsNull ? null : value.ToString();
    }

    public async Task<IReadOnlyDictionary<string, string>> HashGetAllAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var entries = await _db.HashGetAllAsync(fullKey);
        return entries.ToDictionary(
            e => e.Name.ToString(),
            e => e.Value.ToString());
    }

    public async Task<bool> HashDeleteAsync(string key, string field, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        return await _db.HashDeleteAsync(fullKey, field);
    }

    #endregion

    #region 批量操作

    public async Task<IReadOnlyDictionary<string, string?>> GetStringAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var keyList = keys.ToList();
        var fullKeys = keyList.Select(GetFullKey).Select(k => (RedisKey)k).ToArray();

        if (fullKeys.Length == 0)
        {
            return new Dictionary<string, string?>();
        }

        var values = await _db.StringGetAsync(fullKeys);
        var result = new Dictionary<string, string?>();

        for (int i = 0; i < keyList.Count; i++)
        {
            result[keyList[i]] = values[i].IsNull ? null : values[i].ToString();
        }

        return result;
    }

    public async Task SetStringAsync(IEnumerable<KeyValuePair<string, string>> keyValues, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var expiry = expiration ?? _options.DefaultExpiration;

        var batch = _db.CreateBatch();
        var tasks = new List<Task>();

        foreach (var (key, value) in keyValues)
        {
            var fullKey = GetFullKey(key);
            tasks.Add(batch.StringSetAsync(fullKey, value, expiry));
        }

        batch.Execute();
        await Task.WhenAll(tasks);
    }

    #endregion

    #region 模式匹配

    public async Task<IReadOnlyList<string>> GetKeysAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var fullPattern = GetFullKey(pattern);
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: fullPattern).Select(k => k.ToString());

        // 移除前缀
        if (!string.IsNullOrEmpty(_keyPrefix))
        {
            keys = keys.Select(k => k.StartsWith(_keyPrefix) ? k[_keyPrefix.Length..] : k);
        }

        return keys.ToList();
    }

    public async Task<long> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var keys = await GetKeysAsync(pattern, cancellationToken);
        if (keys.Count == 0) return 0;

        return await RemoveAsync(keys, cancellationToken);
    }

    #endregion

    #region 连接管理

    public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.PingAsync() != TimeSpan.Zero;
        }
        catch
        {
            return false;
        }
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        await server.FlushDatabaseAsync(_options.DefaultDatabase);
        _logger.LogInformation("Flushed Redis database {Database}", _options.DefaultDatabase);
    }

    #endregion

    #region 辅助方法

    private string GetFullKey(string key)
    {
        return string.IsNullOrEmpty(_keyPrefix) ? key : $"{_keyPrefix}{key}";
    }

    public void Dispose()
    {
        _redis?.Dispose();
    }

    #endregion
}
