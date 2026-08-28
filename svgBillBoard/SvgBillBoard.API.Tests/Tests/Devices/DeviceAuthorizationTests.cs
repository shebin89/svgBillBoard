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


namespace SvgBillBoard.API.Tests.Tests.Devices;

public class DeviceAuthorizationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    private readonly CustomWebApplicationFactory _factory;

    public DeviceAuthorizationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDevices_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                "/api/devices");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task GetDevices_WithValidUserToken_ShouldReturnOk()
    {
        // Arrange
        var email =
            $"devices-{Guid.NewGuid()}@example.com";

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
                        "Device",

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


        // Authenticate
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
                .ReadFromJsonAsync<
                    List<DeviceResponse>>();

        devices.Should()
            .NotBeNull();
    }

    [Fact]
    public async Task GetDevice_FromAnotherOrganization_ShouldReturnNotFound()
    {
        // Arrange
        var email =
            $"cross-org-{Guid.NewGuid()}@example.com";

        var password =
            "Test@12345";

        // Register Organization A user
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    OrganizationId =
                        TestDataSeeder.OrganizationId,

                    FirstName =
                        "Cross",

                    LastName =
                        "Organization",

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

        // Login as Organization A user
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

        // Authenticate as Organization A
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult.AccessToken);

        // Act
        // Request Organization B's device
        var response =
            await _client.GetAsync(
                $"/api/devices/{TestDataSeeder.SecondOrganizationDeviceId}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task GetDevices_ShouldReturnOnlyCurrentOrganizationDevices()
    {
        // Arrange
        var email =
            $"org-a-devices-{Guid.NewGuid()}@example.com";

        var password = "Test@12345";

        // Register Organization A user
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    OrganizationId =
                        TestDataSeeder.OrganizationId,

                    FirstName = "Organization",
                    LastName = "A",

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

        // Authenticate as Organization A
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
                .ReadFromJsonAsync<
                    List<DeviceResponse>>();

        devices.Should()
            .NotBeNull();

        // Organization B device must not be returned
        devices!
            .Should()
            .NotContain(x =>
                x.Id ==
                TestDataSeeder.SecondOrganizationDeviceId);

        // Every returned device must belong to Organization A
        devices
            .Should()
            .OnlyContain(x =>
                x.OrganizationId ==
                TestDataSeeder.OrganizationId);
    }

    [Fact]
    public async Task GetDevice_WithOwnOrganizationDevice_ShouldReturnOk()
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
                        $"GET-BY-ID-{Guid.NewGuid():N}",

                    Name =
                        "Get By Id Test Device"
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

        // Authenticate as the organization user again
        await AuthenticateUserAsync();

        // Act
        var response =
            await _client.GetAsync(
                $"/api/devices/{pairResult!.Device.Id}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var device =
            await response.Content
                .ReadFromJsonAsync<DeviceResponse>();

        device.Should()
            .NotBeNull();

        device!.Id
            .Should()
            .Be(pairResult.Device.Id);

        device.OrganizationId
            .Should()
            .Be(TestDataSeeder.OrganizationId);

        device.LocationId
            .Should()
            .Be(TestDataSeeder.OrganizationLocationId);

        device.Name
            .Should()
            .Be("Get By Id Test Device");
    }

    [Fact]
    public async Task GetDevice_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        var deviceId = Guid.NewGuid();

        // Act
        var response =
            await _client.GetAsync(
                $"/api/devices/{deviceId}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    private async Task<PairingResponse> CreatePairingAsync()
    {
        await AuthenticateUserAsync();

        var response =
            await _client.PostAsJsonAsync(
                "/api/devices/pairing",
                new GeneratePairingRequest
                {
                    LocationId =
                        TestDataSeeder.OrganizationLocationId
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
                        "Authorization",

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

        // Set user token
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);
    }
}