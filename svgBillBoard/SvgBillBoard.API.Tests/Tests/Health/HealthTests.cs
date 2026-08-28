using System.Net;

using FluentAssertions;

using SvgBillBoard.API.Tests.Infrastructure;

namespace SvgBillBoard.API.Tests.Tests.Health;

public class HealthTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }


    // =========================================================
    // HEALTH CHECK
    // =========================================================

    [Fact]
    public async Task GetHealth_ShouldReturnHealthy()
    {
        // Act
        var response =
            await _client.GetAsync(
                "/api/health");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var content =
            await response.Content.ReadAsStringAsync();

        content
            .Should()
            .Contain("\"status\":\"Healthy\"");
    }


    // =========================================================
    // HEALTH CHECK - WITHOUT AUTHENTICATION
    // =========================================================

    [Fact]
    public async Task GetHealth_WithoutToken_ShouldReturnOk()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync(
                "/api/health");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }
}