using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SvgBillBoard.Application.Abstractions.Security;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Domain.Interfaces;
using SvgBillBoard.Infrastructure.Authentication;
using SvgBillBoard.Infrastructure.BackgroundServices;
using SvgBillBoard.Infrastructure.Persistence;
using SvgBillBoard.Infrastructure.Repositories;
using SvgBillBoard.Infrastructure.Storage;

namespace SvgBillBoard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useInMemoryDatabase = false,
        string? testDatabaseName = null)
    {
        if (useInMemoryDatabase)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(
                    testDatabaseName ?? "SvgBillBoard_TestDb"));
        }
        else
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString(
                        "DefaultConnection")));
        }

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
        services.AddHostedService<DeviceStatusMonitor>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IPlaylistRepository, PlaylistRepository>();
        services.AddScoped<IPlaylistAssignmentRepository, PlaylistAssignmentRepository>();
        services.AddScoped<IPlaylistScheduleRepository, PlaylistScheduleRepository>();
        return services;
    }
}