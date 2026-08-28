using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Authentication;
using SvgBillBoard.Application.DTOs.PlaylistSchedules;
using SvgBillBoard.Application.DTOs.Playlists;

namespace SvgBillBoard.API.Tests.Tests.PlaylistSchedules;

public class PlaylistScheduleTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PlaylistScheduleTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }


    // =========================================================
    // CREATE
    // =========================================================

    [Fact]
    public async Task CreateSchedule_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var request =
            new CreatePlaylistScheduleRequest
            {
                PlaylistId =
                    playlist.Id,

                LocationId =
                    TestDataSeeder.OrganizationLocationId,

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
                    5
            };


        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-schedules",
                request);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    PlaylistScheduleResponse>();

        result.Should()
            .NotBeNull();

        result!.Id
            .Should()
            .NotBeEmpty();

        result.OrganizationId
            .Should()
            .Be(TestDataSeeder.OrganizationId);

        result.PlaylistId
            .Should()
            .Be(playlist.Id);

        result.LocationId
            .Should()
            .Be(
                TestDataSeeder
                    .OrganizationLocationId);

        result.PlaylistName
            .Should()
            .Be(playlist.Name);

        result.LocationName
            .Should()
            .Be(
                "Test Organization Location");

        result.Priority
            .Should()
            .Be(5);

        result.DaysOfWeek
            .Should()
            .Be(127);

        result.Status
            .Should()
            .Be(1);
    }


    // =========================================================
    // VALIDATION
    // =========================================================

    [Fact]
    public async Task CreateSchedule_WithEmptyPlaylistId_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var request =
            new CreatePlaylistScheduleRequest
            {
                PlaylistId =
                    Guid.Empty,

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
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CreateSchedule_WithEmptyLocationId_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var request =
            new CreatePlaylistScheduleRequest
            {
                PlaylistId =
                    playlist.Id,

                LocationId =
                    Guid.Empty,

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
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CreateSchedule_WithEndDateBeforeStartDate_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var request =
            new CreatePlaylistScheduleRequest
            {
                PlaylistId =
                    playlist.Id,

                LocationId =
                    TestDataSeeder.OrganizationLocationId,

                StartDate =
                    DateTime.UtcNow.Date.AddDays(5),

                EndDate =
                    DateTime.UtcNow.Date,

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
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CreateSchedule_WithEndTimeBeforeStartTime_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var request =
            new CreatePlaylistScheduleRequest
            {
                PlaylistId =
                    playlist.Id,

                LocationId =
                    TestDataSeeder.OrganizationLocationId,

                StartTime =
                    new TimeSpan(18, 0, 0),

                EndTime =
                    new TimeSpan(9, 0, 0),

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
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CreateSchedule_WithInvalidDaysOfWeek_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var request =
            new CreatePlaylistScheduleRequest
            {
                PlaylistId =
                    playlist.Id,

                LocationId =
                    TestDataSeeder.OrganizationLocationId,

                DaysOfWeek =
                    128,

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
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CreateSchedule_WithNegativePriority_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

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
                    -1
            };


        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-schedules",
                request);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    // =========================================================
    // INVALID REFERENCES
    // =========================================================

    [Fact]
    public async Task CreateSchedule_WithNonexistentPlaylist_ShouldReturnConflict()
    {
        // Arrange
        await AuthenticateUserAsync();

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
            .Be(HttpStatusCode.Conflict);
    }


    [Fact]
    public async Task CreateSchedule_WithNonexistentLocation_ShouldReturnConflict()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var request =
            new CreatePlaylistScheduleRequest
            {
                PlaylistId =
                    playlist.Id,

                LocationId =
                    Guid.NewGuid(),

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
    // GET ALL
    // =========================================================

    [Fact]
    public async Task GetSchedules_WithValidToken_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        await CreateScheduleAsync();


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
            .NotBeEmpty();

        schedules
            .Should()
            .OnlyContain(x =>
                x.OrganizationId ==
                TestDataSeeder.OrganizationId);
    }


    // =========================================================
    // GET BY ID
    // =========================================================

    [Fact]
    public async Task GetSchedule_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        var schedule =
            await CreateScheduleAsync();


        // Act
        var response =
            await _client.GetAsync(
                $"/api/playlist-schedules/{schedule.Id}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    PlaylistScheduleResponse>();

        result.Should()
            .NotBeNull();

        result!.Id
            .Should()
            .Be(schedule.Id);

        result.PlaylistId
            .Should()
            .Be(schedule.PlaylistId);

        result.LocationId
            .Should()
            .Be(schedule.LocationId);
    }


    [Fact]
    public async Task GetSchedule_WithNonexistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();


        // Act
        var response =
            await _client.GetAsync(
                $"/api/playlist-schedules/{Guid.NewGuid()}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // DELETE
    // =========================================================

    [Fact]
    public async Task DeleteSchedule_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        await AuthenticateUserAsync();

        var schedule =
            await CreateScheduleAsync();


        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/playlist-schedules/{schedule.Id}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);


        // Verify
        var getResponse =
            await _client.GetAsync(
                $"/api/playlist-schedules/{schedule.Id}");

        getResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task DeleteSchedule_WithNonexistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();


        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/playlist-schedules/{Guid.NewGuid()}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // PRIORITY ORDER
    // =========================================================

    [Fact]
    public async Task GetSchedules_ShouldReturnHigherPriorityFirst()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        await CreateScheduleAsync(
            playlist.Id,
            priority: 1);

        await CreateScheduleAsync(
            playlist.Id,
            priority: 10);


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
            .First()
            .Priority
            .Should()
            .Be(10);
    }


    // =========================================================
    // HELPERS
    // =========================================================

    private async Task<PlaylistResponse>
        CreatePlaylistAsync()
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlists",
                new CreatePlaylistRequest
                {
                    Name =
                        $"Schedule Playlist {Guid.NewGuid():N}",

                    Description =
                        "Schedule test playlist"
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


    private async Task<PlaylistScheduleResponse>
        CreateScheduleAsync()
    {
        var playlist =
            await CreatePlaylistAsync();

        return await CreateScheduleAsync(
            playlist.Id,
            1);
    }


    private async Task<PlaylistScheduleResponse>
        CreateScheduleAsync(
            Guid playlistId,
            int priority)
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlist-schedules",
                new CreatePlaylistScheduleRequest
                {
                    PlaylistId =
                        playlistId,

                    LocationId =
                        TestDataSeeder
                            .OrganizationLocationId,

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
                        priority
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


    private async Task AuthenticateUserAsync()
    {
        var email =
            $"schedule-tests-{Guid.NewGuid():N}@example.com";

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
                        "Schedule",

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