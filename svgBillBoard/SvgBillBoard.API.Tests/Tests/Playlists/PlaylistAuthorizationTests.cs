using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Authentication;
using SvgBillBoard.Application.DTOs.Playlists;

namespace SvgBillBoard.API.Tests.Tests.Playlists;

public class PlaylistAuthorizationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public PlaylistAuthorizationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }


    // =========================================================
    // GET ALL - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task GetPlaylists_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                "/api/playlists");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // GET ALL - VALID TOKEN
    // =========================================================

    [Fact]
    public async Task GetPlaylists_WithValidUserToken_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        // Act
        var response =
            await _client.GetAsync(
                "/api/playlists");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var playlists =
            await response.Content
                .ReadFromJsonAsync<
                    List<PlaylistResponse>>();

        playlists.Should()
            .NotBeNull();

        playlists!
            .Should()
            .OnlyContain(x =>
                x.OrganizationId ==
                TestDataSeeder.OrganizationId);
    }


    // =========================================================
    // GET BY ID - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task GetPlaylist_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                $"/api/playlists/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // GET BY ID - NONEXISTENT
    // =========================================================

    [Fact]
    public async Task GetPlaylist_WithNonexistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();

        // Act
        var response =
            await _client.GetAsync(
                $"/api/playlists/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // GET BY ID - OTHER ORGANIZATION
    // =========================================================

    [Fact]
    public async Task GetPlaylist_FromAnotherOrganization_ShouldReturnNotFound()
    {
        // Arrange
        // Authenticate current client as Organization A.
        await AuthenticateUserAsync(
            _client,
            TestDataSeeder.OrganizationId);


        // Create a separate client from the SAME factory.
        using var secondClient =
            _factory.CreateClient();


        // Authenticate second client as Organization B.
        await AuthenticateUserAsync(
            secondClient,
            TestDataSeeder.SecondOrganizationId);


        // Create Organization B playlist.
        var createResponse =
            await secondClient.PostAsJsonAsync(
                "/api/playlists",
                new CreatePlaylistRequest
                {
                    Name =
                        "Organization B Playlist",

                    Description =
                        "Cross organization test"
                });

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var playlist =
            await createResponse.Content
                .ReadFromJsonAsync<PlaylistResponse>();

        playlist.Should()
            .NotBeNull();


        // Act
        // Organization A tries to access Organization B playlist.
        var response =
            await _client.GetAsync(
                $"/api/playlists/{playlist!.Id}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // GET ALL - ORGANIZATION ISOLATION
    // =========================================================

    [Fact]
    public async Task GetPlaylists_ShouldReturnOnlyCurrentOrganizationPlaylists()
    {
        // Arrange
        await AuthenticateUserAsync(
            _client,
            TestDataSeeder.OrganizationId);


        using var secondClient =
            _factory.CreateClient();


        await AuthenticateUserAsync(
            secondClient,
            TestDataSeeder.SecondOrganizationId);


        // Create Organization B playlist.
        var createResponse =
            await secondClient.PostAsJsonAsync(
                "/api/playlists",
                new CreatePlaylistRequest
                {
                    Name =
                        "Organization B Playlist"
                });

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);


        // Act
        var response =
            await _client.GetAsync(
                "/api/playlists");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var playlists =
            await response.Content
                .ReadFromJsonAsync<
                    List<PlaylistResponse>>();

        playlists.Should()
            .NotBeNull();

        playlists!
            .Should()
            .OnlyContain(x =>
                x.OrganizationId ==
                TestDataSeeder.OrganizationId);

        playlists!
            .Should()
            .NotContain(x =>
                x.OrganizationId ==
                TestDataSeeder.SecondOrganizationId);
    }


    // =========================================================
    // HELPER
    // =========================================================

    private async Task AuthenticateUserAsync()
    {
        await AuthenticateUserAsync(
            _client,
            TestDataSeeder.OrganizationId);
    }


    private static async Task AuthenticateUserAsync(
        HttpClient client,
        Guid organizationId)
    {
        var email =
            $"playlist-auth-{Guid.NewGuid():N}@example.com";

        var password =
            "Test@12345";


        // Register
        var registerResponse =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    OrganizationId =
                        organizationId,

                    FirstName =
                        "Playlist",

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
            await client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    OrganizationId =
                        organizationId,

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


        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);
    }
}