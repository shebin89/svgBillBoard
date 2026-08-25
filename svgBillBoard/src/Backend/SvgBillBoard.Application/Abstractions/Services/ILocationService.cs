using SvgBillBoard.Application.DTOs.Locations;

namespace SvgBillBoard.Application.Abstractions.Services;

public interface ILocationService
{
    Task<LocationResponse> CreateAsync(
        Guid organizationId,
        CreateLocationRequest request);

    Task<List<LocationResponse>> GetAllAsync(
        Guid organizationId);

    Task<LocationResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id);

    Task<LocationResponse?> UpdateAsync(
        Guid organizationId,
        Guid id,
        UpdateLocationRequest request);

    Task<bool> DeleteAsync(
        Guid organizationId,
        Guid id);
}