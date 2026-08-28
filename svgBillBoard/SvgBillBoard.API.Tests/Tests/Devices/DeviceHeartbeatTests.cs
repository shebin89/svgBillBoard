using System.IdentityModel.Tokens.Jwt;
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

public class DeviceHeartbeatTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    private readonly CustomWebApplicationFactory _factory;

    public DeviceHeartbeatTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }


    // =========================================================
    // WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task Heartbeat_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.PostAsync(
                "/api/devices/heartbeat",
                content: null);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // INVALID DEVICE TOKEN
    // =========================================================

    [Fact]
    public async Task Heartbeat_WithInvalidDeviceToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                "this-is-not-a-valid-jwt");

        // Act
        var response =
            await _client.PostAsync(
                "/api/devices/heartbeat",
                content: null);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // NORMAL USER TOKEN
    // =========================================================

    [Fact]
    public async Task Heartbeat_WithNormalUserToken_ShouldReturnForbidden()
    {
        // Arrange
        var email =
            $"heartbeat-user-{Guid.NewGuid():N}@example.com";

        var password =
            "Test@12345";


        // Register user
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    OrganizationId =
                        TestDataSeeder.OrganizationId,

                    FirstName =
                        "Heartbeat",

                    LastName =
                        "User",

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


        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        loginResult.Should()
            .NotBeNull();

        loginResult!.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();


        // Authenticate as normal user
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult.AccessToken);


        // Act
        var response =
            await _client.PostAsync(
                "/api/devices/heartbeat",
                content: null);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }


    // =========================================================
    // VALID DEVICE TOKEN
    // =========================================================

    [Fact]
    public async Task Heartbeat_WithValidDeviceToken_ShouldReturnSuccess()
    {
        // Arrange
        var pairing =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization = null;

        var deviceIdentifier =
            $"HEARTBEAT-{Guid.NewGuid():N}";


        // Pair device
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
                        "Heartbeat Test Device",

                    DeviceType =
                        "Billboard",

                    Platform =
                        "Windows",

                    AppVersion =
                        "1.0.0"
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


        // Authenticate as device
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                pairResult.DeviceToken);


        // Act
        var response =
            await _client.PostAsync(
                "/api/devices/heartbeat",
                content: null);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);


        var result =
            await response.Content
                .ReadFromJsonAsync<
                    DeviceHeartbeatResponse>();

        result.Should()
            .NotBeNull();

        result!.DeviceId
            .Should()
            .Be(pairResult.Device.Id);

        result.IsOnline
            .Should()
            .BeTrue();

        result.ServerTime
            .Should()
            .BeCloseTo(
                DateTime.UtcNow,
                TimeSpan.FromSeconds(5));
    }


    // =========================================================
    // HEARTBEAT UPDATES DATABASE
    // =========================================================

    [Fact]
    public async Task Heartbeat_WithValidDeviceToken_ShouldUpdateLastSeenAt()
    {
        // Arrange
        var pairing =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization = null;

        var deviceIdentifier =
            $"LASTSEEN-{Guid.NewGuid():N}";


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
                        "Last Seen Test Device",

                    DeviceType =
                        "Billboard",

                    Platform =
                        "Windows",

                    AppVersion =
                        "1.0.0"
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


        var deviceId =
            pairResult.Device.Id;


        // Authenticate as device
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                pairResult.DeviceToken);


        // Act
        var response =
            await _client.PostAsync(
                "/api/devices/heartbeat",
                content: null);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);


        var result =
            await response.Content
                .ReadFromJsonAsync<
                    DeviceHeartbeatResponse>();

        result.Should()
            .NotBeNull();

        result!.DeviceId
            .Should()
            .Be(deviceId);

        result.IsOnline
            .Should()
            .BeTrue();


        // Verify database
        using var scope =
            _factory.Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();


        var device =
            await context.Devices
                .FirstAsync(
                    x => x.Id == deviceId);


        device.LastHeartbeatAt
            .Should()
            .NotBeNull();

        device.LastSeenAt
            .Should()
            .NotBeNull();

        device.IsOnline
            .Should()
            .BeTrue();

        device.LastOnlineAt
            .Should()
            .NotBeNull();

        device.UpdatedAt
            .Should()
            .BeCloseTo(
                DateTime.UtcNow,
                TimeSpan.FromSeconds(5));
    }


    // =========================================================
    // INACTIVE DEVICE
    // =========================================================

    [Fact]
    public async Task Heartbeat_WithInactiveDevice_ShouldReturnNotFound()
    {
        // Arrange
        var pairing =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization = null;

        var deviceIdentifier =
            $"INACTIVE-{Guid.NewGuid():N}";


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
                        "Inactive Heartbeat Device",

                    DeviceType =
                        "Billboard",

                    Platform =
                        "Windows",

                    AppVersion =
                        "1.0.0"
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


        var deviceId =
            pairResult.Device.Id;


        // Deactivate device
        using (var scope =
               _factory.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

            var device =
                await context.Devices
                    .FirstAsync(
                        x => x.Id == deviceId);

            device.Status = 0;

            await context.SaveChangesAsync();
        }


        // Authenticate as device
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                pairResult.DeviceToken);


        // Act
        var response =
            await _client.PostAsync(
                "/api/devices/heartbeat",
                content: null);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // DEVICE JWT CLAIMS
    // =========================================================

    [Fact]
    public async Task PairDevice_DeviceToken_ShouldContainCorrectClaims()
    {
        // Arrange
        var pairing =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization = null;

        var deviceIdentifier =
            $"CLAIMS-{Guid.NewGuid():N}";


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
                        "Claims Test Device"
                });


        pairResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);


        var result =
            await pairResponse.Content
                .ReadFromJsonAsync<
                    DeviceAuthenticationResponse>();

        result.Should()
            .NotBeNull();

        result!.DeviceToken
            .Should()
            .NotBeNullOrWhiteSpace();


        // Read JWT
        var handler =
            new JwtSecurityTokenHandler();

        handler.CanReadToken(
                result.DeviceToken)
            .Should()
            .BeTrue();


        var jwt =
            handler.ReadJwtToken(
                result.DeviceToken);


        // Required device claim
        jwt.Claims
            .Should()
            .Contain(x =>
                x.Type == "device" &&
                x.Value == "true");


        // Device ID
        jwt.Claims
            .Should()
            .Contain(x =>
                x.Type == "deviceId" &&
                x.Value ==
                result.Device.Id.ToString());


        // Organization ID
        jwt.Claims
            .Should()
            .Contain(x =>
                x.Type == "organizationId" &&
                x.Value ==
                TestDataSeeder
                    .OrganizationId
                    .ToString());


        // Location ID
        jwt.Claims
            .Should()
            .Contain(x =>
                x.Type == "locationId" &&
                x.Value ==
                TestDataSeeder
                    .OrganizationLocationId
                    .ToString());


        // Device code
        jwt.Claims
            .Should()
            .Contain(x =>
                x.Type == "deviceCode" &&
                x.Value ==
                result.Device.DeviceCode);


        // Device identifier
        jwt.Claims
            .Should()
            .Contain(x =>
                x.Type == "deviceIdentifier" &&
                x.Value ==
                deviceIdentifier);


        // Expiration
        jwt.ValidTo
            .Should()
            .BeAfter(DateTime.UtcNow);
    }


    // =========================================================
    // HELPER
    // =========================================================

    private async Task<PairingResponse>
        CreatePairingAsync()
    {
        var email =
            $"heartbeat-pairing-{Guid.NewGuid():N}@example.com";

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
                        "Heartbeat",

                    LastName =
                        "Pairing",

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


        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();


        loginResult.Should()
            .NotBeNull();

        loginResult!.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();


        // Authenticate user
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult.AccessToken);


        // Generate pairing code
        var pairingResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pairing",
                new GeneratePairingRequest
                {
                    LocationId =
                        TestDataSeeder
                            .OrganizationLocationId
                });


        pairingResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);


        var pairing =
            await pairingResponse.Content
                .ReadFromJsonAsync<PairingResponse>();


        pairing.Should()
            .NotBeNull();

        pairing!.PairingCode
            .Should()
            .NotBeNullOrWhiteSpace();


        return pairing;
    }
}