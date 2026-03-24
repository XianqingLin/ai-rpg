namespace AI_RPG.AICapabilities.Tools;

/// <summary>
/// 工具参数定义
/// </summary>
public sealed class ToolParameter
{
    /// <summary>
    /// 参数名称
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 参数描述
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// 参数类型
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// 是否必需
    /// </summary>
    public bool Required { get; init; } = true;

    /// <summary>
    /// 默认值
    /// </summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    /// 枚举值（如果是枚举类型）
    /// </summary>
    public IReadOnlyList<string>? EnumValues { get; init; }
}

/// <summary>
/// 工具元数据
/// </summary>
public sealed class ToolMetadata
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 工具描述
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// 参数定义
    /// </summary>
    public IReadOnlyList<ToolParameter> Parameters { get; init; } = [];

    /// <summary>
    /// 返回值描述
    /// </summary>
    public string? ReturnDescription { get; init; }

    /// <summary>
    /// 示例用法
    /// </summary>
    public string? Example { get; init; }
}

/// <summary>
/// AI工具基类
/// </summary>
public abstract class AITool
{
    /// <summary>
    /// 工具元数据
    /// </summary>
    public abstract ToolMetadata Metadata { get; }

    /// <summary>
    /// 执行工具
    /// </summary>
    /// <param name="parameters">参数字典</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行结果</returns>
    public abstract Task<string> ExecuteAsync(
        IDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 验证参数
    /// </summary>
    protected virtual void ValidateParameters(IDictionary<string, object?> parameters)
    {
        foreach (var param in Metadata.Parameters.Where(p => p.Required))
        {
            if (!parameters.ContainsKey(param.Name) || parameters[param.Name] == null)
            {
                throw new ArgumentException($"Required parameter '{param.Name}' is missing");
            }
        }
    }

    /// <summary>
    /// 获取参数值
    /// </summary>
    protected T GetParameter<T>(IDictionary<string, object?> parameters, string name, T? defaultValue = default)
    {
        if (parameters.TryGetValue(name, out var value) && value != null)
        {
            if (value is T typedValue)
            {
                return typedValue;
            }

            // 尝试转换
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                throw new InvalidOperationException($"Parameter '{name}' cannot be converted to {typeof(T).Name}");
            }
        }

        if (defaultValue != null)
        {
            return defaultValue;
        }

        throw new ArgumentException($"Parameter '{name}' is required");
    }
}
