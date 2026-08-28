using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Authentication;
using SvgBillBoard.Application.DTOs.Playlists;
using SvgBillBoard.Application.DTOs.Media;

namespace SvgBillBoard.API.Tests.Tests.Playlists;

public class PlaylistTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    public PlaylistTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }


    // =========================================================
    // CREATE
    // =========================================================

    [Fact]
    public async Task CreatePlaylist_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        var request =
            new CreatePlaylistRequest
            {
                Name =
                    "Test Playlist",

                Description =
                    "Playlist description"
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlists",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var playlist =
            await response.Content
                .ReadFromJsonAsync<PlaylistResponse>();

        playlist.Should()
            .NotBeNull();

        playlist!.Id
            .Should()
            .NotBeEmpty();

        playlist.OrganizationId
            .Should()
            .Be(TestDataSeeder.OrganizationId);

        playlist.Name
            .Should()
            .Be("Test Playlist");

        playlist.Description
            .Should()
            .Be("Playlist description");

        playlist.Status
            .Should()
            .Be(1);

        playlist.Items
            .Should()
            .NotBeNull();
    }


    [Fact]
    public async Task CreatePlaylist_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var request =
            new CreatePlaylistRequest
            {
                Name = string.Empty
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlists",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CreatePlaylist_WithWhitespaceName_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var request =
            new CreatePlaylistRequest
            {
                Name = "   "
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlists",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CreatePlaylist_ShouldTrimNameAndDescription()
    {
        // Arrange
        await AuthenticateUserAsync();

        var request =
            new CreatePlaylistRequest
            {
                Name =
                    "  Test Playlist  ",

                Description =
                    "  Test Description  "
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/playlists",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var playlist =
            await response.Content
                .ReadFromJsonAsync<PlaylistResponse>();

        playlist.Should()
            .NotBeNull();

        playlist!.Name
            .Should()
            .Be("Test Playlist");

        playlist.Description
            .Should()
            .Be("Test Description");
    }


    // =========================================================
    // GET
    // =========================================================

    [Fact]
    public async Task GetPlaylists_WithValidToken_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/playlists",
                new CreatePlaylistRequest
                {
                    Name =
                        "Get Playlist"
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
            .Contain(x =>
                x.Name ==
                "Get Playlist");
    }


    [Fact]
    public async Task GetPlaylist_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/playlists",
                new CreatePlaylistRequest
                {
                    Name =
                        "Single Playlist"
                });

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<PlaylistResponse>();

        created.Should()
            .NotBeNull();


        // Act
        var response =
            await _client.GetAsync(
                $"/api/playlists/{created!.Id}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var playlist =
            await response.Content
                .ReadFromJsonAsync<PlaylistResponse>();

        playlist.Should()
            .NotBeNull();

        playlist!.Id
            .Should()
            .Be(created.Id);

        playlist.Name
            .Should()
            .Be("Single Playlist");
    }


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
    // DELETE PLAYLIST
    // =========================================================

    [Fact]
    public async Task DeletePlaylist_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        await AuthenticateUserAsync();

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/playlists",
                new CreatePlaylistRequest
                {
                    Name =
                        "Playlist To Delete"
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
        var response =
            await _client.DeleteAsync(
                $"/api/playlists/{playlist!.Id}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);
    }


    [Fact]
    public async Task DeletePlaylist_WithNonexistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();

        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/playlists/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // PLAYLIST ITEMS
    // =========================================================

    [Fact]
    public async Task AddPlaylistItem_WithEmptyMediaId_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var request =
            new AddPlaylistItemRequest
            {
                MediaId =
                    Guid.Empty,

                DisplayOrder =
                    1,

                DurationSeconds =
                    10
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                $"/api/playlists/{playlist.Id}/items",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task AddPlaylistItem_WithInvalidDuration_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var request =
            new AddPlaylistItemRequest
            {
                MediaId =
                    Guid.NewGuid(),

                DisplayOrder =
                    1,

                DurationSeconds =
                    0
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                $"/api/playlists/{playlist.Id}/items",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task AddPlaylistItem_WithNonexistentPlaylist_ShouldReturnConflict()
    {
        // Arrange
        await AuthenticateUserAsync();

        var request =
            new AddPlaylistItemRequest
            {
                MediaId =
                    Guid.NewGuid(),

                DisplayOrder =
                    1,

                DurationSeconds =
                    10
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                $"/api/playlists/{Guid.NewGuid()}/items",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }


    [Fact]
    public async Task DeletePlaylistItem_WithNonexistentPlaylist_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();

        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/playlists/{Guid.NewGuid()}/items/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task DeletePlaylistItem_WithNonexistentItem_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/playlists/{playlist.Id}/items/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }

    // =========================================================
    // REORDER
    // =========================================================

    [Fact]
    public async Task ReorderPlaylistItems_WithEmptyItems_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var request =
            new ReorderPlaylistItemsRequest
            {
                Items = new List<PlaylistItemOrderRequest>()
            };

        // Act
        var response =
            await _client.PutAsJsonAsync(
                $"/api/playlists/{playlist.Id}/items/reorder",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task ReorderPlaylistItems_WithNonexistentPlaylist_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();

        var request =
            new ReorderPlaylistItemsRequest
            {
                Items =
                    new List<PlaylistItemOrderRequest>
                    {
                    new()
                    {
                        ItemId =
                            Guid.NewGuid(),

                        DisplayOrder =
                            1
                    }
                    }
            };

        // Act
        var response =
            await _client.PutAsJsonAsync(
                $"/api/playlists/{Guid.NewGuid()}/items/reorder",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task ReorderPlaylistItems_WithInvalidItemId_ShouldReturnConflict()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var request =
            new ReorderPlaylistItemsRequest
            {
                Items =
                    new List<PlaylistItemOrderRequest>
                    {
                    new()
                    {
                        ItemId =
                            Guid.NewGuid(),

                        DisplayOrder =
                            1
                    }
                    }
            };

        // Act
        var response =
            await _client.PutAsJsonAsync(
                $"/api/playlists/{playlist.Id}/items/reorder",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }


    [Fact]
    public async Task ReorderPlaylistItems_WithInvalidDisplayOrder_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var request =
            new ReorderPlaylistItemsRequest
            {
                Items =
                    new List<PlaylistItemOrderRequest>
                    {
                    new()
                    {
                        ItemId =
                            Guid.NewGuid(),

                        DisplayOrder =
                            0
                    }
                    }
            };

        // Act
        var response =
            await _client.PutAsJsonAsync(
                $"/api/playlists/{playlist.Id}/items/reorder",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }


    [Fact]
    public async Task ReorderPlaylistItems_WithDuplicateDisplayOrders_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var request =
            new ReorderPlaylistItemsRequest
            {
                Items =
                    new List<PlaylistItemOrderRequest>
                    {
                    new()
                    {
                        ItemId =
                            Guid.NewGuid(),

                        DisplayOrder =
                            1
                    },

                    new()
                    {
                        ItemId =
                            Guid.NewGuid(),

                        DisplayOrder =
                            1
                    }
                    }
            };

        // Act
        var response =
            await _client.PutAsJsonAsync(
                $"/api/playlists/{playlist.Id}/items/reorder",
                request);

        // Assert
        response.StatusCode
            .Should()
            .BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.Conflict);
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
                        $"Playlist-{Guid.NewGuid():N}"
                });

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var playlist =
            await response.Content
                .ReadFromJsonAsync<PlaylistResponse>();

        playlist.Should()
            .NotBeNull();

        return playlist!;
    }

    [Fact]
    public async Task AddPlaylistItem_WithValidMedia_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var media =
            await CreateMediaAsync();

        var request =
            new AddPlaylistItemRequest
            {
                MediaId =
                    media.Id,

                DisplayOrder =
                    1,

                DurationSeconds =
                    10
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                $"/api/playlists/{playlist.Id}/items",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var item =
            await response.Content
                .ReadFromJsonAsync<PlaylistItemResponse>();

        item.Should()
            .NotBeNull();

        item!.Id
            .Should()
            .NotBeEmpty();

        item.MediaId
            .Should()
            .Be(media.Id);

        item.MediaName
            .Should()
            .Be(media.Name);

        item.DisplayOrder
            .Should()
            .Be(1);

        item.DurationSeconds
            .Should()
            .Be(10);
    }

    [Fact]
    public async Task AddPlaylistItem_WithMediaFromAnotherOrganization_ShouldReturnConflict()
    {
        // Arrange
        await AuthenticateUserAsync(
            _client,
            TestDataSeeder.OrganizationId);

        var playlist =
            await CreatePlaylistAsync();

        using var secondClient =
            _factory.CreateClient();

        await AuthenticateUserAsync(
            secondClient,
            TestDataSeeder.SecondOrganizationId);

        var media =
            await CreateMediaAsync(
                secondClient);

        var request =
            new AddPlaylistItemRequest
            {
                MediaId =
                    media.Id,

                DisplayOrder =
                    1,

                DurationSeconds =
                    10
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                $"/api/playlists/{playlist.Id}/items",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeletePlaylistItem_WithValidItem_ShouldReturnNoContent()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var media =
            await CreateMediaAsync();

        var addResponse =
            await _client.PostAsJsonAsync(
                $"/api/playlists/{playlist.Id}/items",
                new AddPlaylistItemRequest
                {
                    MediaId =
                        media.Id,

                    DisplayOrder =
                        1,

                    DurationSeconds =
                        10
                });

        addResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var item =
            await addResponse.Content
                .ReadFromJsonAsync<PlaylistItemResponse>();

        item.Should()
            .NotBeNull();

        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/playlists/{playlist.Id}/items/{item!.Id}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);
    }
    [Fact]
    public async Task DeletePlaylistItem_FromAnotherPlaylist_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist1 =
            await CreatePlaylistAsync();

        var playlist2 =
            await CreatePlaylistAsync();

        var media =
            await CreateMediaAsync();

        var addResponse =
            await _client.PostAsJsonAsync(
                $"/api/playlists/{playlist1.Id}/items",
                new AddPlaylistItemRequest
                {
                    MediaId =
                        media.Id,

                    DisplayOrder =
                        1,

                    DurationSeconds =
                        10
                });

        addResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var item =
            await addResponse.Content
                .ReadFromJsonAsync<PlaylistItemResponse>();

        item.Should()
            .NotBeNull();

        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/playlists/{playlist2.Id}/items/{item!.Id}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReorderPlaylistItems_WithValidItems_ShouldReturnNoContent()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var media1 =
            await CreateMediaAsync();

        var media2 =
            await CreateMediaAsync();

        var add1 =
            await AddPlaylistItemAsync(
                playlist.Id,
                media1.Id,
                1,
                10);

        var add2 =
            await AddPlaylistItemAsync(
                playlist.Id,
                media2.Id,
                2,
                20);

        // Act
        var response =
            await _client.PutAsJsonAsync(
                $"/api/playlists/{playlist.Id}/items/reorder",
                new ReorderPlaylistItemsRequest
                {
                    Items =
                        new List<PlaylistItemOrderRequest>
                        {
                        new()
                        {
                            ItemId =
                                add1.Id,

                            DisplayOrder =
                                2
                        },

                        new()
                        {
                            ItemId =
                                add2.Id,

                            DisplayOrder =
                                1
                        }
                        }
                });

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ReorderPlaylistItems_ShouldUpdateDisplayOrders()
    {
        // Arrange
        await AuthenticateUserAsync();

        var playlist =
            await CreatePlaylistAsync();

        var media1 =
            await CreateMediaAsync();

        var media2 =
            await CreateMediaAsync();

        var item1 =
            await AddPlaylistItemAsync(
                playlist.Id,
                media1.Id,
                1,
                10);

        var item2 =
            await AddPlaylistItemAsync(
                playlist.Id,
                media2.Id,
                2,
                20);

        // Act
        var reorderResponse =
            await _client.PutAsJsonAsync(
                $"/api/playlists/{playlist.Id}/items/reorder",
                new ReorderPlaylistItemsRequest
                {
                    Items =
                        new List<PlaylistItemOrderRequest>
                        {
                        new()
                        {
                            ItemId =
                                item1.Id,

                            DisplayOrder =
                                2
                        },

                        new()
                        {
                            ItemId =
                                item2.Id,

                            DisplayOrder =
                                1
                        }
                        }
                });

        reorderResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);


        // Verify
        var getResponse =
            await _client.GetAsync(
                $"/api/playlists/{playlist.Id}");

        getResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await getResponse.Content
                .ReadFromJsonAsync<PlaylistResponse>();

        result.Should()
            .NotBeNull();

        result!.Items
            .Should()
            .NotBeNull();

        var reorderedItem1 =
            result.Items
                .First(x => x.Id == item1.Id);

        var reorderedItem2 =
            result.Items
                .First(x => x.Id == item2.Id);

        reorderedItem1.DisplayOrder
            .Should()
            .Be(2);

        reorderedItem2.DisplayOrder
            .Should()
            .Be(1);
    }

    private async Task<MediaResponse>
    CreateMediaAsync()
    {
        return await CreateMediaAsync(
            _client);
    }


    private static async Task<MediaResponse>
        CreateMediaAsync(
            HttpClient client)
    {
        using var content =
            new MultipartFormDataContent();

        var bytes =
            System.Text.Encoding.UTF8.GetBytes(
                "test media content");

        var fileContent =
            new ByteArrayContent(bytes);

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(
                "image/png");

        content.Add(
            fileContent,
            "File",
            $"playlist-{Guid.NewGuid():N}.png");

        content.Add(
            new StringContent(
                $"Playlist Media {Guid.NewGuid():N}"),
            "Name");

        var response =
            await client.PostAsync(
                "/api/media",
                content);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var media =
            await response.Content
                .ReadFromJsonAsync<MediaResponse>();

        media.Should()
            .NotBeNull();

        return media!;
    }


    private async Task<PlaylistItemResponse>
        AddPlaylistItemAsync(
            Guid playlistId,
            Guid mediaId,
            int displayOrder,
            int durationSeconds)
    {
        var response =
            await _client.PostAsJsonAsync(
                $"/api/playlists/{playlistId}/items",
                new AddPlaylistItemRequest
                {
                    MediaId =
                        mediaId,

                    DisplayOrder =
                        displayOrder,

                    DurationSeconds =
                        durationSeconds
                });

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var item =
            await response.Content
                .ReadFromJsonAsync<
                    PlaylistItemResponse>();

        item.Should()
            .NotBeNull();

        return item!;
    }

    private async Task AuthenticateUserAsync()
    {
        var email =
            $"playlist-tests-{Guid.NewGuid():N}@example.com";

        var password =
            "Test@12345";

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    OrganizationId =
                        TestDataSeeder.OrganizationId,

                    FirstName = "Playlist",
                    LastName = "Test",

                    Email = email,
                    Password = password,

                    Phone = "9876543210",
                    RoleCode = "VIEWER"
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

                    Email = email,
                    Password = password
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

    private static async Task AuthenticateUserAsync(
    HttpClient client,
    Guid organizationId)
    {
        var email =
            $"playlist-tests-{Guid.NewGuid():N}@example.com";

        var password =
            "Test@12345";

        var registerResponse =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    OrganizationId =
                        organizationId,

                    FirstName = "Playlist",
                    LastName = "Test",

                    Email = email,
                    Password = password,

                    Phone = "9876543210",
                    RoleCode = "VIEWER"
                });

        registerResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var loginResponse =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    OrganizationId =
                        organizationId,

                    Email = email,
                    Password = password
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