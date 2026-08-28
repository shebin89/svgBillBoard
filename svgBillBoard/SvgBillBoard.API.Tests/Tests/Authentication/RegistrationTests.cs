using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Authentication;
using SvgBillBoard.Infrastructure.Persistence;

namespace SvgBillBoard.API.Tests.Tests.Authentication;

public class RegistrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public RegistrationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturnCreatedUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            OrganizationId =
                TestDataSeeder.OrganizationId,

            FirstName = "Test",

            LastName = "User",

            Email =
                $"test-{Guid.NewGuid()}@example.com",

            Password = "Test@12345",

            Phone = "9876543210",

            RoleCode = "VIEWER"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<UserResponse>();

        result.Should().NotBeNull();

        result!.Email
            .Should()
            .Be(request.Email.ToLowerInvariant());

        result.FirstName
            .Should()
            .Be(request.FirstName);

        result.LastName
            .Should()
            .Be(request.LastName);

        result.OrganizationId
            .Should()
            .Be(request.OrganizationId);

        result.Roles
            .Should()
            .Contain("VIEWER");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var email =
            $"duplicate-{Guid.NewGuid()}@example.com";

        var request = new RegisterRequest
        {
            OrganizationId =
                TestDataSeeder.OrganizationId,

            FirstName = "Duplicate",

            LastName = "User",

            Email = email,

            Password = "Test@12345",

            Phone = "9876543210",

            RoleCode = "VIEWER"
        };

        // First registration
        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        firstResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Second registration
        var secondResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        secondResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithInvalidRole_ShouldReturnConflict()
    {
        // Arrange
        var request = new RegisterRequest
        {
            OrganizationId =
                TestDataSeeder.OrganizationId,

            FirstName = "Test",

            LastName = "InvalidRole",

            Email =
                $"invalid-role-{Guid.NewGuid()}@example.com",

            Password = "Test@12345",

            Phone = "9876543210",

            RoleCode = "INVALID_ROLE"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithInvalidOrganization_ShouldReturnConflict()
    {
        // Arrange
        var request = new RegisterRequest
        {
            OrganizationId = Guid.NewGuid(),

            FirstName = "Test",

            LastName = "InvalidOrg",

            Email =
                $"invalid-org-{Guid.NewGuid()}@example.com",

            Password = "Test@12345",

            Phone = "9876543210",

            RoleCode = "VIEWER"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithValidRequest_ShouldPersistUserAndRole()
    {
        // Arrange
        var email =
            $"persist-{Guid.NewGuid()}@example.com";

        var request = new RegisterRequest
        {
            OrganizationId =
                TestDataSeeder.OrganizationId,

            FirstName = "Persist",

            LastName = "Test",

            Email = email,

            Password = "Test@12345",

            Phone = "9876543210",

            RoleCode = "VIEWER"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Access test database
        using var scope =
            _factory.Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var user =
            await context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(
                    x => x.Email == email);

        user.Should().NotBeNull();

        user!.OrganizationId
            .Should()
            .Be(TestDataSeeder.OrganizationId);

        user.FirstName
            .Should()
            .Be("Persist");

        user.LastName
            .Should()
            .Be("Test");

        user.PasswordHash
            .Should()
            .NotBeNullOrWhiteSpace();

        user.PasswordHash
            .Should()
            .NotBe("Test@12345");

        user.UserRoles
            .Should()
            .ContainSingle();

        user.UserRoles
            .First()
            .Role!
            .Code
            .Should()
            .Be("VIEWER");
    }
}