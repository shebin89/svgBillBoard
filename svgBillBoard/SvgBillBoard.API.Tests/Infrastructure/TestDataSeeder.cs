using Microsoft.EntityFrameworkCore;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Infrastructure.Persistence;

namespace SvgBillBoard.API.Tests.Infrastructure;

public static class TestDataSeeder
{
    // Organization A
    public static readonly Guid OrganizationId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    // Organization A - VIEWER role
    public static readonly Guid ViewerRoleId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    // Organization B
    public static readonly Guid SecondOrganizationId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");

    // Organization B - VIEWER role
    public static readonly Guid SecondViewerRoleId =
        Guid.Parse("44444444-4444-4444-4444-444444444444");

    // Organization B - Location
    public static readonly Guid SecondOrganizationLocationId =
        Guid.Parse("55555555-5555-5555-5555-555555555555");

    public static readonly Guid OrganizationLocationId =
    Guid.Parse("66666666-6666-6666-6666-666666666666");

    public static readonly Guid SecondOrganizationDeviceId =
    Guid.Parse("77777777-7777-7777-7777-777777777777");

    public static async Task SeedAsync(
        ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();


        // =========================================================
        // ORGANIZATION A
        // =========================================================

        var organization =
            await context.Organizations
                .FirstOrDefaultAsync(
                    x => x.Id == OrganizationId);

        if (organization == null)
        {
            organization = new Organization
            {
                Id = OrganizationId,

                Name = "Test Organization",

                Code = "TEST_ORG",

                Email = "test@example.com",

                Phone = "9876543210",

                Status = 1,

                CreatedAt = DateTime.UtcNow,

                UpdatedAt = DateTime.UtcNow
            };

            context.Organizations.Add(
                organization);
        }


        // =========================================================
        // ORGANIZATION A - VIEWER ROLE
        // =========================================================

        var viewerRole =
            await context.Roles
                .FirstOrDefaultAsync(
                    x => x.Id == ViewerRoleId);

        if (viewerRole == null)
        {
            viewerRole = new Role
            {
                Id = ViewerRoleId,

                OrganizationId =
                    OrganizationId,

                Name = "Viewer",

                Code = "VIEWER",

                Description =
                    "Test viewer role",

                Status = 1,

                CreatedAt = DateTime.UtcNow,

                UpdatedAt = DateTime.UtcNow
            };

            context.Roles.Add(
                viewerRole);
        }


        // =========================================================
        // ORGANIZATION B
        // =========================================================

        var secondOrganization =
            await context.Organizations
                .FirstOrDefaultAsync(
                    x => x.Id == SecondOrganizationId);

        if (secondOrganization == null)
        {
            secondOrganization = new Organization
            {
                Id = SecondOrganizationId,

                Name =
                    "Second Test Organization",

                Code =
                    "TEST_ORG_2",

                Email =
                    "test2@example.com",

                Phone =
                    "9876543211",

                Status = 1,

                CreatedAt =
                    DateTime.UtcNow,

                UpdatedAt =
                    DateTime.UtcNow
            };

            context.Organizations.Add(
                secondOrganization);
        }


        // =========================================================
        // ORGANIZATION B - VIEWER ROLE
        // =========================================================

        var secondViewerRole =
            await context.Roles
                .FirstOrDefaultAsync(
                    x => x.Id == SecondViewerRoleId);

        if (secondViewerRole == null)
        {
            secondViewerRole = new Role
            {
                Id = SecondViewerRoleId,

                OrganizationId =
                    SecondOrganizationId,

                Name =
                    "Viewer",

                Code =
                    "VIEWER",

                Description =
                    "Second test viewer role",

                Status = 1,

                CreatedAt =
                    DateTime.UtcNow,

                UpdatedAt =
                    DateTime.UtcNow
            };

            context.Roles.Add(
                secondViewerRole);
        }


        // =========================================================
        // ORGANIZATION B - LOCATION
        // =========================================================

        var secondLocation =
            await context.Locations
                .FirstOrDefaultAsync(
                    x =>
                        x.Id ==
                        SecondOrganizationLocationId);

        if (secondLocation == null)
        {
            secondLocation = new Location
            {
                Id =
                    SecondOrganizationLocationId,

                OrganizationId =
                    SecondOrganizationId,

                Name =
                    "Second Organization Location",

                Code =
                    "LOC_ORG_2",

                AddressLine1 =
                    "456 Test Street",

                City =
                    "Kochi",

                State =
                    "Kerala",

                PostalCode =
                    "682002",

                Country =
                    "India",

                Status = 1,

                CreatedAt =
                    DateTime.UtcNow,

                UpdatedAt =
                    DateTime.UtcNow
            };

            context.Locations.Add(
                secondLocation);
        }

        var organizationLocation =
    await context.Locations
        .FirstOrDefaultAsync(
            x => x.Id == OrganizationLocationId);

        if (organizationLocation == null)
        {
            organizationLocation = new Location
            {
                Id =
                    OrganizationLocationId,

                OrganizationId =
                    OrganizationId,

                Name =
                    "Test Organization Location",

                Code =
                    "LOC_ORG_1",

                AddressLine1 =
                    "123 Test Street",

                City =
                    "Kochi",

                State =
                    "Kerala",

                PostalCode =
                    "682001",

                Country =
                    "India",

                Status = 1,

                CreatedAt =
                    DateTime.UtcNow,

                UpdatedAt =
                    DateTime.UtcNow
            };

            context.Locations.Add(
                organizationLocation);
        }

        // =========================================================
        // ORGANIZATION B - DEVICE
        // =========================================================

        var secondDevice =
            await context.Devices
                .FirstOrDefaultAsync(
                    x => x.Id == SecondOrganizationDeviceId);

        if (secondDevice == null)
        {
            secondDevice = new Device
            {
                Id =
                    SecondOrganizationDeviceId,

                OrganizationId =
                    SecondOrganizationId,

                LocationId =
                    SecondOrganizationLocationId,

                Name =
                    "Second Organization Device",

                DeviceIdentifier =
                    "TEST-DEVICE-ORG-2",

                DeviceCode =
                    "TV-ORG2",

                DeviceType =
                    "Billboard",

                Platform =
                    "Windows",

                AppVersion =
                    "1.0.0",

                Status = 1,

                CreatedAt =
                    DateTime.UtcNow,

                UpdatedAt =
                    DateTime.UtcNow
            };

            context.Devices.Add(
                secondDevice);
        }
        // =========================================================
        // SAVE
        // =========================================================
        await context.SaveChangesAsync();
    }
}