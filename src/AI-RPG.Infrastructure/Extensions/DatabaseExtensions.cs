using AI_RPG.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI_RPG.Infrastructure.Extensions;

/// <summary>
/// 数据库扩展方法
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// 自动初始化数据库（创建表结构）
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var options = services.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var logger = services.GetService<ILogger<DatabaseInitializer>>();
            
            var initializer = new DatabaseInitializer(options.ConnectionString, logger);
            await initializer.InitializeAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<DatabaseInitializer>>();
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    /// <summary>
    /// 自动初始化数据库（同步版本，用于 Program.cs）
    /// </summary>
    public static void InitializeDatabase(this IApplicationBuilder app)
    {
        InitializeDatabaseAsync(app).GetAwaiter().GetResult();
    }
}
