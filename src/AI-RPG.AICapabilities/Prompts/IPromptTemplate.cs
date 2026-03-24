using System.Text.RegularExpressions;

namespace AI_RPG.AICapabilities.Prompts;

/// <summary>
/// 提示模板接口
/// </summary>
public interface IPromptTemplate
{
    /// <summary>
    /// 模板名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 模板内容
    /// </summary>
    string Template { get; }

    /// <summary>
    /// 渲染模板，替换变量
    /// </summary>
    /// <param name="variables">变量字典</param>
    /// <returns>渲染后的提示</returns>
    string Render(IDictionary<string, object?> variables);

    /// <summary>
    /// 获取模板中的变量名列表
    /// </summary>
    IReadOnlyList<string> GetVariables();
}

/// <summary>
/// 简单字符串替换提示模板实现
/// </summary>
public sealed class PromptTemplate : IPromptTemplate
{
    private static readonly Regex VariableRegex = new(@"\{\{(\s*[\w\.]+\s*)\}\}", RegexOptions.Compiled);

    public string Name { get; }

    public string Template { get; }

    public PromptTemplate(string name, string template)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(template);

        Name = name;
        Template = template;
    }

    public string Render(IDictionary<string, object?> variables)
    {
        ArgumentNullException.ThrowIfNull(variables);

        var result = Template;
        
        foreach (var (key, value) in variables)
        {
            var placeholder = $"{{{{{key}}}}}";
            result = result.Replace(placeholder, value?.ToString() ?? string.Empty);
        }

        // 检查是否有未替换的变量
        var unmatched = VariableRegex.Matches(result);
        if (unmatched.Count > 0)
        {
            var vars = string.Join(", ", unmatched.Select(m => m.Groups[1].Value.Trim()));
            throw new InvalidOperationException($"Template '{Name}' has unmatched variables: {vars}");
        }

        return result;
    }

    public IReadOnlyList<string> GetVariables()
    {
        var matches = VariableRegex.Matches(Template);
        return matches.Select(m => m.Groups[1].Value.Trim()).Distinct().ToList();
    }
}

/// <summary>
/// 提示模板管理器
/// </summary>
public interface IPromptTemplateManager
{
    /// <summary>
    /// 注册模板
    /// </summary>
    void RegisterTemplate(IPromptTemplate template);

    /// <summary>
    /// 获取模板
    /// </summary>
    IPromptTemplate? GetTemplate(string name);

    /// <summary>
    /// 渲染指定模板
    /// </summary>
    string Render(string templateName, IDictionary<string, object?> variables);

    /// <summary>
    /// 获取所有模板名称
    /// </summary>
    IReadOnlyList<string> GetTemplateNames();
}

/// <summary>
/// 提示模板管理器实现
/// </summary>
public sealed class PromptTemplateManager : IPromptTemplateManager
{
    private readonly Dictionary<string, IPromptTemplate> _templates = new();

    public void RegisterTemplate(IPromptTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);
        _templates[template.Name] = template;
    }

    public IPromptTemplate? GetTemplate(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _templates.TryGetValue(name, out var template);
        return template;
    }

    public string Render(string templateName, IDictionary<string, object?> variables)
    {
        var template = GetTemplate(templateName);
        if (template == null)
        {
            throw new KeyNotFoundException($"Template '{templateName}' not found");
        }

        return template.Render(variables);
    }

    public IReadOnlyList<string> GetTemplateNames()
    {
        return _templates.Keys.ToList();
    }
}
