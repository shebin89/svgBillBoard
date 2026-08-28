using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Authentication;
using SvgBillBoard.Application.DTOs.Media;

namespace SvgBillBoard.API.Tests.Tests.Media;

public class MediaAuthorizationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public MediaAuthorizationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }


    // =========================================================
    // GET ALL - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task GetMedia_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                "/api/media");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // GET BY ID - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task GetMediaById_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                $"/api/media/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // CREATE - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task CreateMedia_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        using var content =
            CreateMultipartContent(
                "unauthorized.png",
                "image/png",
                "test",
                "Unauthorized Media");

        // Act
        var response =
            await _client.PostAsync(
                "/api/media",
                content);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // DELETE - WITHOUT TOKEN
    // =========================================================

    [Fact]
    public async Task DeleteMedia_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/media/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // =========================================================
    // GET ALL - VALID USER
    // =========================================================

    [Fact]
    public async Task GetMedia_WithValidUserToken_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

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
            .OnlyContain(x =>
                x.OrganizationId ==
                TestDataSeeder.OrganizationId);
    }


    // =========================================================
    // GET OTHER ORGANIZATION MEDIA
    // =========================================================

    [Fact]
    public async Task GetMedia_FromAnotherOrganization_ShouldReturnNotFound()
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


        // Create media as Organization B.
        var createResponse =
            await CreateMediaAsync(
                secondClient,
                "Organization B Media");

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var media =
            await createResponse.Content
                .ReadFromJsonAsync<MediaResponse>();

        media.Should()
            .NotBeNull();


        // Act
        // Organization A tries to access Organization B media.
        var response =
            await _client.GetAsync(
                $"/api/media/{media!.Id}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // GET ALL - ORGANIZATION ISOLATION
    // =========================================================

    [Fact]
    public async Task GetMedia_ShouldReturnOnlyCurrentOrganizationMedia()
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


        // Create Organization B media.
        var createResponse =
            await CreateMediaAsync(
                secondClient,
                "Organization B Media");

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);


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
            .OnlyContain(x =>
                x.OrganizationId ==
                TestDataSeeder.OrganizationId);

        media!
            .Should()
            .NotContain(x =>
                x.OrganizationId ==
                TestDataSeeder.SecondOrganizationId);
    }


    // =========================================================
    // DELETE OTHER ORGANIZATION MEDIA
    // =========================================================

    [Fact]
    public async Task DeleteMedia_FromAnotherOrganization_ShouldReturnNotFound()
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


        // Create Organization B media.
        var createResponse =
            await CreateMediaAsync(
                secondClient,
                "Organization B Media");

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var media =
            await createResponse.Content
                .ReadFromJsonAsync<MediaResponse>();

        media.Should()
            .NotBeNull();


        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/media/{media!.Id}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // HELPERS
    // =========================================================

    private static async Task<HttpResponseMessage>
        CreateMediaAsync(
            HttpClient client,
            string name)
    {
        using var content =
            CreateMultipartContent(
                $"media-{Guid.NewGuid():N}.png",
                "image/png",
                "organization media",
                name);

        return await client.PostAsync(
            "/api/media",
            content);
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
        await AuthenticateUserAsync(
            _client,
            TestDataSeeder.OrganizationId);
    }


    private static async Task AuthenticateUserAsync(
        HttpClient client,
        Guid organizationId)
    {
        var email =
            $"media-auth-{Guid.NewGuid():N}@example.com";

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
                        "Media",

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