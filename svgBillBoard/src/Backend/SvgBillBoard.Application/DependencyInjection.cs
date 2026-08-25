using Microsoft.Extensions.DependencyInjection;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.Services;

namespace SvgBillBoard.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IDeviceAuthenticationService, DeviceAuthenticationService>();
        services.AddScoped<IDeviceHeartbeatService, DeviceHeartbeatService>();
        return services;
    }
}