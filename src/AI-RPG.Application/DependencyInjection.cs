using AI_RPG.Application.Interfaces;
using AI_RPG.Application.Services;
using AI_RPG.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AI_RPG.Application;

/// <summary>
/// Application 层依赖注入扩展
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 添加 Application 层服务
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // 应用服务
        services.AddScoped<ISessionAppService, SessionAppService>();
        services.AddScoped<IDialogueAppService, DialogueAppService>();

        // 领域服务实现
        services.AddScoped<IDialogueService, AIDialogueService>();

        return services;
    }
}
