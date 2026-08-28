using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Devices;
using SvgBillBoard.Domain.Interfaces;

namespace SvgBillBoard.Infrastructure.BackgroundServices;

public class DeviceOfflineMonitorService
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<DeviceOfflineMonitorService>
        _logger;

    private static readonly TimeSpan CheckInterval =
        TimeSpan.FromSeconds(30);

    private static readonly TimeSpan OfflineThreshold =
        TimeSpan.FromMinutes(2);

    public DeviceOfflineMonitorService(
        IServiceScopeFactory scopeFactory,
        ILogger<DeviceOfflineMonitorService> logger)
    {
        _scopeFactory = scopeFactory;

        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckDevicesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while checking device status.");
            }

            await Task.Delay(
                CheckInterval,
                stoppingToken);
        }
    }

    private async Task CheckDevicesAsync()
    {
        using var scope =
            _scopeFactory.CreateScope();

        var deviceRepository =
            scope.ServiceProvider
                .GetRequiredService<IDeviceRepository>();

        var notifier =
            scope.ServiceProvider
                .GetRequiredService<IDeviceStatusNotifier>();

        var devices =
            await deviceRepository
                .GetOnlineDevicesAsync();

        var offlineLimit =
            DateTime.UtcNow -
            OfflineThreshold;

        foreach (var device in devices)
        {
            if (device.LastHeartbeatAt == null)
            {
                continue;
            }

            if (device.LastHeartbeatAt < offlineLimit)
            {
                device.IsOnline = false;

                device.UpdatedAt =
                    DateTime.UtcNow;

                await deviceRepository
                    .UpdateAsync(device);

                await deviceRepository
                    .SaveChangesAsync();

                await notifier
                    .NotifyStatusChangedAsync(
                        new DeviceStatusChangedResponse
                        {
                            DeviceId =
                                device.Id,

                            OrganizationId =
                                device.OrganizationId,

                            IsOnline = false
                        });

                _logger.LogInformation(
                    "Device {DeviceId} marked offline.",
                    device.Id);
            }
        }
    }
}