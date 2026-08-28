using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Authentication;
using SvgBillBoard.Application.DTOs.Media;

namespace SvgBillBoard.API.Tests.Tests.Media;

public class MediaTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MediaTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }


    // =========================================================
    // CREATE
    // =========================================================

    [Fact]
    public async Task CreateMedia_WithValidImage_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        using var content =
            CreateMultipartContent(
                "test.png",
                "image/png",
                "test image content",
                "Test Image");


        // Act
        var response =
            await _client.PostAsync(
                "/api/media",
                content);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var media =
            await response.Content
                .ReadFromJsonAsync<MediaResponse>();

        media.Should()
            .NotBeNull();

        media!.Id
            .Should()
            .NotBeEmpty();

        media.OrganizationId
            .Should()
            .Be(TestDataSeeder.OrganizationId);

        media.Name
            .Should()
            .Be("Test Image");

        media.FileName
            .Should()
            .Be("test.png");

        media.ContentType
            .Should()
            .Be("image/png");

        media.FileSize
            .Should()
            .BeGreaterThan(0);

        media.MediaType
            .Should()
            .Be(1);

        media.Status
            .Should()
            .Be(1);
    }


    [Fact]
    public async Task CreateMedia_WithoutFile_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        using var content =
            new MultipartFormDataContent();

        content.Add(
            new StringContent("Test Media"),
            "Name");


        // Act
        var response =
            await _client.PostAsync(
                "/api/media",
                content);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CreateMedia_WithEmptyFile_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        using var content =
            new MultipartFormDataContent();

        var fileContent =
            new ByteArrayContent(Array.Empty<byte>());

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(
                "image/png");

        content.Add(
            fileContent,
            "File",
            "empty.png");

        content.Add(
            new StringContent("Empty File"),
            "Name");


        // Act
        var response =
            await _client.PostAsync(
                "/api/media",
                content);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CreateMedia_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        using var content =
            CreateMultipartContent(
                "test.png",
                "image/png",
                "test",
                "");


        // Act
        var response =
            await _client.PostAsync(
                "/api/media",
                content);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CreateMedia_WithUnsupportedContentType_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        using var content =
            CreateMultipartContent(
                "test.txt",
                "text/plain",
                "plain text",
                "Invalid Media");


        // Act
        var response =
            await _client.PostAsync(
                "/api/media",
                content);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CreateMedia_WithJpeg_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        using var content =
            CreateMultipartContent(
                "test.jpg",
                "image/jpeg",
                "jpeg test",
                "JPEG Media");


        // Act
        var response =
            await _client.PostAsync(
                "/api/media",
                content);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var media =
            await response.Content
                .ReadFromJsonAsync<MediaResponse>();

        media.Should()
            .NotBeNull();

        media!.ContentType
            .Should()
            .Be("image/jpeg");

        media.MediaType
            .Should()
            .Be(1);
    }


    [Fact]
    public async Task CreateMedia_WithWebp_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        using var content =
            CreateMultipartContent(
                "test.webp",
                "image/webp",
                "webp test",
                "WebP Media");


        // Act
        var response =
            await _client.PostAsync(
                "/api/media",
                content);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var media =
            await response.Content
                .ReadFromJsonAsync<MediaResponse>();

        media.Should()
            .NotBeNull();

        media!.ContentType
            .Should()
            .Be("image/webp");

        media.MediaType
            .Should()
            .Be(1);
    }


    [Fact]
    public async Task CreateMedia_WithSvg_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        using var content =
            CreateMultipartContent(
                "test.svg",
                "image/svg+xml",
                "<svg></svg>",
                "SVG Media");


        // Act
        var response =
            await _client.PostAsync(
                "/api/media",
                content);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var media =
            await response.Content
                .ReadFromJsonAsync<MediaResponse>();

        media.Should()
            .NotBeNull();

        media!.ContentType
            .Should()
            .Be("image/svg+xml");

        media.MediaType
            .Should()
            .Be(1);
    }


    [Fact]
    public async Task CreateMedia_WithMp4_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        using var content =
            CreateMultipartContent(
                "test.mp4",
                "video/mp4",
                "fake video content",
                "Video Media");


        // Act
        var response =
            await _client.PostAsync(
                "/api/media",
                content);


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var media =
            await response.Content
                .ReadFromJsonAsync<MediaResponse>();

        media.Should()
            .NotBeNull();

        media!.ContentType
            .Should()
            .Be("video/mp4");

        media.MediaType
            .Should()
            .Be(2);
    }


    // =========================================================
    // GET ALL
    // =========================================================

    [Fact]
    public async Task GetMedia_WithValidToken_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        await CreateMediaAsync();


        // Act
        var response =
            await _client.GetAsync(
                "/api/media");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var media =
            await response.Content
                .ReadFromJsonAsync<
                    List<MediaResponse>>();

        media.Should()
            .NotBeNull();

        media!
            .Should()
            .NotBeEmpty();

        media
            .Should()
            .OnlyContain(x =>
                x.OrganizationId ==
                TestDataSeeder.OrganizationId);
    }


    // =========================================================
    // GET BY ID
    // =========================================================

    [Fact]
    public async Task GetMedia_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        var created =
            await CreateMediaAsync();


        // Act
        var response =
            await _client.GetAsync(
                $"/api/media/{created.Id}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var media =
            await response.Content
                .ReadFromJsonAsync<MediaResponse>();

        media.Should()
            .NotBeNull();

        media!.Id
            .Should()
            .Be(created.Id);

        media.Name
            .Should()
            .Be(created.Name);
    }


    [Fact]
    public async Task GetMedia_WithNonexistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();


        // Act
        var response =
            await _client.GetAsync(
                $"/api/media/{Guid.NewGuid()}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // DELETE
    // =========================================================

    [Fact]
    public async Task DeleteMedia_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        await AuthenticateUserAsync();

        var media =
            await CreateMediaAsync();


        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/media/{media.Id}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);
    }


    [Fact]
    public async Task DeleteMedia_WithNonexistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();


        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/media/{Guid.NewGuid()}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // HELPERS
    // =========================================================

    private async Task<MediaResponse>
        CreateMediaAsync()
    {
        using var content =
            CreateMultipartContent(
                $"media-{Guid.NewGuid():N}.png",
                "image/png",
                "test media content",
                $"Test Media {Guid.NewGuid():N}");


        var response =
            await _client.PostAsync(
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


    private static MultipartFormDataContent
        CreateMultipartContent(
            string fileName,
            string contentType,
            string content,
            string name)
    {
        var multipart =
            new MultipartFormDataContent();

        var fileContent =
            new ByteArrayContent(
                System.Text.Encoding.UTF8.GetBytes(
                    content));

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(
                contentType);

        multipart.Add(
            fileContent,
            "File",
            fileName);

        multipart.Add(
            new StringContent(name),
            "Name");

        return multipart;
    }


    private async Task AuthenticateUserAsync()
    {
        var email =
            $"media-tests-{Guid.NewGuid():N}@example.com";

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
                        "Media",

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
}