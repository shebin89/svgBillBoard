using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Devices;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SvgBillBoard.Application.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;

    private readonly IDevicePairingRepository
        _pairingRepository;

    private readonly ILocationRepository
        _locationRepository;

    private readonly IDeviceCredentialRepository
        _credentialRepository;

    private readonly IDeviceJwtService
        _deviceJwtService;


    public DeviceService(
        IDeviceRepository deviceRepository,
        IDevicePairingRepository pairingRepository,
        ILocationRepository locationRepository,
        IDeviceCredentialRepository credentialRepository,
        IDeviceJwtService deviceJwtService)
    {
        _deviceRepository =
            deviceRepository;

        _pairingRepository =
            pairingRepository;

        _locationRepository =
            locationRepository;

        _credentialRepository =
            credentialRepository;

        _deviceJwtService =
            deviceJwtService;
    }


    public async Task<PairingResponse>
        GeneratePairingAsync(
            Guid organizationId,
            GeneratePairingRequest request)
    {
        var location =
            await _locationRepository
                .GetByIdAsync(
                    request.LocationId);

        if (location == null ||
            location.OrganizationId != organizationId)
        {
            throw new InvalidOperationException(
                "Location was not found.");
        }

        var pairingCode =
            GeneratePairingCode();

        var pairing =
            new DevicePairing
            {
                Id =
                    Guid.NewGuid(),

                OrganizationId =
                    organizationId,

                LocationId =
                    request.LocationId,

                PairingCode =
                    pairingCode,

                CreatedAt =
                    DateTime.UtcNow,

                ExpiresAt =
                    DateTime.UtcNow
                        .AddMinutes(10),

                Status = 1
            };

        await _pairingRepository
            .AddAsync(pairing);

        await _pairingRepository
            .SaveChangesAsync();

        return new PairingResponse
        {
            PairingCode =
                pairing.PairingCode,

            LocationId =
                pairing.LocationId,

            ExpiresAt =
                pairing.ExpiresAt
        };
    }


    public async Task<DeviceAuthenticationResponse>
        PairDeviceAsync(
            PairDeviceRequest request)
    {
        // ---------------------------------------------------------
        // Validate request
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(
                request.PairingCode))
        {
            throw new ArgumentException(
                "Pairing code is required.");
        }

        if (string.IsNullOrWhiteSpace(
                request.DeviceIdentifier))
        {
            throw new ArgumentException(
                "Device identifier is required.");
        }


        // ---------------------------------------------------------
        // Normalize pairing code
        // ---------------------------------------------------------

        var code =
            request.PairingCode
                .Trim()
                .ToUpperInvariant();


        // ---------------------------------------------------------
        // Find pairing
        // ---------------------------------------------------------

        var pairing =
            await _pairingRepository
                .GetByCodeAsync(code);

        if (pairing == null)
        {
            throw new InvalidOperationException(
                "Invalid pairing code.");
        }


        // ---------------------------------------------------------
        // Check pairing status
        // ---------------------------------------------------------

        if (pairing.Status != 1)
        {
            throw new InvalidOperationException(
                "Pairing code has already been used.");
        }


        // ---------------------------------------------------------
        // Check expiration
        // ---------------------------------------------------------

        if (pairing.ExpiresAt <
            DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "Pairing code has expired.");
        }


        // ---------------------------------------------------------
        // Check duplicate device
        // ---------------------------------------------------------

        var existingDevice =
            await _deviceRepository
                .ExistsByIdentifierAsync(
                    request.DeviceIdentifier);

        if (existingDevice)
        {
            throw new InvalidOperationException(
                "This device is already registered.");
        }


        // ---------------------------------------------------------
        // Generate unique device code
        // ---------------------------------------------------------

        var deviceCode =
            await GenerateUniqueDeviceCodeAsync(
                pairing.OrganizationId);


        // ---------------------------------------------------------
        // Create device
        // ---------------------------------------------------------

        var device =
            new Device
            {
                Id =
                    Guid.NewGuid(),

                OrganizationId =
                    pairing.OrganizationId,

                LocationId =
                    pairing.LocationId,

                Name =
                    string.IsNullOrWhiteSpace(
                        request.Name)
                        ? $"Device {deviceCode}"
                        : request.Name.Trim(),

                DeviceIdentifier =
                    request.DeviceIdentifier.Trim(),

                DeviceCode =
                    deviceCode,

                DeviceType =
                    request.DeviceType?.Trim(),

                Platform =
                    request.Platform?.Trim(),

                AppVersion =
                    request.AppVersion?.Trim(),

                Model =
                    request.Model?.Trim(),

                Manufacturer =
                    request.Manufacturer?.Trim(),

                SerialNumber =
                    request.SerialNumber?.Trim(),

                MacAddress =
                    request.MacAddress?.Trim(),

                IpAddress =
                    request.IpAddress?.Trim(),

                Status = 1,

                CreatedAt =
                    DateTime.UtcNow,

                UpdatedAt =
                    DateTime.UtcNow
            };


        // ---------------------------------------------------------
        // Generate JWT AFTER device exists
        // ---------------------------------------------------------

        var deviceToken =
            _deviceJwtService
                .GenerateToken(device);


        // ---------------------------------------------------------
        // Store only token hash
        // ---------------------------------------------------------

        var tokenHash =
            HashToken(deviceToken);


        // ---------------------------------------------------------
        // Add device
        // ---------------------------------------------------------

        await _deviceRepository
            .AddAsync(device);


        // ---------------------------------------------------------
        // Create device credential
        // ---------------------------------------------------------

        var credential =
            new DeviceCredential
            {
                Id =
                    Guid.NewGuid(),

                DeviceId =
                    device.Id,

                TokenHash =
                    tokenHash,

                CreatedAt =
                    DateTime.UtcNow,

                ExpiresAt =
                    null,

                LastUsedAt =
                    null,

                RevokedAt =
                    null
            };

        await _credentialRepository
            .AddAsync(credential);


        // ---------------------------------------------------------
        // Consume pairing code
        // ---------------------------------------------------------

        pairing.DeviceId =
            device.Id;

        pairing.UsedAt =
            DateTime.UtcNow;

        pairing.Status = 0;

        await _pairingRepository
            .UpdateAsync(pairing);


        // ---------------------------------------------------------
        // Save changes
        // ---------------------------------------------------------

        await _deviceRepository
            .SaveChangesAsync();

        await _credentialRepository
            .SaveChangesAsync();

        await _pairingRepository
            .SaveChangesAsync();


        // ---------------------------------------------------------
        // Return device authentication response
        // ---------------------------------------------------------

        return new DeviceAuthenticationResponse
        {
            Device =
                Map(device),

            DeviceToken =
                deviceToken,

            ExpiresAt =
                credential.ExpiresAt
        };
    }


    public async Task<List<DeviceResponse>>
        GetAllAsync(
            Guid organizationId)
    {
        var devices =
            await _deviceRepository
                .GetByOrganizationIdAsync(
                    organizationId);

        return devices
            .Select(Map)
            .ToList();
    }


    public async Task<DeviceResponse?>
        GetByIdAsync(
            Guid organizationId,
            Guid id)
    {
        var device =
            await _deviceRepository
                .GetByIdAsync(id);

        if (device == null ||
            device.OrganizationId !=
                organizationId)
        {
            return null;
        }

        return Map(device);
    }


    private async Task<string>
        GenerateUniqueDeviceCodeAsync(
            Guid organizationId)
    {
        string code;

        do
        {
            var number =
                RandomNumberGenerator
                    .GetInt32(
                        100000,
                        999999);

            code =
                $"TV-{number}";

        } while (
            await _deviceRepository
                .ExistsByCodeAsync(
                    organizationId,
                    code));

        return code;
    }


    private static string
        GeneratePairingCode()
    {
        const string characters =
            "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        Span<char> result =
            stackalloc char[6];

        for (
            var i = 0;
            i < result.Length;
            i++)
        {
            result[i] =
                characters[
                    RandomNumberGenerator
                        .GetInt32(
                            characters.Length)];
        }

        return new string(result);
    }


    private static string
        HashToken(
            string token)
    {
        var bytes =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(
                    token));

        return Convert.ToHexString(bytes);
    }


    private static DeviceResponse
        Map(
            Device device)
    {
        return new DeviceResponse
        {
            Id =
                device.Id,

            OrganizationId =
                device.OrganizationId,

            LocationId =
                device.LocationId,

            Name =
                device.Name,

            DeviceIdentifier =
                device.DeviceIdentifier,

            DeviceCode =
                device.DeviceCode,

            DeviceType =
                device.DeviceType,

            Platform =
                device.Platform,

            AppVersion =
                device.AppVersion,

            Model =
                device.Model,

            Manufacturer =
                device.Manufacturer,

            SerialNumber =
                device.SerialNumber,

            MacAddress =
                device.MacAddress,

            IpAddress =
                device.IpAddress,

            LastSeenAt =
                device.LastSeenAt,

            Status =
                device.Status,

            CreatedAt =
                device.CreatedAt,

            UpdatedAt =
                device.UpdatedAt
        };
    }
}