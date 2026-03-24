using AI_RPG.AICapabilities.Extensions;
using AI_RPG.Application.Interfaces;
using AI_RPG.Application.Services;
using AI_RPG.Domain.Repositories;
using AI_RPG.Domain.Services;
using AI_RPG.Infrastructure.Extensions;
using AI_RPG.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// 注册基础设施服务（内存仓储）
builder.Services.AddInMemoryRepositories();

// 注册AI能力层服务（Kimi LLM）
builder.Services.AddKimiClient(builder.Configuration);

// 注册应用服务
builder.Services.AddScoped<ISessionAppService, SessionAppService>();
builder.Services.AddScoped<IDialogueAppService, DialogueAppService>();

// 注册领域服务
builder.Services.AddScoped<IDialogueService, AIDialogueService>();

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
