using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SvgBillBoard.Application.Abstractions.Security;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Domain.Interfaces;
using SvgBillBoard.Infrastructure.Authentication;
using SvgBillBoard.Infrastructure.Persistence;
using SvgBillBoard.Infrastructure.Repositories;

namespace SvgBillBoard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<PasswordHasher>();
        services.AddScoped<JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IDevicePairingRepository, DevicePairingRepository>();
        services.AddScoped<IDeviceCredentialRepository, DeviceCredentialRepository>();
        services.AddScoped<IDeviceJwtService, DeviceJwtService>();
        return services;
    }
}