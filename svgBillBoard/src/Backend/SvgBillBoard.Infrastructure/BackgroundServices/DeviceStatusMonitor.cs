using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Devices;
using SvgBillBoard.Infrastructure.Persistence;

namespace SvgBillBoard.Infrastructure.BackgroundServices;

public class DeviceStatusMonitor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly TimeSpan CheckInterval =
        TimeSpan.FromSeconds(30);

    private static readonly TimeSpan OfflineThreshold =
        TimeSpan.FromSeconds(90);

    public DeviceStatusMonitor(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckDevicesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Device status monitor error: {ex.Message}");
            }

            await Task.Delay(
                CheckInterval,
                stoppingToken);
        }
    }

    private async Task CheckDevicesAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            _scopeFactory.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var notifier =
            scope.ServiceProvider
                .GetRequiredService<IDeviceStatusNotifier>();

        var threshold =
            DateTime.UtcNow - OfflineThreshold;

        var devices =
            await context.Devices
                .Where(x =>
                    x.IsOnline &&
                    x.LastHeartbeatAt != null &&
                    x.LastHeartbeatAt < threshold)
                .ToListAsync(cancellationToken);

        foreach (var device in devices)
        {
            device.IsOnline = false;
            device.UpdatedAt = DateTime.UtcNow;

            await notifier.NotifyStatusChangedAsync(
                new DeviceStatusChangedResponse
                {
                    DeviceId = device.Id,
                    OrganizationId =
                        device.OrganizationId,
                    DeviceCode =
                        device.DeviceCode,
                    DeviceName =
                        device.Name,
                    IsOnline = false,
                    ChangedAt = DateTime.UtcNow
                });
        }

        if (devices.Count > 0)
        {
            await context.SaveChangesAsync(
                cancellationToken);
        }
    }
}