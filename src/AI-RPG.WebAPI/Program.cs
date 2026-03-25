using AI_RPG.Application.Interfaces;
using AI_RPG.Application.Services;
using AI_RPG.Infrastructure.Extensions;
using DotNetEnv;

// 加载 .env 文件
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// 添加环境变量配置（优先级高于 appsettings.json）
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddOpenApi();

// 注册基础设施服务（包括仓储）
builder.Services.AddInfrastructure(builder.Configuration);

// 注册用户应用服务
builder.Services.AddScoped<IUserAppService, UserAppService>();

// 注册控制器
builder.Services.AddControllers();

// 添加CORS支持
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 自动初始化数据库（创建表结构）
app.InitializeDatabase();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 启用路由（必须在UseCors和MapControllers之前）
app.UseRouting();

// CORS（在UseRouting之后，MapControllers之前）
app.UseCors("AllowAll");

// HTTPS重定向
app.UseHttpsRedirection();

// 测试端点
app.MapGet("/", () => "AI-RPG WebAPI is running!");
app.MapGet("/test", () => "GET test works!");
app.MapPost("/test", (HttpContext context) =>
{
    Console.WriteLine("POST /test received!");
    return "POST test works!";
});

// 映射控制器（必须在UseRouting之后）
app.MapControllers();

app.Run();
