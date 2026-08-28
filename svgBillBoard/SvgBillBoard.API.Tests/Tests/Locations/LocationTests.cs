using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Locations;
using SvgBillBoard.Infrastructure.Persistence;

namespace SvgBillBoard.API.Tests.Tests.Locations;

public class LocationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    private readonly CustomWebApplicationFactory _factory;

    public LocationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }


    // =========================================================
    // CREATE
    // =========================================================

    [Fact]
    public async Task CreateLocation_WithValidRequest_ShouldReturnCreated()
    {
        // Arrange
        await AuthenticateUserAsync();

        var code =
            $"LOC-{Guid.NewGuid():N}"[..12];

        var request =
            new CreateLocationRequest
            {
                Name =
                    "New Test Location",

                Code =
                    code,

                AddressLine1 =
                    "123 Test Street",

                AddressLine2 =
                    "Building A",

                City =
                    "Kochi",

                State =
                    "Kerala",

                PostalCode =
                    "682001",

                Country =
                    "India",

                Latitude =
                    9.9312,

                Longitude =
                    76.2673
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/locations",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var location =
            await response.Content
                .ReadFromJsonAsync<LocationResponse>();

        location.Should()
            .NotBeNull();

        location!.Id
            .Should()
            .NotBeEmpty();

        location.OrganizationId
            .Should()
            .Be(TestDataSeeder.OrganizationId);

        location.Name
            .Should()
            .Be("New Test Location");

        location.Code
            .Should()
            .Be(code);

        location.Status
            .Should()
            .Be(1);
    }


    [Fact]
    public async Task CreateLocation_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var request =
            new CreateLocationRequest
            {
                Name =
                    string.Empty,

                Code =
                    $"EMPTY-{Guid.NewGuid():N}"[..12]
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/locations",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CreateLocation_WithWhitespaceName_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateUserAsync();

        var request =
            new CreateLocationRequest
            {
                Name =
                    "   ",

                Code =
                    $"SPACE-{Guid.NewGuid():N}"[..12]
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/locations",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CreateLocation_WithoutCode_ShouldReturnCreated()
    {
        // Arrange
        await AuthenticateUserAsync();

        var request =
            new CreateLocationRequest
            {
                Name =
                    "Location Without Code",

                Code = null,

                City =
                    "Kochi",

                State =
                    "Kerala",

                Country =
                    "India"
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/locations",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var location =
            await response.Content
                .ReadFromJsonAsync<LocationResponse>();

        location.Should()
            .NotBeNull();

        location!.Name
            .Should()
            .Be("Location Without Code");

        location.Code
            .Should()
            .BeNull();

        location.Status
            .Should()
            .Be(1);
    }


    [Fact]
    public async Task CreateLocation_WithDuplicateCode_ShouldReturnConflict()
    {
        // Arrange
        await AuthenticateUserAsync();

        var code =
            $"DUP-{Guid.NewGuid():N}"[..12];


        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/locations",
                new CreateLocationRequest
                {
                    Name =
                        "First Location",

                    Code =
                        code
                });

        firstResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);


        // Act
        var secondResponse =
            await _client.PostAsJsonAsync(
                "/api/locations",
                new CreateLocationRequest
                {
                    Name =
                        "Second Location",

                    Code =
                        code
                });


        // Assert
        secondResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }


    [Fact]
    public async Task CreateLocation_WithCodeContainingWhitespace_ShouldTrimCode()
    {
        // Arrange
        await AuthenticateUserAsync();

        var code =
            $"TRIM-{Guid.NewGuid():N}"[..12];

        var request =
            new CreateLocationRequest
            {
                Name =
                    "Trim Code Location",

                Code =
                    $"  {code}  "
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/locations",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var location =
            await response.Content
                .ReadFromJsonAsync<LocationResponse>();

        location.Should()
            .NotBeNull();

        location!.Code
            .Should()
            .Be(code);
    }


    // =========================================================
    // GET
    // =========================================================

    [Fact]
    public async Task GetLocation_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/locations",
                new CreateLocationRequest
                {
                    Name =
                        "Get Test Location",

                    Code =
                        $"GET-{Guid.NewGuid():N}"[..12],

                    City =
                        "Kochi",

                    State =
                        "Kerala",

                    Country =
                        "India"
                });

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<LocationResponse>();

        created.Should()
            .NotBeNull();


        // Act
        var response =
            await _client.GetAsync(
                $"/api/locations/{created!.Id}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<LocationResponse>();

        result.Should()
            .NotBeNull();

        result!.Id
            .Should()
            .Be(created.Id);

        result.Name
            .Should()
            .Be("Get Test Location");
    }


    [Fact]
    public async Task GetLocation_WithNonexistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();

        var id =
            Guid.NewGuid();

        // Act
        var response =
            await _client.GetAsync(
                $"/api/locations/{id}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // UPDATE
    // =========================================================

    [Fact]
    public async Task UpdateLocation_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateUserAsync();

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/locations",
                new CreateLocationRequest
                {
                    Name =
                        "Location Before Update",

                    Code =
                        $"UPD-{Guid.NewGuid():N}"[..12],

                    City =
                        "Kochi",

                    State =
                        "Kerala",

                    Country =
                        "India"
                });

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<LocationResponse>();

        created.Should()
            .NotBeNull();


        var updateCode =
            $"NEW-{Guid.NewGuid():N}"[..12];

        var updateRequest =
            new UpdateLocationRequest
            {
                Name =
                    "Location After Update",

                Code =
                    updateCode,

                AddressLine1 =
                    "456 Updated Street",

                City =
                    "Trivandrum",

                State =
                    "Kerala",

                PostalCode =
                    "695001",

                Country =
                    "India",

                Latitude =
                    8.5241,

                Longitude =
                    76.9366
            };


        // Act
        var response =
            await _client.PutAsJsonAsync(
                $"/api/locations/{created!.Id}",
                updateRequest);


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
            .Be(created.Id);

        location.Name
            .Should()
            .Be("Location After Update");

        location.Code
            .Should()
            .Be(updateCode);

        location.City
            .Should()
            .Be("Trivandrum");

        location.Latitude
            .Should()
            .Be(8.5241);

        location.Longitude
            .Should()
            .Be(76.9366);
    }


    [Fact]
    public async Task UpdateLocation_WithNonexistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();

        var request =
            new UpdateLocationRequest
            {
                Name =
                    "Updated Location",

                Code =
                    $"NONE-{Guid.NewGuid():N}"[..12],

                City =
                    "Kochi",

                State =
                    "Kerala",

                Country =
                    "India"
            };

        // Act
        var response =
            await _client.PutAsJsonAsync(
                $"/api/locations/{Guid.NewGuid()}",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task UpdateLocation_WithDuplicateCode_ShouldReturnConflict()
    {
        // Arrange
        await AuthenticateUserAsync();

        var firstCode =
            $"FIRST-{Guid.NewGuid():N}"[..12];

        var secondCode =
            $"SECOND-{Guid.NewGuid():N}"[..12];


        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/locations",
                new CreateLocationRequest
                {
                    Name =
                        "First Location",

                    Code =
                        firstCode
                });

        firstResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);


        var secondResponse =
            await _client.PostAsJsonAsync(
                "/api/locations",
                new CreateLocationRequest
                {
                    Name =
                        "Second Location",

                    Code =
                        secondCode
                });

        secondResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var secondLocation =
            await secondResponse.Content
                .ReadFromJsonAsync<LocationResponse>();

        secondLocation.Should()
            .NotBeNull();


        // Act
        var response =
            await _client.PutAsJsonAsync(
                $"/api/locations/{secondLocation!.Id}",
                new UpdateLocationRequest
                {
                    Name =
                        "Updated Second Location",

                    Code =
                        firstCode,

                    City =
                        "Kochi",

                    State =
                        "Kerala",

                    Country =
                        "India"
                });


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }


    // =========================================================
    // DELETE
    // =========================================================

    [Fact]
    public async Task DeleteLocation_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        await AuthenticateUserAsync();

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/locations",
                new CreateLocationRequest
                {
                    Name =
                        "Location To Delete",

                    Code =
                        $"DEL-{Guid.NewGuid():N}"[..12]
                });

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var location =
            await createResponse.Content
                .ReadFromJsonAsync<LocationResponse>();

        location.Should()
            .NotBeNull();


        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/locations/{location!.Id}");


        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);
    }


    [Fact]
    public async Task DeleteLocation_WithNonexistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateUserAsync();

        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/locations/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task DeleteLocation_ShouldSoftDelete()
    {
        // Arrange
        await AuthenticateUserAsync();

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/locations",
                new CreateLocationRequest
                {
                    Name =
                        "Soft Delete Location",

                    Code =
                        $"SOFT-{Guid.NewGuid():N}"[..12]
                });

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var location =
            await createResponse.Content
                .ReadFromJsonAsync<LocationResponse>();

        location.Should()
            .NotBeNull();


        // Act
        var deleteResponse =
            await _client.DeleteAsync(
                $"/api/locations/{location!.Id}");


        // Assert
        deleteResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);


        // Verify soft delete in database
        using var scope =
            _factory.Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var databaseLocation =
            await context.Locations
                .FirstOrDefaultAsync(
                    x =>
                        x.Id ==
                        location.Id);

        databaseLocation.Should()
            .NotBeNull();

        databaseLocation!.Status
            .Should()
            .Be(0);
    }


    // =========================================================
    // HELPER
    // =========================================================

    private async Task AuthenticateUserAsync()
    {
        var email =
            $"location-tests-{Guid.NewGuid():N}@example.com";

        var password =
            "Test@12345";


        // Register
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new SvgBillBoard.Application.DTOs.Authentication.RegisterRequest
                {
                    OrganizationId =
                        TestDataSeeder.OrganizationId,

                    FirstName =
                        "Location",

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
                .ReadFromJsonAsync<
                    SvgBillBoard.Application.DTOs.Authentication.LoginResponse>();

        login.Should()
            .NotBeNull();

        login!.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();


        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);
    }
}