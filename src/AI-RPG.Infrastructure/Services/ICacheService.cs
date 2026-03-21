namespace AI_RPG.Infrastructure.Services;

/// <summary>
/// 缓存服务接口
/// </summary>
public interface ICacheService
{
    #region 字符串操作

    /// <summary>
    /// 获取字符串值
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>缓存值，不存在时返回null</returns>
    Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取并反序列化对象
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>缓存对象，不存在时返回null</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置字符串值
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task SetStringAsync(string key, string value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置对象（序列化为JSON）
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存对象</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    #endregion

    #region 删除和过期

    /// <summary>
    /// 删除缓存
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功删除</returns>
    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量删除缓存
    /// </summary>
    /// <param name="keys">缓存键集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<long> RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置过期时间
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功设置</returns>
    Task<bool> SetExpirationAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查键是否存在
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    #endregion

    #region 原子操作

    /// <summary>
    /// 仅当键不存在时才设置值
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否设置成功</returns>
    Task<bool> SetIfNotExistsAsync(string key, string value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取并设置新值
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="value">新值</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>旧值</returns>
    Task<string?> GetAndSetAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 递增
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="value">递增值</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>递增后的值</returns>
    Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// 递减
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="value">递减值</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>递减后的值</returns>
    Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);

    #endregion

    #region 分布式锁

    /// <summary>
    /// 获取分布式锁
    /// </summary>
    /// <param name="lockKey">锁键</param>
    /// <param name="lockValue">锁值（用于释放时验证）</param>
    /// <param name="expiration">锁过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否获取成功</returns>
    Task<bool> AcquireLockAsync(string lockKey, string lockValue, TimeSpan expiration, CancellationToken cancellationToken = default);

    /// <summary>
    /// 释放分布式锁
    /// </summary>
    /// <param name="lockKey">锁键</param>
    /// <param name="lockValue">锁值（用于验证）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否释放成功</returns>
    Task<bool> ReleaseLockAsync(string lockKey, string lockValue, CancellationToken cancellationToken = default);

    #endregion

    #region 集合操作

    /// <summary>
    /// 添加到集合
    /// </summary>
    /// <param name="key">集合键</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> SetAddAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从集合中移除
    /// </summary>
    /// <param name="key">集合键</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> SetRemoveAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取集合所有成员
    /// </summary>
    /// <param name="key">集合键</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyList<string>> SetMembersAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查值是否在集合中
    /// </summary>
    /// <param name="key">集合键</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> SetContainsAsync(string key, string value, CancellationToken cancellationToken = default);

    #endregion

    #region 列表操作

    /// <summary>
    /// 从列表左侧推入值
    /// </summary>
    /// <param name="key">列表键</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<long> ListLeftPushAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从列表右侧弹出值
    /// </summary>
    /// <param name="key">列表键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>弹出的值</returns>
    Task<string?> ListRightPopAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取列表范围
    /// </summary>
    /// <param name="key">列表键</param>
    /// <param name="start">起始索引</param>
    /// <param name="stop">结束索引</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyList<string>> ListRangeAsync(string key, long start = 0, long stop = -1, CancellationToken cancellationToken = default);

    #endregion

    #region 哈希操作

    /// <summary>
    /// 设置哈希字段值
    /// </summary>
    /// <param name="key">哈希键</param>
    /// <param name="field">字段名</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> HashSetAsync(string key, string field, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取哈希字段值
    /// </summary>
    /// <param name="key">哈希键</param>
    /// <param name="field">字段名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>字段值</returns>
    Task<string?> HashGetAsync(string key, string field, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取哈希所有字段和值
    /// </summary>
    /// <param name="key">哈希键</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyDictionary<string, string>> HashGetAllAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除哈希字段
    /// </summary>
    /// <param name="key">哈希键</param>
    /// <param name="field">字段名</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> HashDeleteAsync(string key, string field, CancellationToken cancellationToken = default);

    #endregion

    #region 批量操作

    /// <summary>
    /// 批量获取
    /// </summary>
    /// <param name="keys">缓存键集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyDictionary<string, string?>> GetStringAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量设置
    /// </summary>
    /// <param name="keyValues">键值对</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task SetStringAsync(IEnumerable<KeyValuePair<string, string>> keyValues, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    #endregion

    #region 模式匹配

    /// <summary>
    /// 根据模式查找键
    /// </summary>
    /// <param name="pattern">匹配模式（如 user:*）</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyList<string>> GetKeysAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据模式删除键
    /// </summary>
    /// <param name="pattern">匹配模式</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<long> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    #endregion

    #region 连接管理

    /// <summary>
    /// 检查连接状态
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> PingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 清空当前数据库
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task FlushAsync(CancellationToken cancellationToken = default);

    #endregion
}
