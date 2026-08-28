using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SvgBillBoard.API.Tests.Infrastructure;
using SvgBillBoard.Infrastructure.Persistence;

namespace SvgBillBoard.API.Tests;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();

            logging.AddConsole();

            logging.SetMinimumLevel(
                LogLevel.Debug);
        });

        builder.ConfigureServices(services =>
        {
            var serviceProvider =
                services.BuildServiceProvider();

            using var scope =
                serviceProvider.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

            TestDataSeeder
                .SeedAsync(context)
                .GetAwaiter()
                .GetResult();
        });
    }
}