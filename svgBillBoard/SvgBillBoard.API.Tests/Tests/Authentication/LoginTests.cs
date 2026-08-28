using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Application.DTOs.Authentication;
using SvgBillBoard.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SvgBillBoard.API.Tests.Tests.Authentication;

public class LoginTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    private readonly CustomWebApplicationFactory _factory;

    public LoginTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new
        {
            OrganizationId =
                TestDataSeeder.OrganizationId,

            Email =
                "invalid@test.com",

            Password =
                "wrong-password"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var email =
            $"login-{Guid.NewGuid()}@example.com";

        var password =
            "Test@12345";

        var registerRequest =
            new RegisterRequest
            {
                OrganizationId =
                    TestDataSeeder.OrganizationId,

                FirstName =
                    "Login",

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
            };

        // Create user first
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                registerRequest);

        registerResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Login
        var loginRequest = new
        {
            OrganizationId =
                TestDataSeeder.OrganizationId,

            Email =
                email,

            Password =
                password
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<LoginResponse>();

        result.Should()
            .NotBeNull();

        result!.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();

        result.ExpiresAt
            .Should()
            .BeAfter(DateTime.UtcNow);

        result.User
            .Should()
            .NotBeNull();

        result.User.Email
            .Should()
            .Be(email);

        result.User.Roles
            .Should()
            .Contain("VIEWER");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var email =
            $"wrong-password-{Guid.NewGuid()}@example.com";

        var correctPassword =
            "Test@12345";

        var registerRequest =
            new RegisterRequest
            {
                OrganizationId =
                    TestDataSeeder.OrganizationId,

                FirstName =
                    "Wrong",

                LastName =
                    "Password",

                Email =
                    email,

                Password =
                    correctPassword,

                Phone =
                    "9876543210",

                RoleCode =
                    "VIEWER"
            };

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                registerRequest);

        registerResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Act
        var loginRequest = new
        {
            OrganizationId =
                TestDataSeeder.OrganizationId,

            Email =
                email,

            Password =
                "WrongPassword@123"
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new
        {
            OrganizationId =
                TestDataSeeder.OrganizationId,

            Email =
                $"unknown-{Guid.NewGuid()}@example.com",

            Password =
                "Test@12345"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInactiveUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var email =
            $"inactive-{Guid.NewGuid()}@example.com";

        var password =
            "Test@12345";

        var registerRequest =
            new RegisterRequest
            {
                OrganizationId =
                    TestDataSeeder.OrganizationId,

                FirstName =
                    "Inactive",

                LastName =
                    "User",

                Email =
                    email,

                Password =
                    password,

                Phone =
                    "9876543210",

                RoleCode =
                    "VIEWER"
            };

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                registerRequest);

        registerResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Change user status to inactive
        using var scope =
            _factory.Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var user =
            await context.Users
                .FirstAsync(
                    x => x.Email == email);

        user.Status = 0;

        await context.SaveChangesAsync();

        // Act
        var loginRequest = new
        {
            OrganizationId =
                TestDataSeeder.OrganizationId,

            Email =
                email,

            Password =
                password
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithValidToken_ShouldReturnCurrentUser()
    {
        // Arrange
        var email =
            $"me-{Guid.NewGuid()}@example.com";

        var password = "Test@12345";

        var registerRequest = new RegisterRequest
        {
            OrganizationId = TestDataSeeder.OrganizationId,
            FirstName = "Me",
            LastName = "Test",
            Email = email,
            Password = password,
            Phone = "9876543210",
            RoleCode = "VIEWER"
        };

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                registerRequest);

        registerResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Login
        var loginRequest = new
        {
            OrganizationId = TestDataSeeder.OrganizationId,
            Email = email,
            Password = password
        };

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        loginResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        loginResult.Should().NotBeNull();

        loginResult!.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();

        // Add JWT
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                loginResult.AccessToken);

        // Act
        var response =
            await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
    await response.Content
        .ReadFromJsonAsync<JsonElement>();

        result.Should().NotBeNull();

        result.GetProperty("email")
    .GetString()
    .Should()
    .Be(email);

        result.GetProperty("organizationId")
            .GetString()
            .Should()
            .Be(TestDataSeeder.OrganizationId.ToString());

        result.GetProperty("roles")
            .EnumerateArray()
            .Select(x => x.GetString())
            .Should()
            .Contain("VIEWER");
    }

    [Fact]
    public async Task Me_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                "/api/auth/me");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                "this-is-not-a-valid-jwt");

        // Act
        var response =
            await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldGenerateTokenWithCorrectClaims()
    {
        // Arrange
        var email =
            $"claims-{Guid.NewGuid()}@example.com";

        var password = "Test@12345";

        var registerRequest = new RegisterRequest
        {
            OrganizationId = TestDataSeeder.OrganizationId,
            FirstName = "Claims",
            LastName = "Test",
            Email = email,
            Password = password,
            Phone = "9876543210",
            RoleCode = "VIEWER"
        };

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                registerRequest);

        registerResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var registeredUser =
            await registerResponse.Content
                .ReadFromJsonAsync<UserResponse>();

        registeredUser.Should().NotBeNull();

        // Act
        var loginRequest = new
        {
            OrganizationId = TestDataSeeder.OrganizationId,
            Email = email,
            Password = password
        };

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        loginResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        loginResult.Should().NotBeNull();

        var token =
            loginResult!.AccessToken;

        token.Should()
            .NotBeNullOrWhiteSpace();

        // Add token to client
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        // Act - /me reads the JWT claims
        var meResponse =
            await _client.GetAsync("/api/auth/me");

        // Assert
        meResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await meResponse.Content
                .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        result.GetProperty("userId")
            .GetString()
            .Should()
            .Be(registeredUser!.Id.ToString());

        result.GetProperty("organizationId")
            .GetString()
            .Should()
            .Be(TestDataSeeder.OrganizationId.ToString());

        result.GetProperty("email")
            .GetString()
            .Should()
            .Be(email);

        result.GetProperty("name")
            .GetString()
            .Should()
            .Be("Claims Test");

        result.GetProperty("roles")
            .EnumerateArray()
            .Select(x => x.GetString())
            .Should()
            .Contain("VIEWER");
    }
}