using FluentAssertions;
using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Authentication;
using SvgBillBoard.Application.DTOs.Devices;
using SvgBillBoard.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;

namespace SvgBillBoard.API.Tests.Tests.Devices;

public class DevicePairingTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public DevicePairingTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GeneratePairing_WithValidUser_ShouldReturnPairingCode()
    {
        // Arrange

        var email =
            $"pairing-{Guid.NewGuid()}@example.com";

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
                        "Pairing",

                    LastName =
                        "Test",

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

        // Authenticate
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult.AccessToken);

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/devices/pairing",
                new GeneratePairingRequest
                {
                    LocationId =
                        TestDataSeeder.OrganizationLocationId
                });

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<PairingResponse>();

        result.Should()
            .NotBeNull();

        result!.PairingCode
            .Should()
            .NotBeNullOrWhiteSpace();

        result.PairingCode
            .Should()
            .HaveLength(6);

        result.LocationId
            .Should()
            .Be(TestDataSeeder.OrganizationLocationId);

        result.ExpiresAt
            .Should()
            .BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task PairDevice_WithValidPairingCode_ShouldCreateDevice()
    {
        // Arrange

        // 1. Register user
        var email =
            $"device-pair-{Guid.NewGuid()}@example.com";

        var password =
            "Test@12345";

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
                        "Owner",

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


        // 2. Login
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


        // 3. Authenticate user
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult!.AccessToken);


        // 4. Generate pairing code
        var pairingResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pairing",
                new GeneratePairingRequest
                {
                    LocationId =
                        TestDataSeeder.OrganizationLocationId
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


        // 5. Remove user JWT
        _client.DefaultRequestHeaders.Authorization = null;


        // 6. Pair device anonymously
        var deviceIdentifier =
            $"DEVICE-{Guid.NewGuid():N}";

        var pairRequest =
            new PairDeviceRequest
            {
                PairingCode =
                    pairing.PairingCode,

                DeviceIdentifier =
                    deviceIdentifier,

                Name =
                    "Test Device",

                DeviceType =
                    "Billboard",

                Platform =
                    "Windows",

                AppVersion =
                    "1.0.0",

                Model =
                    "Test Model",

                Manufacturer =
                    "Test Manufacturer",

                SerialNumber =
                    $"SN-{Guid.NewGuid():N}",

                MacAddress =
                    "00:11:22:33:44:55",

                IpAddress =
                    "192.168.1.100"
            };


        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                pairRequest);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<DeviceAuthenticationResponse>();

        result.Should()
            .NotBeNull();

        result!.Device.Should()
            .NotBeNull();

        result.Device.DeviceIdentifier
            .Should()
            .Be(deviceIdentifier);

        result.Device.Name
            .Should()
            .Be("Test Device");

        result.Device.OrganizationId
            .Should()
            .Be(TestDataSeeder.OrganizationId);

        result.Device.LocationId
            .Should()
            .Be(TestDataSeeder.OrganizationLocationId);

        result.Device.Status
            .Should()
            .Be(1);

        result.Device.DeviceCode
            .Should()
            .NotBeNullOrWhiteSpace();

        result.DeviceToken
            .Should()
            .NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PairDevice_WithAlreadyUsedPairingCode_ShouldReturnConflict()
    {
        // Arrange
        var email =
            $"reuse-pairing-{Guid.NewGuid()}@example.com";

        var password = "Test@12345";

        // Register user
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    OrganizationId =
                        TestDataSeeder.OrganizationId,

                    FirstName = "Pairing",
                    LastName = "Reuse",

                    Email = email,
                    Password = password,

                    Phone = "9876543210",
                    RoleCode = "VIEWER"
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

                    Email = email,
                    Password = password
                });

        loginResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        loginResult.Should()
            .NotBeNull();

        // Authenticate
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult!.AccessToken);

        // Generate pairing code
        var pairingResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pairing",
                new GeneratePairingRequest
                {
                    LocationId =
                        TestDataSeeder.OrganizationLocationId
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

        // Remove user JWT because /pair is anonymous
        _client.DefaultRequestHeaders.Authorization = null;

        // First pairing
        var firstPairResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        pairing.PairingCode,

                    DeviceIdentifier =
                        $"DEVICE-{Guid.NewGuid():N}",

                    Name =
                        "First Device",

                    DeviceType =
                        "Billboard",

                    Platform =
                        "Windows",

                    AppVersion =
                        "1.0.0"
                });

        firstPairResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Second pairing using the SAME pairing code
        var secondPairResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        pairing.PairingCode,

                    DeviceIdentifier =
                        $"DEVICE-{Guid.NewGuid():N}",

                    Name =
                        "Second Device",

                    DeviceType =
                        "Billboard",

                    Platform =
                        "Windows",

                    AppVersion =
                        "1.0.0"
                });

        // Assert
        secondPairResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PairDevice_WithInvalidPairingCode_ShouldReturnConflict()
    {
        // Arrange
        var request = new PairDeviceRequest
        {
            PairingCode = "XXXXXX",

            DeviceIdentifier =
                $"INVALID-{Guid.NewGuid():N}",

            Name = "Invalid Pairing Device",

            DeviceType = "Billboard",

            Platform = "Windows",

            AppVersion = "1.0.0"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PairDevice_WithoutPairingCode_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new PairDeviceRequest
        {
            PairingCode = "",

            DeviceIdentifier =
                $"NO-CODE-{Guid.NewGuid():N}",

            Name = "No Pairing Code Device",

            DeviceType = "Billboard",

            Platform = "Windows",

            AppVersion = "1.0.0"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PairDevice_WithoutDeviceIdentifier_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new PairDeviceRequest
        {
            PairingCode = "XXXXXX",

            DeviceIdentifier = "",

            Name = "Missing Identifier Device",

            DeviceType = "Billboard",

            Platform = "Windows",

            AppVersion = "1.0.0"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PairDevice_WithDuplicateDeviceIdentifier_ShouldReturnConflict()
    {
        // Arrange
        var email =
            $"duplicate-device-{Guid.NewGuid()}@example.com";

        var password = "Test@12345";

        // Register user
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    OrganizationId =
                        TestDataSeeder.OrganizationId,

                    FirstName = "Duplicate",
                    LastName = "Device",

                    Email = email,
                    Password = password,

                    Phone = "9876543210",
                    RoleCode = "VIEWER"
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

                    Email = email,
                    Password = password
                });

        loginResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        loginResult.Should()
            .NotBeNull();

        // Authenticate
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult!.AccessToken);

        // Generate first pairing code
        var pairingResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pairing",
                new GeneratePairingRequest
                {
                    LocationId =
                        TestDataSeeder.OrganizationLocationId
                });

        pairingResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var pairing =
            await pairingResponse.Content
                .ReadFromJsonAsync<PairingResponse>();

        pairing.Should()
            .NotBeNull();

        // Remove user JWT
        _client.DefaultRequestHeaders.Authorization = null;

        var deviceIdentifier =
            $"DEVICE-DUPLICATE-{Guid.NewGuid():N}";

        // First pairing
        var firstPairResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        pairing!.PairingCode,

                    DeviceIdentifier =
                        deviceIdentifier,

                    Name =
                        "First Device",

                    DeviceType =
                        "Billboard",

                    Platform =
                        "Windows",

                    AppVersion =
                        "1.0.0"
                });

        firstPairResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Get a NEW pairing code
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult.AccessToken);

        var secondPairingResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pairing",
                new GeneratePairingRequest
                {
                    LocationId =
                        TestDataSeeder.OrganizationLocationId
                });

        secondPairingResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var secondPairing =
            await secondPairingResponse.Content
                .ReadFromJsonAsync<PairingResponse>();

        secondPairing.Should()
            .NotBeNull();

        // Remove user JWT
        _client.DefaultRequestHeaders.Authorization = null;

        // Act - try registering same device identifier
        var secondPairResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        secondPairing!.PairingCode,

                    DeviceIdentifier =
                        deviceIdentifier,

                    Name =
                        "Second Device",

                    DeviceType =
                        "Billboard",

                    Platform =
                        "Windows",

                    AppVersion =
                        "1.0.0"
                });

        // Assert
        secondPairResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PairDevice_WithExpiredPairingCode_ShouldReturnConflict()
    {
        // Arrange
        var email =
            $"expired-pairing-{Guid.NewGuid()}@example.com";

        var password = "Test@12345";

        // Register
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    OrganizationId =
                        TestDataSeeder.OrganizationId,

                    FirstName = "Expired",
                    LastName = "Pairing",

                    Email = email,
                    Password = password,

                    Phone = "9876543210",
                    RoleCode = "VIEWER"
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

                    Email = email,
                    Password = password
                });

        loginResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        loginResult.Should()
            .NotBeNull();

        // Authenticate
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult!.AccessToken);

        // Generate pairing code
        var pairingResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pairing",
                new GeneratePairingRequest
                {
                    LocationId =
                        TestDataSeeder.OrganizationLocationId
                });

        pairingResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var pairing =
            await pairingResponse.Content
                .ReadFromJsonAsync<PairingResponse>();

        pairing.Should()
            .NotBeNull();

        // Expire pairing code directly in test database
        using var scope =
            _factory.Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var pairingEntity =
            await context.DevicePairings
                .FirstAsync(
                    x => x.PairingCode ==
                         pairing!.PairingCode);

        pairingEntity.ExpiresAt =
            DateTime.UtcNow.AddMinutes(-1);

        await context.SaveChangesAsync();

        // Remove user JWT
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        pairing.PairingCode,

                    DeviceIdentifier =
                        $"EXPIRED-{Guid.NewGuid():N}",

                    Name =
                        "Expired Device",

                    DeviceType =
                        "Billboard",

                    Platform =
                        "Windows",

                    AppVersion =
                        "1.0.0"
                });

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetAllDevices_ShouldReturnOnlyCurrentOrganizationDevices()
    {
        // Arrange
        var email =
            $"get-devices-{Guid.NewGuid()}@example.com";

        var password = "Test@12345";

        // Register Organization A user
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    OrganizationId =
                        TestDataSeeder.OrganizationId,

                    FirstName = "Device",
                    LastName = "Viewer",

                    Email = email,
                    Password = password,

                    Phone = "9876543210",
                    RoleCode = "VIEWER"
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

                    Email = email,
                    Password = password
                });

        loginResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        loginResult.Should()
            .NotBeNull();

        // Authenticate
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult!.AccessToken);

        // Generate pairing code
        var pairingResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pairing",
                new GeneratePairingRequest
                {
                    LocationId =
                        TestDataSeeder.OrganizationLocationId
                });

        pairingResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var pairing =
            await pairingResponse.Content
                .ReadFromJsonAsync<PairingResponse>();

        pairing.Should()
            .NotBeNull();

        // Remove user JWT
        _client.DefaultRequestHeaders.Authorization = null;

        // Pair device
        var deviceIdentifier =
            $"GET-DEVICES-{Guid.NewGuid():N}";

        var pairResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        pairing!.PairingCode,

                    DeviceIdentifier =
                        deviceIdentifier,

                    Name =
                        "Organization A Device",

                    DeviceType =
                        "Billboard",

                    Platform =
                        "Windows",

                    AppVersion =
                        "1.0.0",

                    Model =
                        "Test Model",

                    Manufacturer =
                        "Test Manufacturer"
                });

        pairResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Authenticate again as Organization A user
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult.AccessToken);

        // Act
        var response =
            await _client.GetAsync(
                "/api/devices");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var devices =
            await response.Content
                .ReadFromJsonAsync<List<DeviceResponse>>();

        devices.Should()
            .NotBeNull();

        // Organization A device should exist
        devices!
            .Should()
            .Contain(x =>
                x.DeviceIdentifier ==
                deviceIdentifier);

        // Every returned device must belong to Organization A
        devices
            .Should()
            .OnlyContain(x =>
                x.OrganizationId ==
                TestDataSeeder.OrganizationId);

        // Organization B devices must not be returned
        devices
            .Should()
            .NotContain(x =>
                x.OrganizationId ==
                TestDataSeeder.SecondOrganizationId);
    }

    [Fact]
    public async Task GetDeviceById_FromOwnOrganization_ShouldReturnDevice()
    {
        // Arrange
        var email =
            $"get-device-{Guid.NewGuid()}@example.com";

        var password = "Test@12345";

        // Register
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    OrganizationId =
                        TestDataSeeder.OrganizationId,

                    FirstName = "Get",
                    LastName = "Device",

                    Email = email,
                    Password = password,

                    Phone = "9876543210",
                    RoleCode = "VIEWER"
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

                    Email = email,
                    Password = password
                });

        loginResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        loginResult.Should()
            .NotBeNull();

        // Authenticate
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult!.AccessToken);

        // Generate pairing code
        var pairingResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pairing",
                new GeneratePairingRequest
                {
                    LocationId =
                        TestDataSeeder.OrganizationLocationId
                });

        pairingResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var pairing =
            await pairingResponse.Content
                .ReadFromJsonAsync<PairingResponse>();

        pairing.Should()
            .NotBeNull();

        // Remove user JWT
        _client.DefaultRequestHeaders.Authorization = null;

        // Pair device
        var deviceIdentifier =
            $"GET-BY-ID-{Guid.NewGuid():N}";

        var pairResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        pairing!.PairingCode,

                    DeviceIdentifier =
                        deviceIdentifier,

                    Name =
                        "Get By ID Device",

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
                .ReadFromJsonAsync<DeviceAuthenticationResponse>();

        pairResult.Should()
            .NotBeNull();

        // Authenticate again
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult.AccessToken);

        // Act
        var response =
            await _client.GetAsync(
                $"/api/devices/{pairResult!.Device.Id}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<DeviceResponse>();

        result.Should()
            .NotBeNull();

        result!.Id
            .Should()
            .Be(pairResult.Device.Id);

        result.DeviceIdentifier
            .Should()
            .Be(deviceIdentifier);

        result.OrganizationId
            .Should()
            .Be(TestDataSeeder.OrganizationId);
    }

    [Fact]
    public async Task GetDeviceById_FromDifferentOrganization_ShouldReturnNotFound()
    {
        // Arrange
        var email =
            $"cross-org-device-{Guid.NewGuid()}@example.com";

        var password = "Test@12345";

        // Register Organization A user
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    OrganizationId =
                        TestDataSeeder.OrganizationId,

                    FirstName = "Cross",
                    LastName = "Organization",

                    Email = email,
                    Password = password,

                    Phone = "9876543210",
                    RoleCode = "VIEWER"
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

                    Email = email,
                    Password = password
                });

        loginResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        loginResult.Should()
            .NotBeNull();

        // Authenticate as Organization A
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult!.AccessToken);

        // Act
        // Organization B's device
        var response =
            await _client.GetAsync(
                $"/api/devices/{TestDataSeeder.SecondOrganizationDeviceId}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PairDevice_WithEmptyPairingCode_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new PairDeviceRequest
        {
            PairingCode = string.Empty,

            DeviceIdentifier =
                $"EMPTY-CODE-{Guid.NewGuid():N}",

            Name = "Empty Code Device",

            DeviceType = "Billboard",

            Platform = "Windows",

            AppVersion = "1.0.0"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PairDevice_ShouldReturnValidDeviceJwt()
    {
        // Arrange
        var email =
            $"jwt-device-{Guid.NewGuid()}@example.com";

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
                        "JWT",

                    LastName =
                        "Device",

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

        // Authenticate user
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult!.AccessToken);

        // Generate pairing code
        var pairingResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pairing",
                new GeneratePairingRequest
                {
                    LocationId =
                        TestDataSeeder.OrganizationLocationId
                });

        pairingResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var pairing =
            await pairingResponse.Content
                .ReadFromJsonAsync<PairingResponse>();

        pairing.Should()
            .NotBeNull();

        // Remove user token
        _client.DefaultRequestHeaders.Authorization =
            null;

        // Pair device
        var deviceIdentifier =
            $"JWT-DEVICE-{Guid.NewGuid():N}";

        var pairResponse =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        pairing!.PairingCode,

                    DeviceIdentifier =
                        deviceIdentifier,

                    Name =
                        "JWT Test Device",

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

        var result =
            await pairResponse.Content
                .ReadFromJsonAsync<
                    DeviceAuthenticationResponse>();

        result.Should()
            .NotBeNull();

        result!.DeviceToken
            .Should()
            .NotBeNullOrWhiteSpace();

        // =========================================================
        // Validate JWT
        // =========================================================

        var handler =
            new JwtSecurityTokenHandler();

        handler.CanReadToken(
                result.DeviceToken)
            .Should()
            .BeTrue();

        var token =
            handler.ReadJwtToken(
                result.DeviceToken);

        token.Should()
            .NotBeNull();

        // =========================================================
        // Validate required claims
        // =========================================================

        token.Claims
            .Should()
            .Contain(x =>
                x.Type ==
                "device" &&
                x.Value ==
                "true");

        token.Claims
            .Should()
            .Contain(x =>
                x.Type ==
                "deviceId" &&
                x.Value ==
                result.Device.Id.ToString());

        token.Claims
            .Should()
            .Contain(x =>
                x.Type ==
                "organizationId" &&
                x.Value ==
                TestDataSeeder
                    .OrganizationId
                    .ToString());

        token.Claims
            .Should()
            .Contain(x =>
                x.Type ==
                "locationId" &&
                x.Value ==
                TestDataSeeder
                    .OrganizationLocationId
                    .ToString());

        token.Claims
            .Should()
            .Contain(x =>
                x.Type ==
                "deviceCode" &&
                x.Value ==
                result.Device.DeviceCode);

        token.Claims
            .Should()
            .Contain(x =>
                x.Type ==
                "deviceIdentifier" &&
                x.Value ==
                deviceIdentifier);

        // =========================================================
        // Validate token metadata
        // =========================================================

        token.Issuer
            .Should()
            .NotBeNullOrWhiteSpace();

        token.Audiences
            .Should()
            .NotBeEmpty();

        token.ValidTo
            .Should()
            .BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task PairDevice_ReturnedToken_ShouldAuthenticateHeartbeat()
    {
        // Arrange
        var pairing =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization =
            null;

        var deviceIdentifier =
            $"HEARTBEAT-{Guid.NewGuid():N}";

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
                        "Heartbeat Authentication Device",

                    DeviceType =
                        "Billboard",

                    Platform =
                        "Windows",

                    AppVersion =
                        "1.0.0"
                });

        // Pairing should succeed
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
        // Use the JWT returned by pairing
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                pairResult.DeviceToken);

        var heartbeatResponse =
            await _client.PostAsync(
                "/api/devices/heartbeat",
                content: null);

        // Assert
        heartbeatResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var heartbeatResult =
            await heartbeatResponse.Content
                .ReadFromJsonAsync<
                    DeviceHeartbeatResponse>();

        heartbeatResult.Should()
            .NotBeNull();

        heartbeatResult!.DeviceId
            .Should()
            .Be(pairResult.Device.Id);

        heartbeatResult.IsOnline
            .Should()
            .BeTrue();

        heartbeatResult.ServerTime
            .Should()
            .BeCloseTo(
                DateTime.UtcNow,
                TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task PairDevice_WithWhitespaceDeviceIdentifier_ShouldReturnBadRequest()
    {
        // Arrange
        var pairing =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization =
            null;

        var request =
            new PairDeviceRequest
            {
                PairingCode =
                    pairing.PairingCode,

                DeviceIdentifier =
                    "   ",

                Name =
                    "Whitespace Identifier Device",

                DeviceType =
                    "Billboard",

                Platform =
                    "Windows",

                AppVersion =
                    "1.0.0"
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    private async Task<PairingResponse> CreatePairingAsync()
    {
        var email =
            $"pairing-{Guid.NewGuid():N}@example.com";

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

                    FirstName = "Pairing",
                    LastName = "Test",

                    Email = email,
                    Password = password,

                    Phone = "9876543210",
                    RoleCode = "VIEWER"
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

                    Email = email,
                    Password = password
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
                        TestDataSeeder.OrganizationLocationId
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

    [Fact]
    public async Task PairDevice_WithEmptyName_ShouldGenerateDefaultDeviceName()
    {
        // Arrange
        var pairing =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization =
            null;

        var request =
            new PairDeviceRequest
            {
                PairingCode =
                    pairing.PairingCode,

                DeviceIdentifier =
                    $"DEFAULT-NAME-{Guid.NewGuid():N}",

                Name =
                    string.Empty,

                DeviceType =
                    "Billboard",

                Platform =
                    "Windows",

                AppVersion =
                    "1.0.0"
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    DeviceAuthenticationResponse>();

        result.Should()
            .NotBeNull();

        result!.Device.Should()
            .NotBeNull();

        result.Device.DeviceCode
            .Should()
            .NotBeNullOrWhiteSpace();

        result.Device.Name
            .Should()
            .Be(
                $"Device {result.Device.DeviceCode}");
    }

    [Fact]
    public async Task PairDevice_WithoutOptionalFields_ShouldReturnOk()
    {
        // Arrange
        var pairing =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization =
            null;

        var deviceIdentifier =
            $"OPTIONAL-{Guid.NewGuid():N}";

        var request =
            new PairDeviceRequest
            {
                PairingCode =
                    pairing.PairingCode,

                DeviceIdentifier =
                    deviceIdentifier,

                Name =
                    "Minimal Device"

                // Optional fields intentionally omitted
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    DeviceAuthenticationResponse>();

        result.Should()
            .NotBeNull();

        result!.Device.Should()
            .NotBeNull();

        result.Device.DeviceIdentifier
            .Should()
            .Be(deviceIdentifier);

        result.Device.Name
            .Should()
            .Be("Minimal Device");

        result.Device.DeviceType
            .Should()
            .BeNull();

        result.Device.Platform
            .Should()
            .BeNull();

        result.Device.AppVersion
            .Should()
            .BeNull();

        result.Device.Model
            .Should()
            .BeNull();

        result.Device.Manufacturer
            .Should()
            .BeNull();

        result.Device.SerialNumber
            .Should()
            .BeNull();

        result.Device.MacAddress
            .Should()
            .BeNull();

        result.Device.IpAddress
            .Should()
            .BeNull();
    }

    [Fact]
    public async Task PairDevice_TwoDevices_ShouldGenerateUniqueDeviceCodes()
    {
        // Arrange
        var pairing1 =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization =
            null;

        var response1 =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        pairing1.PairingCode,

                    DeviceIdentifier =
                        $"UNIQUE-1-{Guid.NewGuid():N}",

                    Name =
                        "Unique Device 1"
                });

        response1.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result1 =
            await response1.Content
                .ReadFromJsonAsync<
                    DeviceAuthenticationResponse>();

        result1.Should()
            .NotBeNull();

        result1!.Device.DeviceCode
            .Should()
            .NotBeNullOrWhiteSpace();


        // Generate second pairing code
        var pairing2 =
            await CreatePairingAsync();

        _client.DefaultRequestHeaders.Authorization =
            null;

        var response2 =
            await _client.PostAsJsonAsync(
                "/api/devices/pair",
                new PairDeviceRequest
                {
                    PairingCode =
                        pairing2.PairingCode,

                    DeviceIdentifier =
                        $"UNIQUE-2-{Guid.NewGuid():N}",

                    Name =
                        "Unique Device 2"
                });

        response2.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result2 =
            await response2.Content
                .ReadFromJsonAsync<
                    DeviceAuthenticationResponse>();

        result2.Should()
            .NotBeNull();

        result2!.Device.DeviceCode
            .Should()
            .NotBeNullOrWhiteSpace();


        // Assert
        result1.Device.DeviceCode
            .Should()
            .NotBe(
                result2.Device.DeviceCode);
    }

}