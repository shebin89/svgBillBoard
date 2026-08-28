using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Authentication;
using SvgBillBoard.Application.DTOs.Locations;
using SvgBillBoard.Application.DTOs.PlaylistAssignments;
using SvgBillBoard.Application.DTOs.Playlists;

namespace SvgBillBoard.API.Tests.Tests.PlaylistAssignments;

public class PlaylistAssignmentTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PlaylistAssignmentTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }


    // =========================================================
    // CREATE
    // =========================================================

    [Fact]
    public async Task CreateAssignment_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var location =
            await CreateLocationAsync();

        var request =
            new CreatePlaylistAssignmentRequest
            {
                PlaylistId =
                    playlist.Id,

                LocationId =
                    location.Id,

                StartDate =
                    DateTime.UtcNow.Date,

                EndDate =
                    DateTime.UtcNow.Date.AddDays(7)
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-assignments",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    PlaylistAssignmentResponse>();

        result.Should()
            .NotBeNull();

        result!.OrganizationId
            .Should()
            .Be(TestDataSeeder.OrganizationId);

        result.PlaylistId
            .Should()
            .Be(playlist.Id);

        result.LocationId
            .Should()
            .Be(location.Id);

        result.PlaylistName
            .Should()
            .Be(playlist.Name);

        result.LocationName
            .Should()
            .Be(location.Name);

        result.Status
            .Should()
            .Be(1);
    }


    // =========================================================
    // CREATE - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task CreateAssignment_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        var request =
            new CreatePlaylistAssignmentRequest
            {
                PlaylistId =
                    Guid.NewGuid(),

                LocationId =
                    TestDataSeeder.OrganizationLocationId
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-assignments",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // GET ALL
    // =========================================================

    [Fact]
    public async Task GetAssignments_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var location =
            await CreateLocationAsync();

        await CreateAssignmentAsync(
            playlist.Id,
            location.Id);

        // Act
        var response =
            await _client.GetAsync(
                "/api/playlist-assignments");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    List<PlaylistAssignmentResponse>>();

        result.Should()
            .NotBeNull();

        result!
            .Should()
            .Contain(x =>
                x.PlaylistId == playlist.Id &&
                x.LocationId == location.Id);
    }


    // =========================================================
    // GET BY ID
    // =========================================================

    [Fact]
    public async Task GetAssignmentById_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var location =
            await CreateLocationAsync();

        var assignment =
            await CreateAssignmentAsync(
                playlist.Id,
                location.Id);

        // Act
        var response =
            await _client.GetAsync(
                $"/api/playlist-assignments/{assignment.Id}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    PlaylistAssignmentResponse>();

        result.Should()
            .NotBeNull();

        result!.Id
            .Should()
            .Be(assignment.Id);

        result.PlaylistId
            .Should()
            .Be(playlist.Id);

        result.LocationId
            .Should()
            .Be(location.Id);
    }


    // =========================================================
    // GET NONEXISTENT
    // =========================================================

    [Fact]
    public async Task GetAssignmentById_WithNonexistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();

        // Act
        var response =
            await _client.GetAsync(
                $"/api/playlist-assignments/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // DELETE
    // =========================================================

    [Fact]
    public async Task DeleteAssignment_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var location =
            await CreateLocationAsync();

        var assignment =
            await CreateAssignmentAsync(
                playlist.Id,
                location.Id);

        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/playlist-assignments/{assignment.Id}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse =
            await _client.GetAsync(
                $"/api/playlist-assignments/{assignment.Id}");

        getResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // DELETE NONEXISTENT
    // =========================================================

    [Fact]
    public async Task DeleteAssignment_WithNonexistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();

        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/playlist-assignments/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // DUPLICATE LOCATION
    // =========================================================

    [Fact]
    public async Task CreateAssignment_WithAlreadyAssignedLocation_ShouldReturnConflict()
    {
        // Arrange
        await AuthenticateUserAsync();

        var location =
            await CreateLocationAsync();

        var firstPlaylist =
            await CreatePlaylistAsync();

        await CreateAssignmentAsync(
            firstPlaylist.Id,
            location.Id);


        var secondPlaylist =
            await CreatePlaylistAsync();

        var request =
            new CreatePlaylistAssignmentRequest
            {
                PlaylistId =
                    secondPlaylist.Id,

                LocationId =
                    location.Id
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-assignments",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }


    // =========================================================
    // INVALID PLAYLIST ID
    // =========================================================

    [Fact]
    public async Task CreateAssignment_WithEmptyPlaylistId_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var location =
            await CreateLocationAsync();

        var request =
            new CreatePlaylistAssignmentRequest
            {
                PlaylistId =
                    Guid.Empty,

                LocationId =
                    location.Id
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-assignments",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    // =========================================================
    // INVALID LOCATION ID
    // =========================================================

    [Fact]
    public async Task CreateAssignment_WithEmptyLocationId_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var request =
            new CreatePlaylistAssignmentRequest
            {
                PlaylistId =
                    playlist.Id,

                LocationId =
                    Guid.Empty
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-assignments",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    // =========================================================
    // INVALID DATE RANGE
    // =========================================================

    [Fact]
    public async Task CreateAssignment_WithEndDateBeforeStartDate_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var location =
            await CreateLocationAsync();

        var request =
            new CreatePlaylistAssignmentRequest
            {
                PlaylistId =
                    playlist.Id,

                LocationId =
                    location.Id,

                StartDate =
                    DateTime.UtcNow.Date.AddDays(7),

                EndDate =
                    DateTime.UtcNow.Date
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-assignments",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    // =========================================================
    // OTHER ORGANIZATION PLAYLIST
    // =========================================================

    [Fact]
    public async Task CreateAssignment_WithPlaylistFromAnotherOrganization_ShouldReturnConflict()
    {
        // Arrange
        await AuthenticateUserAsync(
            _client,
            TestDataSeeder.OrganizationId);

        using var secondClient =
            new CustomWebApplicationFactory()
                .CreateClient();

        await AuthenticateUserAsync(
            secondClient,
            TestDataSeeder.SecondOrganizationId);

        var secondPlaylist =
            await CreatePlaylistAsync(
                secondClient);

        var request =
            new CreatePlaylistAssignmentRequest
            {
                PlaylistId =
                    secondPlaylist.Id,

                LocationId =
                    TestDataSeeder.OrganizationLocationId
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-assignments",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }


    // =========================================================
    // OTHER ORGANIZATION LOCATION
    // =========================================================

    [Fact]
    public async Task CreateAssignment_WithLocationFromAnotherOrganization_ShouldReturnConflict()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var request =
            new CreatePlaylistAssignmentRequest
            {
                PlaylistId =
                    playlist.Id,

                LocationId =
                    TestDataSeeder
                        .SecondOrganizationLocationId
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-assignments",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }


    // =========================================================
    // GET ALL - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task GetAssignments_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                "/api/playlist-assignments");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // GET BY ID - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task GetAssignmentById_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                $"/api/playlist-assignments/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // DELETE - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task DeleteAssignment_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/playlist-assignments/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // HELPERS
    // =========================================================

    private async Task<PlaylistResponse>
        CreatePlaylistAsync()
    {
        return await CreatePlaylistAsync(
            _client);
    }


    private static async Task<PlaylistResponse>
        CreatePlaylistAsync(
            HttpClient client)
    {
        var response =
            await client.PostAsJsonAsync(
                "/api/playlists",
                new CreatePlaylistRequest
                {
                    Name =
                        $"Assignment Playlist {Guid.NewGuid():N}",

                    Description =
                        "Playlist assignment test"
                });

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var playlist =
            await response.Content
                .ReadFromJsonAsync<
                    PlaylistResponse>();

        playlist.Should()
            .NotBeNull();

        return playlist!;
    }


    private async Task<LocationResponse>
        CreateLocationAsync()
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/locations",
                new CreateLocationRequest
                {
                    Name =
                        $"Assignment Location {Guid.NewGuid():N}",

                    Code =
                        $"ASSIGN-{Guid.NewGuid():N}"
                });

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var location =
            await response.Content
                .ReadFromJsonAsync<
                    LocationResponse>();

        location.Should()
            .NotBeNull();

        return location!;
    }


    private async Task<
        PlaylistAssignmentResponse>
        CreateAssignmentAsync(
            Guid playlistId,
            Guid locationId)
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-assignments",
                new CreatePlaylistAssignmentRequest
                {
                    PlaylistId =
                        playlistId,

                    LocationId =
                        locationId,

                    StartDate =
                        DateTime.UtcNow.Date,

                    EndDate =
                        DateTime.UtcNow.Date.AddDays(7)
                });

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    PlaylistAssignmentResponse>();

        result.Should()
            .NotBeNull();

        return result!;
    }


    private async Task
        AuthenticateUserAsync()
    {
        await AuthenticateUserAsync(
            _client,
            TestDataSeeder.OrganizationId);
    }


    private static async Task
        AuthenticateUserAsync(
            HttpClient client,
            Guid organizationId)
    {
        var email =
            $"assignment-{Guid.NewGuid():N}@example.com";

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
                        "Assignment",

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
                .ReadFromJsonAsync<
                    LoginResponse>();

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