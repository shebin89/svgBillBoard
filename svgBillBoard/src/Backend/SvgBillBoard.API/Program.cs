using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SvgBillBoard.API.Hubs;
using SvgBillBoard.API.Services;
using SvgBillBoard.Application;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Infrastructure;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var jwtSettings = builder.Configuration
    .GetSection("Jwt");

var jwtSecret = jwtSettings["Secret"]
    ?? throw new InvalidOperationException(
        "JWT Secret is not configured.");

var jwtIssuer = jwtSettings["Issuer"]
    ?? throw new InvalidOperationException(
        "JWT Issuer is not configured.");

var jwtAudience = jwtSettings["Audience"]
    ?? throw new InvalidOperationException(
        "JWT Audience is not configured.");

// Controllers
builder.Services.AddControllers();

builder.Services.AddSignalR();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token."
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "Bearer",
                document)] = []
        });
});

// JWT Authentication
builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSecret)),

                ClockSkew = TimeSpan.Zero
            };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine(
                    "========== JWT AUTHENTICATION FAILED ==========");

                Console.WriteLine(
                    context.Exception.ToString());

                Console.WriteLine(
                    "================================================");

                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                Console.WriteLine(
                    "========== JWT CHALLENGE ==========");

                Console.WriteLine(
                    $"Error: {context.Error}");

                Console.WriteLine(
                    $"Description: {context.ErrorDescription}");

                Console.WriteLine(
                    "===================================");

                return Task.CompletedTask;
            }
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DeviceOnly", policy =>
    {
        policy.RequireAuthenticatedUser();

        policy.RequireClaim(
            "device",
            "true");
    });
});

// Application / Infrastructure
builder.Services.AddApplication();

builder.Services.AddInfrastructure(
    builder.Configuration,
    builder.Environment.IsEnvironment("Testing"),
    builder.Environment.IsEnvironment("Testing")
        ? $"SvgBillBoard_TestDb_{Guid.NewGuid()}"
        : null);

// SignalR notifier
builder.Services.AddScoped<
    IDeviceStatusNotifier,
    DeviceStatusNotifier>();

// Build
var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<DeviceStatusHub>(
    "/hubs/device-status");

app.Run();

public partial class Program
{
}