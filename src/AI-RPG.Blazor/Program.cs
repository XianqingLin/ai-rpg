using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using AI_RPG.Blazor;
using AI_RPG.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 配置 HttpClient
var apiBaseUrl = builder.HostEnvironment.BaseAddress.Contains("localhost") 
    ? "http://localhost:5238/"  // WebAPI 地址
    : builder.HostEnvironment.BaseAddress;
    
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(apiBaseUrl) 
});

// 添加 MudBlazor 服务
builder.Services.AddMudServices();

// 注册应用服务
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<DialogueService>();

await builder.Build().RunAsync();
