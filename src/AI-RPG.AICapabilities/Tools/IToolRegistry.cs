namespace AI_RPG.AICapabilities.Tools;

/// <summary>
/// 工具注册表接口
/// </summary>
public interface IToolRegistry
{
    /// <summary>
    /// 注册工具
    /// </summary>
    void RegisterTool(AITool tool);

    /// <summary>
    /// 注销工具
    /// </summary>
    void UnregisterTool(string toolName);

    /// <summary>
    /// 获取工具
    /// </summary>
    AITool? GetTool(string toolName);

    /// <summary>
    /// 获取工具元数据
    /// </summary>
    ToolMetadata? GetToolMetadata(string toolName);

    /// <summary>
    /// 获取所有已注册工具
    /// </summary>
    IReadOnlyList<AITool> GetAllTools();

    /// <summary>
    /// 获取所有工具元数据
    /// </summary>
    IReadOnlyList<ToolMetadata> GetAllMetadata();

    /// <summary>
    /// 检查工具是否已注册
    /// </summary>
    bool HasTool(string toolName);

    /// <summary>
    /// 获取工具数量
    /// </summary>
    int Count { get; }
}

/// <summary>
/// 工具注册表实现
/// </summary>
public sealed class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, AITool> _tools = new(StringComparer.OrdinalIgnoreCase);
    private readonly ReaderWriterLockSlim _lock = new();

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _tools.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public void RegisterTool(AITool tool)
    {
        ArgumentNullException.ThrowIfNull(tool);
        ArgumentException.ThrowIfNullOrWhiteSpace(tool.Metadata.Name);

        _lock.EnterWriteLock();
        try
        {
            _tools[tool.Metadata.Name] = tool;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void UnregisterTool(string toolName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolName);

        _lock.EnterWriteLock();
        try
        {
            _tools.Remove(toolName);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public AITool? GetTool(string toolName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolName);

        _lock.EnterReadLock();
        try
        {
            _tools.TryGetValue(toolName, out var tool);
            return tool;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public ToolMetadata? GetToolMetadata(string toolName)
    {
        return GetTool(toolName)?.Metadata;
    }

    public IReadOnlyList<AITool> GetAllTools()
    {
        _lock.EnterReadLock();
        try
        {
            return _tools.Values.ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IReadOnlyList<ToolMetadata> GetAllMetadata()
    {
        _lock.EnterReadLock();
        try
        {
            return _tools.Values.Select(t => t.Metadata).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool HasTool(string toolName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolName);

        _lock.EnterReadLock();
        try
        {
            return _tools.ContainsKey(toolName);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}
