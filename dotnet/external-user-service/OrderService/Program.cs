using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrderService.Data;
using OrderService.Services;
using OrderService.Models;
using Serilog;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using OrderService.Startup;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with better console output
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/order-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add console logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();
builder.Services.AddHttpClient<IJwtAuthenticationService, JwtAuthenticationService>();
builder.Services.AddSingleton<IJwtAuthenticationService, JwtAuthenticationService>();

builder.Services.AddHostedService<JwksRefreshBackgroundService>();

// Configure JWT Authentication 
builder.Services.AddJwtAuthenticationAndAuthorization(builder.Configuration);

// Add API Explorer services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Order Management Service API", 
        Version = "v1",
        Description = "A minimal API for order management with CRUD operations for Customers, Products, Orders, and OrderItems"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure URLs explicitly
app.Urls.Clear();
app.Urls.Add("https://localhost:7000");
app.Urls.Add("http://localhost:5001");

// Add startup logging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 Starting Order Management Service...");
logger.LogInformation("📊 Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("🔗 User Service URL: {UserServiceUrl}", builder.Configuration["Jwt:Authority"]);
logger.LogInformation("🎯 Order Service will run on: {OrderServiceUrl}", "https://localhost:7000");
logger.LogInformation("🎯 Order Service HTTP will run on: {OrderServiceHttpUrl}", "http://localhost:5001");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    logger.LogInformation("📚 Swagger UI available at: https://localhost:7000/swagger");
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    logger.LogInformation("🗄️ Initializing database...");
    context.Database.EnsureCreated();
    logger.LogInformation("✅ Database initialized successfully");
}

// Customer endpoints
app.MapGet("/api/customers", async (ICustomerService customerService) =>
{
    var customers = await customerService.GetAllCustomersAsync();
    return Results.Ok(customers);
})
.WithName("GetAllCustomers")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/api/customers/{id:int}", async (int id, ICustomerService customerService) =>
{
    var customer = await customerService.GetCustomerByIdAsync(id);
    return customer != null ? Results.Ok(customer) : Results.NotFound();
})
.WithName("GetCustomerById")
.WithOpenApi()
.RequireAuthorization();

app.MapPost("/api/customers", async (CreateCustomerDto createCustomerDto, ICustomerService customerService) =>
{
    try
    {
        var customer = await customerService.CreateCustomerAsync(createCustomerDto);
        return Results.Created($"/api/customers/{customer.Id}", customer);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CreateCustomer")
.WithOpenApi()
.RequireAuthorization();

app.MapPut("/api/customers/{id:int}", async (int id, UpdateCustomerDto updateCustomerDto, ICustomerService customerService) =>
{
    try
    {
        var customer = await customerService.UpdateCustomerAsync(id, updateCustomerDto);
        return customer != null ? Results.Ok(customer) : Results.NotFound();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("UpdateCustomer")
.WithOpenApi()
.RequireAuthorization();

app.MapDelete("/api/customers/{id:int}", async (int id, ICustomerService customerService) =>
{
    try
    {
        var result = await customerService.DeleteCustomerAsync(id);
        return result ? Results.NoContent() : Results.NotFound();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("DeleteCustomer")
.WithOpenApi()
.RequireAuthorization();

// Product endpoints
app.MapGet("/api/products", async (IProductService productService) =>
{
    var products = await productService.GetAllProductsAsync();
    return Results.Ok(products);
})
.WithName("GetAllProducts")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/api/products/{id:int}", async (int id, IProductService productService) =>
{
    var product = await productService.GetProductByIdAsync(id);
    return product != null ? Results.Ok(product) : Results.NotFound();
})
.WithName("GetProductById")
.WithOpenApi()
.RequireAuthorization();

app.MapPost("/api/products", async (CreateProductDto createProductDto, IProductService productService) =>
{
    try
    {
        var product = await productService.CreateProductAsync(createProductDto);
        return Results.Created($"/api/products/{product.Id}", product);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CreateProduct")
.WithOpenApi()
.RequireAuthorization();

app.MapPut("/api/products/{id:int}", async (int id, UpdateProductDto updateProductDto, IProductService productService) =>
{
    try
    {
        var product = await productService.UpdateProductAsync(id, updateProductDto);
        return product != null ? Results.Ok(product) : Results.NotFound();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("UpdateProduct")
.WithOpenApi()
.RequireAuthorization();

app.MapDelete("/api/products/{id:int}", async (int id, IProductService productService) =>
{
    try
    {
        var result = await productService.DeleteProductAsync(id);
        return result ? Results.NoContent() : Results.NotFound();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("DeleteProduct")
.WithOpenApi()
.RequireAuthorization();

// Order endpoints
app.MapGet("/api/orders", async (IOrderService orderService) =>
{
    var orders = await orderService.GetAllOrdersAsync();
    return Results.Ok(orders);
})
.WithName("GetAllOrders")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/api/orders/{id:int}", async (int id, IOrderService orderService) =>
{
    var order = await orderService.GetOrderByIdAsync(id);
    return order != null ? Results.Ok(order) : Results.NotFound();
})
.WithName("GetOrderById")
.WithOpenApi()
.RequireAuthorization();

app.MapPost("/api/orders", async (CreateOrderDto createOrderDto, IOrderService orderService) =>
{
    try
    {
        var order = await orderService.CreateOrderAsync(createOrderDto);
        return Results.Created($"/api/orders/{order.Id}", order);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CreateOrder")
.WithOpenApi()
.RequireAuthorization();

app.MapPut("/api/orders/{id:int}", async (int id, UpdateOrderDto updateOrderDto, IOrderService orderService) =>
{
    try
    {
        var order = await orderService.UpdateOrderAsync(id, updateOrderDto);
        return order != null ? Results.Ok(order) : Results.NotFound();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("UpdateOrder")
.WithOpenApi()
.RequireAuthorization();

app.MapDelete("/api/orders/{id:int}", async (int id, IOrderService orderService) =>
{
    try
    {
        var result = await orderService.DeleteOrderAsync(id);
        return result ? Results.NoContent() : Results.NotFound();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("DeleteOrder")
.WithOpenApi()
.RequireAuthorization();

app.MapPut("/api/orders/{id:int}/status", async (int id, string status, IOrderService orderService) =>
{
    try
    {
        var order = await orderService.UpdateOrderStatusAsync(id, status);
        return order != null ? Results.Ok(order) : Results.NotFound();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("UpdateOrderStatus")
.WithOpenApi()
.RequireAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
.WithName("HealthCheck")
.WithOpenApi();

// Add startup completion logging
logger.LogInformation("🎉 Order Management Service is ready!");
logger.LogInformation("🌐 Service URLs:");
logger.LogInformation("   • Health Check: https://localhost:7000/health");
logger.LogInformation("   • Swagger UI: https://localhost:7000/swagger");
logger.LogInformation("   • API Base: https://localhost:7000/api");
logger.LogInformation("🔐 JWT Authentication configured with User Service");
logger.LogInformation("📋 Available endpoints:");
logger.LogInformation("   • GET /api/customers - Get all customers");
logger.LogInformation("   • GET /api/products - Get all products");
logger.LogInformation("   • GET /api/orders - Get all orders");
logger.LogInformation("   • POST /api/customers - Create customer");
logger.LogInformation("   • POST /api/products - Create product");
logger.LogInformation("   • POST /api/orders - Create order");
logger.LogInformation("=");
logger.LogInformation("🚀 Order Service is now running and ready to accept requests!");
logger.LogInformation("=");

// Add listening confirmation
app.Lifetime.ApplicationStarted.Register(() =>
{
    logger.LogInformation("🎯 Order Service is now listening on:");
    foreach (var url in app.Urls)
    {
        logger.LogInformation("   • {Url}", url);
    }
    logger.LogInformation("✅ Ready to accept requests!");
});

app.Run();

