using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Authentication;
using SvgBillBoard.Application.DTOs.PlaylistSchedules;
using SvgBillBoard.Application.DTOs.Playlists;

namespace SvgBillBoard.API.Tests.Tests.PlaylistSchedules;

public class PlaylistScheduleAuthorizationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public PlaylistScheduleAuthorizationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }


    // =========================================================
    // GET ALL - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task GetSchedules_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                "/api/playlist-schedules");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // GET BY ID - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task GetScheduleById_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                $"/api/playlist-schedules/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // CREATE - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task CreateSchedule_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        var request =
            new CreatePlaylistScheduleRequest
            {
                PlaylistId =
                    Guid.NewGuid(),

                LocationId =
                    TestDataSeeder.OrganizationLocationId,

                DaysOfWeek =
                    1,

                Priority =
                    1
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-schedules",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // DELETE - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task DeleteSchedule_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/playlist-schedules/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // GET OTHER ORGANIZATION SCHEDULE
    // =========================================================

    [Fact]
    public async Task GetSchedule_FromAnotherOrganization_ShouldReturnNotFound()
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

        var playlist =
            await CreatePlaylistAsync(
                secondClient);

        var schedule =
            await CreateScheduleAsync(
                secondClient,
                playlist.Id,
                TestDataSeeder.SecondOrganizationLocationId);

        // Act
        var response =
            await _client.GetAsync(
                $"/api/playlist-schedules/{schedule.Id}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // GET ALL - ORGANIZATION ISOLATION
    // =========================================================

    [Fact]
    public async Task GetSchedules_ShouldReturnOnlyCurrentOrganizationSchedules()
    {
        // Arrange
        await AuthenticateUserAsync(
            _client,
            TestDataSeeder.OrganizationId);

        var organizationAPlaylist =
            await CreatePlaylistAsync(
                _client);

        await CreateScheduleAsync(
            _client,
            organizationAPlaylist.Id,
            TestDataSeeder.OrganizationLocationId);


        using var secondClient =
            _factory.CreateClient();

        await AuthenticateUserAsync(
            secondClient,
            TestDataSeeder.SecondOrganizationId);

        var organizationBPlaylist =
            await CreatePlaylistAsync(
                secondClient);

        await CreateScheduleAsync(
            secondClient,
            organizationBPlaylist.Id,
            TestDataSeeder.SecondOrganizationLocationId);


        // Act
        var response =
            await _client.GetAsync(
                "/api/playlist-schedules");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var schedules =
            await response.Content
                .ReadFromJsonAsync<
                    List<PlaylistScheduleResponse>>();

        schedules.Should()
            .NotBeNull();

        schedules!
            .Should()
            .OnlyContain(x =>
                x.OrganizationId ==
                TestDataSeeder.OrganizationId);

        schedules!
            .Should()
            .NotContain(x =>
                x.OrganizationId ==
                TestDataSeeder.SecondOrganizationId);
    }


    // =========================================================
    // DELETE OTHER ORGANIZATION SCHEDULE
    // =========================================================

    [Fact]
    public async Task DeleteSchedule_FromAnotherOrganization_ShouldReturnNotFound()
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

        var playlist =
            await CreatePlaylistAsync(
                secondClient);

        var schedule =
            await CreateScheduleAsync(
                secondClient,
                playlist.Id,
                TestDataSeeder.SecondOrganizationLocationId);


        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/playlist-schedules/{schedule.Id}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // CREATE WITH OTHER ORGANIZATION PLAYLIST
    // =========================================================

    [Fact]
    public async Task CreateSchedule_WithPlaylistFromAnotherOrganization_ShouldReturnConflict()
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

        var playlist =
            await CreatePlaylistAsync(
                secondClient);

        var request =
            new CreatePlaylistScheduleRequest
            {
                PlaylistId =
                    playlist.Id,

                LocationId =
                    TestDataSeeder.OrganizationLocationId,

                DaysOfWeek =
                    1,

                Priority =
                    1
            };


        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-schedules",
                request);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }


    // =========================================================
    // CREATE WITH OTHER ORGANIZATION LOCATION
    // =========================================================

    [Fact]
    public async Task CreateSchedule_WithLocationFromAnotherOrganization_ShouldReturnConflict()
    {
        // Arrange
        await AuthenticateUserAsync(
            _client,
            TestDataSeeder.OrganizationId);

        var playlist =
            await CreatePlaylistAsync(
                _client);

        var request =
            new CreatePlaylistScheduleRequest
            {
                PlaylistId =
                    playlist.Id,

                LocationId =
                    TestDataSeeder
                        .SecondOrganizationLocationId,

                DaysOfWeek =
                    1,

                Priority =
                    1
            };


        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-schedules",
                request);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }


    // =========================================================
    // HELPERS
    // =========================================================

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
                        $"Schedule Auth Playlist {Guid.NewGuid():N}",

                    Description =
                        "Authorization test playlist"
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


    private static async Task<
        PlaylistScheduleResponse>
        CreateScheduleAsync(
            HttpClient client,
            Guid playlistId,
            Guid locationId)
    {
        var response =
            await client.PostAsJsonAsync(
                "/api/playlist-schedules",
                new CreatePlaylistScheduleRequest
                {
                    PlaylistId =
                        playlistId,

                    LocationId =
                        locationId,

                    StartDate =
                        DateTime.UtcNow.Date,

                    EndDate =
                        DateTime.UtcNow.Date.AddDays(7),

                    StartTime =
                        new TimeSpan(9, 0, 0),

                    EndTime =
                        new TimeSpan(18, 0, 0),

                    DaysOfWeek =
                        127,

                    Priority =
                        1
                });

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var schedule =
            await response.Content
                .ReadFromJsonAsync<
                    PlaylistScheduleResponse>();

        schedule.Should()
            .NotBeNull();

        return schedule!;
    }


    private static async Task
        AuthenticateUserAsync(
            HttpClient client,
            Guid organizationId)
    {
        var email =
            $"schedule-auth-{Guid.NewGuid():N}@example.com";

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
                        "Schedule",

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