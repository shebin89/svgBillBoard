using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Authentication;
using SvgBillBoard.Application.DTOs.Devices;
using SvgBillBoard.Infrastructure.Persistence;

namespace SvgBillBoard.API.Tests.Tests.Devices;

public class DeviceAuthenticationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    private readonly CustomWebApplicationFactory _factory;

    public DeviceAuthenticationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }


    // =========================================================
    // VALID DEVICE TOKEN
    // =========================================================

    [Fact]
    public async Task Login_WithValidDeviceToken_ShouldReturnSuccess()
    {
        // Arrange
        var pairing =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization = null;

        var deviceIdentifier =
            $"AUTH-{Guid.NewGuid():N}";

        var pairResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        pairing.PairingCode,

                    DeviceIdentifier =
                        deviceIdentifier,

                    Name =
                        "Authentication Test Device"
                });

        pairResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var pairResult =
            await pairResponse.Content
                .ReadFromJsonAsync<
                    DeviceAuthenticationResponse>();

        pairResult.Should()
            .NotBeNull();

        pairResult!.DeviceToken
            .Should()
            .NotBeNullOrWhiteSpace();


        // Act
        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/device-auth/login",
                new DeviceLoginRequest
                {
                    DeviceToken =
                        pairResult.DeviceToken
                });


        // Assert
        loginResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await loginResponse.Content
                .ReadFromJsonAsync<
                    DeviceLoginResponse>();

        result.Should()
            .NotBeNull();

        result!.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();

        result.Device
            .Should()
            .NotBeNull();

        result.Device.Id
            .Should()
            .Be(pairResult.Device.Id);

        result.Device.OrganizationId
            .Should()
            .Be(TestDataSeeder.OrganizationId);

        result.Device.LocationId
            .Should()
            .Be(TestDataSeeder.OrganizationLocationId);

        result.ExpiresAt
            .Should()
            .BeAfter(DateTime.UtcNow);
    }


    // =========================================================
    // EMPTY DEVICE TOKEN
    // =========================================================

    [Fact]
    public async Task Login_WithEmptyDeviceToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        var request =
            new DeviceLoginRequest
            {
                DeviceToken =
                    string.Empty
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/device-auth/login",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // WHITESPACE DEVICE TOKEN
    // =========================================================

    [Fact]
    public async Task Login_WithWhitespaceDeviceToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        var request =
            new DeviceLoginRequest
            {
                DeviceToken =
                    "   "
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/device-auth/login",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // INVALID DEVICE TOKEN
    // =========================================================

    [Fact]
    public async Task Login_WithInvalidDeviceToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        var request =
            new DeviceLoginRequest
            {
                DeviceToken =
                    "invalid-device-token"
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/device-auth/login",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // VALID DEVICE TOKEN UPDATES LAST USED
    // =========================================================

    [Fact]
    public async Task Login_WithValidDeviceToken_ShouldUpdateLastUsedAt()
    {
        // Arrange
        var pairing =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization = null;

        var pairResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        pairing.PairingCode,

                    DeviceIdentifier =
                        $"LASTUSED-{Guid.NewGuid():N}",

                    Name =
                        "Last Used Test Device"
                });

        pairResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var pairResult =
            await pairResponse.Content
                .ReadFromJsonAsync<
                    DeviceAuthenticationResponse>();

        pairResult.Should()
            .NotBeNull();

        var deviceId =
            pairResult!.Device.Id;


        // Act
        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/device-auth/login",
                new DeviceLoginRequest
                {
                    DeviceToken =
                        pairResult.DeviceToken
                });


        // Assert
        loginResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);


        // Verify database
        using var scope =
            _factory.Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var credential =
            await context.DeviceCredentials
                .FirstOrDefaultAsync(
                    x => x.DeviceId == deviceId);

        credential.Should()
            .NotBeNull();

        credential!.LastUsedAt
            .Should()
            .NotBeNull();

        credential.LastUsedAt
            .Should()
            .BeCloseTo(
                DateTime.UtcNow,
                TimeSpan.FromSeconds(5));
    }


    // =========================================================
    // REVOKED CREDENTIAL
    // =========================================================

    [Fact]
    public async Task Login_WithRevokedDeviceCredential_ShouldReturnUnauthorized()
    {
        // Arrange
        var pairing =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization = null;

        var pairResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        pairing.PairingCode,

                    DeviceIdentifier =
                        $"REVOKED-{Guid.NewGuid():N}",

                    Name =
                        "Revoked Device"
                });

        pairResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var pairResult =
            await pairResponse.Content
                .ReadFromJsonAsync<
                    DeviceAuthenticationResponse>();

        pairResult.Should()
            .NotBeNull();


        // Revoke credential
        using (var scope =
               _factory.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

            var credential =
                await context.DeviceCredentials
                    .FirstOrDefaultAsync(
                        x =>
                            x.DeviceId ==
                            pairResult!.Device.Id);

            credential.Should()
                .NotBeNull();

            credential!.RevokedAt =
                DateTime.UtcNow;

            await context.SaveChangesAsync();
        }


        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/device-auth/login",
                new DeviceLoginRequest
                {
                    DeviceToken =
                        pairResult!.DeviceToken
                });


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // EXPIRED CREDENTIAL
    // =========================================================

    [Fact]
    public async Task Login_WithExpiredDeviceCredential_ShouldReturnUnauthorized()
    {
        // Arrange
        var pairing =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization = null;

        var pairResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        pairing.PairingCode,

                    DeviceIdentifier =
                        $"EXPIRED-{Guid.NewGuid():N}",

                    Name =
                        "Expired Device"
                });

        pairResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var pairResult =
            await pairResponse.Content
                .ReadFromJsonAsync<
                    DeviceAuthenticationResponse>();

        pairResult.Should()
            .NotBeNull();


        // Expire credential
        using (var scope =
               _factory.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

            var credential =
                await context.DeviceCredentials
                    .FirstOrDefaultAsync(
                        x =>
                            x.DeviceId ==
                            pairResult!.Device.Id);

            credential.Should()
                .NotBeNull();

            credential!.ExpiresAt =
                DateTime.UtcNow.AddMinutes(-1);

            await context.SaveChangesAsync();
        }


        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/device-auth/login",
                new DeviceLoginRequest
                {
                    DeviceToken =
                        pairResult!.DeviceToken
                });


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // INACTIVE DEVICE
    // =========================================================

    [Fact]
    public async Task Login_WithInactiveDevice_ShouldReturnUnauthorized()
    {
        // Arrange
        var pairing =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization = null;

        var pairResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        pairing.PairingCode,

                    DeviceIdentifier =
                        $"INACTIVE-{Guid.NewGuid():N}",

                    Name =
                        "Inactive Device"
                });

        pairResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var pairResult =
            await pairResponse.Content
                .ReadFromJsonAsync<
                    DeviceAuthenticationResponse>();

        pairResult.Should()
            .NotBeNull();


        // Deactivate device
        using (var scope =
               _factory.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

            var device =
                await context.Devices
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                            pairResult!.Device.Id);

            device.Should()
                .NotBeNull();

            device!.Status = 0;

            await context.SaveChangesAsync();
        }


        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/device-auth/login",
                new DeviceLoginRequest
                {
                    DeviceToken =
                        pairResult!.DeviceToken
                });


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // HELPERS
    // =========================================================

    private async Task<PairingResponse>
        CreatePairingAsync()
    {
        await AuthenticateUserAsync();

        var response =
            await _client.PostAsJsonAsync(
                "/api/devices/pairing",
                new GeneratePairingRequest
                {
                    LocationId =
                        TestDataSeeder
                            .OrganizationLocationId
                });

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var pairing =
            await response.Content
                .ReadFromJsonAsync<PairingResponse>();

        pairing.Should()
            .NotBeNull();

        pairing!.PairingCode
            .Should()
            .NotBeNullOrWhiteSpace();

        return pairing;
    }


    private async Task AuthenticateUserAsync()
    {
        var email =
            $"device-auth-{Guid.NewGuid():N}@example.com";

        var password =
            "Test@12345";


        // Register
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    OrganizationId =
                        TestDataSeeder.OrganizationId,

                    FirstName =
                        "Device",

                    LastName =
                        "Authentication",

                    Email =
                        email,

                    Password =
                        password,

                    Phone =
                        "9876543210",

                    RoleCode =
                        "VIEWER"
                });

        registerResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);


        // Login
        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    OrganizationId =
                        TestDataSeeder.OrganizationId,

                    Email =
                        email,

                    Password =
                        password
                });

        loginResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var login =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        login.Should()
            .NotBeNull();

        login!.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();


        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);
    }
}