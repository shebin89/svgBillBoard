    using System.Security.Cryptography;
    using System.Text;
    using SvgBillBoard.Application.Abstractions.Services;
    using SvgBillBoard.Application.DTOs.Devices;
    using SvgBillBoard.Domain.Interfaces;

    namespace SvgBillBoard.Application.Services;

    public class DeviceAuthenticationService
        : IDeviceAuthenticationService
    {
        private readonly IDeviceCredentialRepository
            _credentialRepository;

        private readonly IDeviceJwtService
            _deviceJwtService;

        public DeviceAuthenticationService(
            IDeviceCredentialRepository credentialRepository,
            IDeviceJwtService deviceJwtService)
        {
            _credentialRepository =
                credentialRepository;

            _deviceJwtService =
                deviceJwtService;
        }

        public async Task<DeviceLoginResponse> LoginAsync(
            DeviceLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.DeviceToken))
            {
                throw new UnauthorizedAccessException(
                    "Device token is required.");
            }

            var tokenHash =
                HashToken(request.DeviceToken);

            var credential =
                await _credentialRepository
                    .GetByTokenHashAsync(tokenHash);

            if (credential == null)
            {
                throw new UnauthorizedAccessException(
                    "Invalid device token.");
            }

            if (credential.RevokedAt != null)
            {
                throw new UnauthorizedAccessException(
                    "Device credential has been revoked.");
            }

            if (credential.ExpiresAt != null &&
                credential.ExpiresAt <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException(
                    "Device credential has expired.");
            }

            if (credential.Device == null)
            {
                throw new UnauthorizedAccessException(
                    "Device was not found.");
            }

            var device = credential.Device;

            if (device.Status != 1)
            {
                throw new UnauthorizedAccessException(
                    "Device is inactive.");
            }

            credential.LastUsedAt =
                DateTime.UtcNow;

            await _credentialRepository
                .UpdateAsync(credential);

            await _credentialRepository
                .SaveChangesAsync();

            var accessToken =
                _deviceJwtService.GenerateToken(device);

            var expiresAt =
                DateTime.UtcNow.AddDays(30);

            return new DeviceLoginResponse
            {
                AccessToken = accessToken,

                ExpiresAt = expiresAt,

                Device = new DeviceResponse
                {
                    Id = device.Id,
                    OrganizationId =
                        device.OrganizationId,
                    LocationId =
                        device.LocationId,
                    Name = device.Name,
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
                }
            };
        }

        private static string HashToken(
            string token)
        {
            var bytes =
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(token));

            return Convert.ToHexString(bytes);
        }
    }