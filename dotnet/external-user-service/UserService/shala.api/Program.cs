using shala.api.startup.configurations;

var builder = WebApplication.CreateBuilder(args);

// Add startup logging
Console.WriteLine("🚀 Starting User Service...");
Console.WriteLine("📊 Environment: " + builder.Environment.EnvironmentName);
Console.WriteLine("🔧 Loading configurations...");

builder.AddConfigs();
var app = builder.Build();

Console.WriteLine("⚙️ Configuring application...");
app.UseConfigs();

Console.WriteLine("🎯 User Service configuration complete!");
Console.WriteLine("=");

app.Run();
