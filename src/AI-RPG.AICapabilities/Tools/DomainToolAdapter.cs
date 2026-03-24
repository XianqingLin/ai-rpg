using System.Reflection;
using Microsoft.Extensions.Logging;

namespace AI_RPG.AICapabilities.Tools;

/// <summary>
/// 领域服务方法信息
/// </summary>
public sealed class DomainMethodInfo
{
    /// <summary>
    /// 方法名称
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 方法描述
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 参数信息
    /// </summary>
    public IReadOnlyList<DomainParameterInfo> Parameters { get; init; } = [];

    /// <summary>
    /// 返回值类型
    /// </summary>
    public Type? ReturnType { get; init; }

    /// <summary>
    /// 返回值描述
    /// </summary>
    public string? ReturnDescription { get; init; }
}

/// <summary>
/// 领域参数信息
/// </summary>
public sealed class DomainParameterInfo
{
    /// <summary>
    /// 参数名称
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 参数类型
    /// </summary>
    public required Type Type { get; init; }

    /// <summary>
    /// 参数描述
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 是否必需
    /// </summary>
    public bool IsRequired { get; init; } = true;

    /// <summary>
    /// 默认值
    /// </summary>
    public object? DefaultValue { get; init; }
}

/// <summary>
/// 领域服务适配器 - 将领域服务方法包装为AI工具
/// </summary>
public sealed class DomainToolAdapter : AITool
{
    private readonly object _targetService;
    private readonly MethodInfo _method;
    private readonly ILogger<DomainToolAdapter>? _logger;

    public override ToolMetadata Metadata { get; }

    /// <summary>
    /// 创建领域服务适配器
    /// </summary>
    /// <param name="targetService">领域服务实例</param>
    /// <param name="methodInfo">方法信息</param>
    /// <param name="methodDescription">方法描述</param>
    /// <param name="logger">日志记录器</param>
    public DomainToolAdapter(
        object targetService,
        MethodInfo methodInfo,
        DomainMethodInfo methodDescription,
        ILogger<DomainToolAdapter>? logger = null)
    {
        _targetService = targetService ?? throw new ArgumentNullException(nameof(targetService));
        _method = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
        _logger = logger;

        Metadata = new ToolMetadata
        {
            Name = methodDescription.Name,
            Description = methodDescription.Description ?? $"调用 {methodDescription.Name} 方法",
            Parameters = methodDescription.Parameters.Select(p => new ToolParameter
            {
                Name = p.Name,
                Description = p.Description ?? $"参数 {p.Name}",
                Type = GetTypeName(p.Type),
                Required = p.IsRequired,
                DefaultValue = p.DefaultValue
            }).ToList(),
            ReturnDescription = methodDescription.ReturnDescription
        };
    }

    public override async Task<string> ExecuteAsync(
        IDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        ValidateParameters(parameters);

        try
        {
            _logger?.LogDebug("Executing domain method '{MethodName}'", Metadata.Name);

            // 准备参数
            var methodParams = _method.GetParameters();
            var args = new object?[methodParams.Length];

            for (int i = 0; i < methodParams.Length; i++)
            {
                var param = methodParams[i];
                
                if (parameters.TryGetValue(param.Name!, out var value) && value != null)
                {
                    args[i] = ConvertValue(value, param.ParameterType);
                }
                else if (param.HasDefaultValue)
                {
                    args[i] = param.DefaultValue;
                }
                else if (param.ParameterType == typeof(CancellationToken))
                {
                    args[i] = cancellationToken;
                }
                else
                {
                    throw new ArgumentException($"Missing required parameter '{param.Name}'");
                }
            }

            // 调用方法
            var result = _method.Invoke(_targetService, args);

            // 处理异步结果
            if (result is Task task)
            {
                await task;

                // 获取Task<T>的结果
                var resultProperty = task.GetType().GetProperty("Result");
                if (resultProperty != null)
                {
                    result = resultProperty.GetValue(task);
                }
                else
                {
                    result = null;
                }
            }

            // 序列化结果
            return result != null 
                ? System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                })
                : "null";
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            _logger?.LogError(ex.InnerException, "Error invoking domain method '{MethodName}'", Metadata.Name);
            throw ex.InnerException;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error executing domain method '{MethodName}'", Metadata.Name);
            throw;
        }
    }

    private static object? ConvertValue(object value, Type targetType)
    {
        if (value == null)
            return null;

        if (targetType.IsInstanceOfType(value))
            return value;

        // 处理可空类型
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            if (value == null) return null;
            targetType = underlyingType;
        }

        // 类型转换
        try
        {
            if (targetType.IsEnum && value is string str)
            {
                return Enum.Parse(targetType, str, true);
            }

            if (targetType == typeof(Guid) && value is string guidStr)
            {
                return Guid.Parse(guidStr);
            }

            if (targetType == typeof(DateTime) && value is string dateStr)
            {
                return DateTime.Parse(dateStr);
            }

            return Convert.ChangeType(value, targetType);
        }
        catch (Exception ex)
        {
            throw new InvalidCastException(
                $"Cannot convert value '{value}' of type {value.GetType().Name} to {targetType.Name}", ex);
        }
    }

    private static string GetTypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int) || type == typeof(long)) return "integer";
        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) return "number";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(DateTime)) return "datetime";
        if (type == typeof(Guid)) return "uuid";
        if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))) return "array";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) return "object";
        
        return "string";
    }
}

/// <summary>
/// 领域工具构建器
/// </summary>
public sealed class DomainToolBuilder
{
    private readonly IToolRegistry _toolRegistry;
    private readonly ILoggerFactory? _loggerFactory;

    public DomainToolBuilder(IToolRegistry toolRegistry, ILoggerFactory? loggerFactory = null)
    {
        _toolRegistry = toolRegistry ?? throw new ArgumentNullException(nameof(toolRegistry));
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// 从领域服务注册工具
    /// </summary>
    public DomainToolBuilder RegisterFromService<TService>(
        TService service,
        params string[] methodNames) where TService : class
    {
        var serviceType = typeof(TService);
        var methods = methodNames.Length > 0
            ? methodNames.Select(name => serviceType.GetMethod(name)).Where(m => m != null).Cast<MethodInfo>().ToList()
            : serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName && m.DeclaringType != typeof(object))
                .ToList();

        foreach (var method in methods)
        {
            if (method == null) continue;

            var methodInfo = new DomainMethodInfo
            {
                Name = method.Name,
                Description = GetMethodDescription(method),
                Parameters = method.GetParameters().Select(p => new DomainParameterInfo
                {
                    Name = p.Name ?? "param",
                    Type = p.ParameterType,
                    IsRequired = !p.IsOptional,
                    DefaultValue = p.DefaultValue
                }).ToList(),
                ReturnType = method.ReturnType,
                ReturnDescription = GetReturnDescription(method)
            };

            var logger = _loggerFactory?.CreateLogger<DomainToolAdapter>();
            var tool = new DomainToolAdapter(service, method, methodInfo, logger);
            _toolRegistry.RegisterTool(tool);
        }

        return this;
    }

    private static string? GetMethodDescription(MethodInfo method)
    {
        // 可以从自定义Attribute获取描述
        var attr = method.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
        return attr?.Description;
    }

    private static string? GetReturnDescription(MethodInfo method)
    {
        // 可以从自定义Attribute获取返回值描述
        return null;
    }
}
