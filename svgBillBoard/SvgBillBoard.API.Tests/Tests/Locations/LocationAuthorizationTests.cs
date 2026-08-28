using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Authentication;
using SvgBillBoard.Application.DTOs.Locations;

namespace SvgBillBoard.API.Tests.Tests.Locations;

public class LocationAuthorizationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LocationAuthorizationTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }


    // =========================================================
    // GET ALL - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task GetLocations_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                "/api/locations");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // GET ALL - VALID USER
    // =========================================================

    [Fact]
    public async Task GetLocations_WithValidUserToken_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        // Act
        var response =
            await _client.GetAsync(
                "/api/locations");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var locations =
            await response.Content
                .ReadFromJsonAsync<
                    List<LocationResponse>>();

        locations.Should()
            .NotBeNull();

        locations!
            .Should()
            .OnlyContain(x =>
                x.OrganizationId ==
                TestDataSeeder.OrganizationId);
    }


    // =========================================================
    // GET BY ID - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task GetLocation_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                $"/api/locations/{TestDataSeeder.OrganizationLocationId}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // GET BY ID - OWN ORGANIZATION
    // =========================================================

    [Fact]
    public async Task GetLocation_FromOwnOrganization_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        // Act
        var response =
            await _client.GetAsync(
                $"/api/locations/{TestDataSeeder.OrganizationLocationId}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var location =
            await response.Content
                .ReadFromJsonAsync<LocationResponse>();

        location.Should()
            .NotBeNull();

        location!.Id
            .Should()
            .Be(TestDataSeeder.OrganizationLocationId);

        location.OrganizationId
            .Should()
            .Be(TestDataSeeder.OrganizationId);
    }


    // =========================================================
    // GET BY ID - OTHER ORGANIZATION
    // =========================================================

    [Fact]
    public async Task GetLocation_FromAnotherOrganization_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();

        // Act
        var response =
            await _client.GetAsync(
                $"/api/locations/{TestDataSeeder.SecondOrganizationLocationId}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // GET ALL - ORGANIZATION ISOLATION
    // =========================================================

    [Fact]
    public async Task GetLocations_ShouldReturnOnlyCurrentOrganizationLocations()
    {
        // Arrange
        await AuthenticateUserAsync();

        // Act
        var response =
            await _client.GetAsync(
                "/api/locations");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var locations =
            await response.Content
                .ReadFromJsonAsync<
                    List<LocationResponse>>();

        locations.Should()
            .NotBeNull();

        locations!
            .Should()
            .OnlyContain(x =>
                x.OrganizationId ==
                TestDataSeeder.OrganizationId);

        locations!
            .Should()
            .NotContain(x =>
                x.Id ==
                TestDataSeeder.SecondOrganizationLocationId);
    }


    // =========================================================
    // HELPER
    // =========================================================

    private async Task AuthenticateUserAsync()
    {
        var email =
            $"location-auth-{Guid.NewGuid():N}@example.com";

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
                        "Location",

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


        // Authenticate
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);
    }
}