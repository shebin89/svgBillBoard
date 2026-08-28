using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Organization;

namespace SvgBillBoard.API.Tests.Tests.Organizations;

public class OrganizationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrganizationTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }


    // =========================================================
    // CREATE
    // =========================================================

    [Fact]
    public async Task CreateOrganization_WithValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request =
            new CreateOrganizationRequest
            {
                Name =
                    $"Test Organization {Guid.NewGuid():N}",

                Code =
                    $"ORG-{Guid.NewGuid():N}",

                Email =
                    "organization@test.com",

                Phone =
                    "9876543210"
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/organizations",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    OrganizationResponse>();

        result.Should()
            .NotBeNull();

        result!.Id
            .Should()
            .NotBe(Guid.Empty);

        result.Name
            .Should()
            .Be(request.Name);

        result.Code
            .Should()
            .Be(request.Code);

        result.Email
            .Should()
            .Be(request.Email);

        result.Phone
            .Should()
            .Be(request.Phone);

        result.Status
            .Should()
            .Be(1);
    }


    // =========================================================
    // CREATE - DUPLICATE CODE
    // =========================================================

    [Fact]
    public async Task CreateOrganization_WithDuplicateCode_ShouldReturnConflict()
    {
        // Arrange
        var code =
            $"DUP-{Guid.NewGuid():N}";

        var firstRequest =
            new CreateOrganizationRequest
            {
                Name =
                    "First Organization",

                Code =
                    code,

                Email =
                    "first@test.com",

                Phone =
                    "9876543210"
            };

        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/organizations",
                firstRequest);

        firstResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);


        var secondRequest =
            new CreateOrganizationRequest
            {
                Name =
                    "Second Organization",

                Code =
                    code,

                Email =
                    "second@test.com",

                Phone =
                    "9876543211"
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/organizations",
                secondRequest);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }


    // =========================================================
    // GET ALL
    // =========================================================

    [Fact]
    public async Task GetOrganizations_ShouldReturnOk()
    {
        // Arrange
        var request =
            new CreateOrganizationRequest
            {
                Name =
                    $"Get All Organization {Guid.NewGuid():N}",

                Code =
                    $"GETALL-{Guid.NewGuid():N}",

                Email =
                    "getall@test.com",

                Phone =
                    "9876543210"
            };

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/organizations",
                request);

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        // Act
        var response =
            await _client.GetAsync(
                "/api/organizations");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    List<OrganizationResponse>>();

        result.Should()
            .NotBeNull();

        result!
            .Should()
            .Contain(x =>
                x.Code == request.Code);
    }


    // =========================================================
    // GET BY ID
    // =========================================================

    [Fact]
    public async Task GetOrganizationById_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var request =
            new CreateOrganizationRequest
            {
                Name =
                    $"Get By Id Organization {Guid.NewGuid():N}",

                Code =
                    $"GETID-{Guid.NewGuid():N}",

                Email =
                    "getbyid@test.com",

                Phone =
                    "9876543210"
            };

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/organizations",
                request);

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<
                    OrganizationResponse>();

        created.Should()
            .NotBeNull();

        // Act
        var response =
            await _client.GetAsync(
                $"/api/organizations/{created!.Id}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    OrganizationResponse>();

        result.Should()
            .NotBeNull();

        result!.Id
            .Should()
            .Be(created.Id);

        result.Name
            .Should()
            .Be(request.Name);

        result.Code
            .Should()
            .Be(request.Code);
    }


    // =========================================================
    // GET BY ID - NONEXISTENT
    // =========================================================

    [Fact]
    public async Task GetOrganizationById_WithNonexistentId_ShouldReturnNotFound()
    {
        // Arrange
        var id =
            Guid.NewGuid();

        // Act
        var response =
            await _client.GetAsync(
                $"/api/organizations/{id}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // UPDATE
    // =========================================================

    [Fact]
    public async Task UpdateOrganization_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest =
            new CreateOrganizationRequest
            {
                Name =
                    $"Update Organization {Guid.NewGuid():N}",

                Code =
                    $"UPDATE-{Guid.NewGuid():N}",

                Email =
                    "before@test.com",

                Phone =
                    "9876543210"
            };

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/organizations",
                createRequest);

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<
                    OrganizationResponse>();

        created.Should()
            .NotBeNull();


        var updateRequest =
            new UpdateOrganizationRequest
            {
                Name =
                    "Updated Organization",

                Email =
                    "updated@test.com",

                Phone =
                    "9999999999"
            };

        // Act
        var response =
            await _client.PutAsJsonAsync(
                $"/api/organizations/{created!.Id}",
                updateRequest);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);


        // Verify
        var getResponse =
            await _client.GetAsync(
                $"/api/organizations/{created.Id}");

        getResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await getResponse.Content
                .ReadFromJsonAsync<
                    OrganizationResponse>();

        result.Should()
            .NotBeNull();

        result!.Name
            .Should()
            .Be(updateRequest.Name);

        result.Email
            .Should()
            .Be(updateRequest.Email);

        result.Phone
            .Should()
            .Be(updateRequest.Phone);

        // Code should remain unchanged
        result.Code
            .Should()
            .Be(createRequest.Code);
    }


    // =========================================================
    // UPDATE - NONEXISTENT
    // =========================================================

    [Fact]
    public async Task UpdateOrganization_WithNonexistentId_ShouldReturnNotFound()
    {
        // Arrange
        var request =
            new UpdateOrganizationRequest
            {
                Name =
                    "Updated Organization",

                Email =
                    "updated@test.com",

                Phone =
                    "9999999999"
            };

        // Act
        var response =
            await _client.PutAsJsonAsync(
                $"/api/organizations/{Guid.NewGuid()}",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }


    // =========================================================
    // DEACTIVATE
    // =========================================================

    [Fact]
    public async Task DeactivateOrganization_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest =
            new CreateOrganizationRequest
            {
                Name =
                    $"Deactivate Organization {Guid.NewGuid():N}",

                Code =
                    $"DEACTIVATE-{Guid.NewGuid():N}",

                Email =
                    "deactivate@test.com",

                Phone =
                    "9876543210"
            };

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/organizations",
                createRequest);

        createResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<
                    OrganizationResponse>();

        created.Should()
            .NotBeNull();


        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/organizations/{created!.Id}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);


        // Verify status changed to inactive
        var getResponse =
            await _client.GetAsync(
                $"/api/organizations/{created.Id}");

        getResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await getResponse.Content
                .ReadFromJsonAsync<
                    OrganizationResponse>();

        result.Should()
            .NotBeNull();

        result!.Status
            .Should()
            .Be(0);
    }


    // =========================================================
    // DEACTIVATE - NONEXISTENT
    // =========================================================

    [Fact]
    public async Task DeactivateOrganization_WithNonexistentId_ShouldReturnNotFound()
    {
        // Act
        var response =
            await _client.DeleteAsync(
                $"/api/organizations/{Guid.NewGuid()}");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }
}