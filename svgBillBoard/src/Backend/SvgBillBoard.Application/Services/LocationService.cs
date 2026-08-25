using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Locations;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;

namespace SvgBillBoard.Application.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _repository;

    public LocationService(
        ILocationRepository repository)
    {
        _repository = repository;
    }

    public async Task<LocationResponse> CreateAsync(
        Guid organizationId,
        CreateLocationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Location name is required.");
        }

        var code = request.Code?.Trim();

        if (!string.IsNullOrWhiteSpace(code))
        {
            var exists =
                await _repository.ExistsByCodeAsync(
                    organizationId,
                    code);

            if (exists)
            {
                throw new InvalidOperationException(
                    $"Location code '{code}' already exists.");
            }
        }

        var location = new Location
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,

            Name = request.Name.Trim(),
            Code = code,

            AddressLine1 =
                request.AddressLine1?.Trim(),

            AddressLine2 =
                request.AddressLine2?.Trim(),

            City = request.City?.Trim(),
            State = request.State?.Trim(),
            PostalCode = request.PostalCode?.Trim(),
            Country = request.Country?.Trim(),

            Latitude = request.Latitude,
            Longitude = request.Longitude,

            Status = 1,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(location);

        await _repository.SaveChangesAsync();

        return Map(location);
    }

    public async Task<List<LocationResponse>> GetAllAsync(
        Guid organizationId)
    {
        var locations =
            await _repository.GetByOrganizationIdAsync(
                organizationId);

        return locations
            .Select(Map)
            .ToList();
    }

    public async Task<LocationResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id)
    {
        var location =
            await _repository.GetByIdAsync(id);

        if (location == null ||
            location.OrganizationId != organizationId)
        {
            return null;
        }

        return Map(location);
    }

    public async Task<LocationResponse?> UpdateAsync(
        Guid organizationId,
        Guid id,
        UpdateLocationRequest request)
    {
        var location =
            await _repository.GetByIdAsync(id);

        if (location == null ||
            location.OrganizationId != organizationId)
        {
            return null;
        }

        var code = request.Code?.Trim();

        if (!string.IsNullOrWhiteSpace(code) &&
            !string.Equals(
                location.Code,
                code,
                StringComparison.OrdinalIgnoreCase))
        {
            var exists =
                await _repository.ExistsByCodeAsync(
                    organizationId,
                    code);

            if (exists)
            {
                throw new InvalidOperationException(
                    $"Location code '{code}' already exists.");
            }
        }

        location.Name = request.Name.Trim();
        location.Code = code;

        location.AddressLine1 =
            request.AddressLine1?.Trim();

        location.AddressLine2 =
            request.AddressLine2?.Trim();

        location.City = request.City?.Trim();
        location.State = request.State?.Trim();
        location.PostalCode =
            request.PostalCode?.Trim();

        location.Country =
            request.Country?.Trim();

        location.Latitude = request.Latitude;
        location.Longitude = request.Longitude;

        location.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(location);

        await _repository.SaveChangesAsync();

        return Map(location);
    }

    public async Task<bool> DeleteAsync(
        Guid organizationId,
        Guid id)
    {
        var location =
            await _repository.GetByIdAsync(id);

        if (location == null ||
            location.OrganizationId != organizationId)
        {
            return false;
        }

        // Soft delete
        location.Status = 0;
        location.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(location);

        await _repository.SaveChangesAsync();

        return true;
    }

    private static LocationResponse Map(
        Location location)
    {
        return new LocationResponse
        {
            Id = location.Id,
            OrganizationId =
                location.OrganizationId,

            Name = location.Name,
            Code = location.Code,

            AddressLine1 =
                location.AddressLine1,

            AddressLine2 =
                location.AddressLine2,

            City = location.City,
            State = location.State,
            PostalCode = location.PostalCode,
            Country = location.Country,

            Latitude = location.Latitude,
            Longitude = location.Longitude,

            Status = location.Status,

            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt
        };
    }
}