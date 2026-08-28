using FluentAssertions;
using System.Net;

namespace SvgBillBoard.API.Tests;

public class HealthTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Api_Should_Start()
    {
        var response =
            await _client.GetAsync(
                "/api/health");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }
}