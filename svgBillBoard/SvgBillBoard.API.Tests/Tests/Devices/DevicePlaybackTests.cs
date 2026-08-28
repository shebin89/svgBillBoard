using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Authentication;
using SvgBillBoard.Application.DTOs.Devices;

namespace SvgBillBoard.API.Tests.Tests.Devices;

public class DevicePlaybackTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DevicePlaybackTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }


    // =========================================================
    // WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task GetPlayback_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                "/api/device/playback");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task SyncPlayback_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                "/api/device/playback/sync");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // USER TOKEN
    // =========================================================

    [Fact]
    public async Task GetPlayback_WithUserToken_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateUserAsync();

        // Act
        var response =
            await _client.GetAsync(
                "/api/device/playback");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }


    [Fact]
    public async Task SyncPlayback_WithUserToken_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateUserAsync();

        // Act
        var response =
            await _client.GetAsync(
                "/api/device/playback/sync");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }


    // =========================================================
    // VALID DEVICE TOKEN - EMPTY PLAYBACK
    // =========================================================

    [Fact]
    public async Task GetPlayback_WithValidDeviceTokenAndNoAssignment_ShouldReturnEmptyPlayback()
    {
        // Arrange
        var device =
            await CreateAndAuthenticateDeviceAsync();

        // Act
        var response =
            await _client.GetAsync(
                "/api/device/playback");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    DevicePlaybackResponse>();

        result.Should()
            .NotBeNull();

        result!.DeviceId
            .Should()
            .Be(device.Id);

        result.LocationId
            .Should()
            .Be(device.LocationId);

        result.DeviceName
            .Should()
            .Be(device.Name);

        result.PlaylistId
            .Should()
            .BeNull();

        result.PlaylistName
            .Should()
            .BeNull();

        result.Items
            .Should()
            .BeEmpty();

        result.PlaybackVersion
            .Should()
            .BeGreaterThanOrEqualTo(0);
    }


    // =========================================================
    // VALID DEVICE TOKEN - SYNC
    // =========================================================

    [Fact]
    public async Task SyncPlayback_WithValidDeviceTokenAndNoAssignment_ShouldReturnEmptyPlayback()
    {
        // Arrange
        var device =
            await CreateAndAuthenticateDeviceAsync();

        // Act
        var response =
            await _client.GetAsync(
                "/api/device/playback/sync");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    DevicePlaybackResponse>();

        result.Should()
            .NotBeNull();

        result!.DeviceId
            .Should()
            .Be(device.Id);

        result.LocationId
            .Should()
            .Be(device.LocationId);

        result.Items
            .Should()
            .BeEmpty();
    }


    // =========================================================
    // SAME VERSION - 304
    // =========================================================

    [Fact]
    public async Task GetPlayback_WithSameVersion_ShouldReturnNotModified()
    {
        // Arrange
        await CreateAndAuthenticateDeviceAsync();

        var currentResponse =
            await _client.GetAsync(
                "/api/device/playback");

        currentResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var currentPlayback =
            await currentResponse.Content
                .ReadFromJsonAsync<
                    DevicePlaybackResponse>();

        currentPlayback.Should()
            .NotBeNull();

        // Act
        var response =
            await _client.GetAsync(
                $"/api/device/playback?version={currentPlayback!.PlaybackVersion}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotModified);
    }


    [Fact]
    public async Task SyncPlayback_WithSameVersion_ShouldReturnNotModified()
    {
        // Arrange
        await CreateAndAuthenticateDeviceAsync();

        var currentResponse =
            await _client.GetAsync(
                "/api/device/playback");

        currentResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var currentPlayback =
            await currentResponse.Content
                .ReadFromJsonAsync<
                    DevicePlaybackResponse>();

        currentPlayback.Should()
            .NotBeNull();

        // Act
        var response =
            await _client.GetAsync(
                $"/api/device/playback/sync?version={currentPlayback!.PlaybackVersion}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotModified);
    }


    // =========================================================
    // DIFFERENT VERSION - 200
    // =========================================================

    [Fact]
    public async Task GetPlayback_WithDifferentVersion_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateDeviceAsync();

        var currentResponse =
            await _client.GetAsync(
                "/api/device/playback");

        currentResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var currentPlayback =
            await currentResponse.Content
                .ReadFromJsonAsync<
                    DevicePlaybackResponse>();

        currentPlayback.Should()
            .NotBeNull();

        var differentVersion =
            currentPlayback!.PlaybackVersion + 1;

        // Act
        var response =
            await _client.GetAsync(
                $"/api/device/playback?version={differentVersion}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    DevicePlaybackResponse>();

        result.Should()
            .NotBeNull();
    }


    [Fact]
    public async Task SyncPlayback_WithDifferentVersion_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateDeviceAsync();

        var currentResponse =
            await _client.GetAsync(
                "/api/device/playback");

        currentResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var currentPlayback =
            await currentResponse.Content
                .ReadFromJsonAsync<
                    DevicePlaybackResponse>();

        currentPlayback.Should()
            .NotBeNull();

        var differentVersion =
            currentPlayback!.PlaybackVersion + 1;

        // Act
        var response =
            await _client.GetAsync(
                $"/api/device/playback/sync?version={differentVersion}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    DevicePlaybackResponse>();

        result.Should()
            .NotBeNull();
    }


    // =========================================================
    // INVALID DEVICE TOKEN
    // =========================================================

    [Fact]
    public async Task GetPlayback_WithInvalidDeviceToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                "invalid-device-token");

        // Act
        var response =
            await _client.GetAsync(
                "/api/device/playback");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task SyncPlayback_WithInvalidDeviceToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                "invalid-device-token");

        // Act
        var response =
            await _client.GetAsync(
                "/api/device/playback/sync");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // HELPERS
    // =========================================================

    private async Task<TestDevice>
        CreateAndAuthenticateDeviceAsync()
    {
        await AuthenticateUserAsync();

        var pairing =
            await CreatePairingAsync();

        var deviceIdentifier =
            $"PLAYBACK-{Guid.NewGuid():N}";

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
                        "Playback Test Device",

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


        // Device login
        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/device-auth/login",
                new DeviceLoginRequest
                {
                    DeviceToken =
                        pairResult.DeviceToken
                });

        loginResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<
                    DeviceLoginResponse>();

        loginResult.Should()
            .NotBeNull();

        loginResult!.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();


        // Authenticate using device JWT
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult.AccessToken);


        return new TestDevice
        {
            Id =
                pairResult.Device.Id,

            LocationId =
                pairResult.Device.LocationId,

            Name =
                pairResult.Device.Name
        };
    }


    private async Task<PairingResponse>
        CreatePairingAsync()
    {
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
                .ReadFromJsonAsync<
                    PairingResponse>();

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
            $"playback-{Guid.NewGuid():N}@example.com";

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
                        "Playback",

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


    // =========================================================
    // TEST DEVICE
    // =========================================================

    private class TestDevice
    {
        public Guid Id { get; set; }

        public Guid LocationId { get; set; }

        public string Name { get; set; }
            = string.Empty;
    }
}