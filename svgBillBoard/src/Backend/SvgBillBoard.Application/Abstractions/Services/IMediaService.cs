using SvgBillBoard.Application.DTOs.Media;

namespace SvgBillBoard.Application.Abstractions.Services;

public interface IMediaService
{
    Task<MediaResponse> CreateAsync(
        Guid organizationId,
        CreateMediaRequest request);

    Task<List<MediaResponse>> GetAllAsync(
        Guid organizationId);

    Task<MediaResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id);

    Task<bool> DeleteAsync(
        Guid organizationId,
        Guid id);
}