using AspNetJwtAuth.Demo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace AspNetJwtAuth.Demo
{
    /// <summary>
    /// Demo Resource Service program
    /// This service validates JWT tokens from the User Service
    /// </summary>
    public class DemoProgram
    {
        public static void Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/demo-resource-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            // Add services
            builder.Services.AddControllers();

            // Configure JWT Authentication to validate tokens from User Service
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "https://user-service.com",
                        ValidAudience = "demo-api-service",
                        ClockSkew = TimeSpan.FromMinutes(5),
                        IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                        {
                            // Fetch public key from User Service JWKS endpoint
                            var httpClient = new HttpClient();
                            try
                            {
                                var jwksUrl = "http://localhost:5000/.well-known/jwks.json";
                                var response = httpClient.GetStringAsync(jwksUrl).Result;
                                var jwks = System.Text.Json.JsonSerializer.Deserialize<JwksResponse>(response, new System.Text.Json.JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });

                                if (jwks?.Keys != null)
                                {
                                    var key = jwks.Keys.FirstOrDefault(k => k.Kid == kid);
                                    if (key != null)
                                    {
                                        var rsa = System.Security.Cryptography.RSA.Create();
                                        rsa.ImportParameters(new System.Security.Cryptography.RSAParameters
                                        {
                                            Modulus = Base64UrlEncoder.DecodeBytes(key.N),
                                            Exponent = Base64UrlEncoder.DecodeBytes(key.E)
                                        });
                                        return new List<SecurityKey> { new RsaSecurityKey(rsa) { KeyId = key.Kid } };
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Error fetching JWKS from User Service");
                            }
                            return new List<SecurityKey>();
                        }
                    };
                });

            // Add authorization
            builder.Services.AddAuthorization();

            // Add CORS for development
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

            // Configure pipeline
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Configure URLs
            app.Urls.Add("http://localhost:5001");
            app.Urls.Add("https://localhost:5002");

            try
            {
                Log.Information("Starting Demo Resource Service on http://localhost:5001");
                Log.Information("This service validates JWT tokens from User Service");
                Log.Information("User Service JWKS: http://localhost:5000/.well-known/jwks.json");
                Log.Information("Health check: GET http://localhost:5001/api/demoresource/health");
                
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Demo Resource Service terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }

    // JWKS response model for deserialization
    public class JwksResponse
    {
        public List<JsonWebKey> Keys { get; set; } = new();
    }

    public class JsonWebKey
    {
        public string Kid { get; set; } = string.Empty;
        public string N { get; set; } = string.Empty;
        public string E { get; set; } = string.Empty;
    }
}
