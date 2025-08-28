using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Serilog;
using AspNetJwtAuth.Data;
using AspNetJwtAuth.Services;
using AspNetJwtAuth.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/jwt-auth-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"), 
                     sqliteOptions => sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IJwksService, JwksService>();
builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// Configure JWT Authentication with proper key resolution
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.FromMinutes(5),
            RequireSignedTokens = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Token;
                Log.Information("JWT Token received: {HasToken} - Length: {TokenLength}", 
                    !string.IsNullOrEmpty(token), 
                    token?.Length ?? 0);
                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                Log.Information("JWT Token validated successfully for user: {User}", 
                    context.Principal?.Identity?.Name ?? "Unknown");
                
                var blacklistService = context.HttpContext.RequestServices.GetService<ITokenBlacklistService>();
                var jti = context.Principal?.FindFirst("jti")?.Value;
                
                Log.Information("Checking token blacklist for JTI: {JTI}", jti);
                
                if (!string.IsNullOrEmpty(jti) && blacklistService != null && 
                    await blacklistService.IsTokenBlacklistedAsync(jti))
                {
                    Log.Warning("Token {JTI} is blacklisted, failing authentication", jti);
                    context.Fail("Token has been revoked");
                }
                else
                {
                    Log.Information("Token {JTI} is not blacklisted", jti);
                }
            },
            OnAuthenticationFailed = context =>
            {
                Log.Error("JWT Authentication failed: {Error} - {Exception} - {InnerException}", 
                    context.Exception?.Message,
                    context.Exception?.GetType().Name,
                    context.Exception?.InnerException?.Message);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Log.Warning("JWT Challenge triggered: {Error} - {ErrorDescription} - {AuthenticateFailure}", 
                    context.Error, 
                    context.ErrorDescription,
                    context.AuthenticateFailure?.Message);
                return Task.CompletedTask;
            }
        };
    });

// Add authorization with policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("ModeratorOrAdmin", policy => policy.RequireRole("admin", "moderator"));
    options.AddPolicy("WritePostsPermission", policy => policy.RequireClaim("permissions", "write:posts"));
    options.AddPolicy("DeletePostsPermission", policy => policy.RequireClaim("permissions", "delete:posts"));
});

builder.Services.AddControllers();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "JWT Authentication API", 
        Version = "v1",
        Description = "ASP.NET Core JWT Authentication with JWKS and Role-Based Access Control"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure JWT signing key after app is built
using (var scope = app.Services.CreateScope())
{
    var jwtOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
    var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
    
    var signingKey = jwtTokenService.GetSigningKey();
    jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters.IssuerSigningKey = signingKey;
    
    Log.Information("JWT signing key configured: {KeyType}", signingKey.GetType().Name);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "JWT Auth API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("DefaultPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName,
    Version = "1.0.0"
});

// JWT info endpoint (remove in production)
app.MapGet("/debug/jwt-info", (IJwtTokenService jwtService) => new
{
    KeyType = jwtService.GetSigningKey().GetType().Name,
    HasKey = jwtService.GetSigningKey() != null
}).RequireAuthorization("AdminOnly");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

try
{
    Log.Information("Starting JWT Authentication Service");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    Log.Information("Swagger UI available at: /swagger");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}