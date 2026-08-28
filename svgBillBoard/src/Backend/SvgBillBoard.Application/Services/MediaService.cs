using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Media;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;

namespace SvgBillBoard.Application.Services;

public class MediaService : IMediaService
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IFileStorageService _fileStorageService;

    private static readonly string[] AllowedContentTypes =
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/svg+xml",
        "video/mp4"
    };

    private const long MaxFileSize =
        500 * 1024 * 1024; // 500 MB

    public MediaService(
        IMediaRepository mediaRepository,
        IFileStorageService fileStorageService)
    {
        _mediaRepository = mediaRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<MediaResponse> CreateAsync(
        Guid organizationId,
        CreateMediaRequest request)
    {
        if (request.FileStream == null)
        {
            throw new ArgumentException(
                "Media file is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Media name is required.");
        }

        var contentType =
            request.ContentType.ToLowerInvariant();

        if (!AllowedContentTypes.Contains(contentType))
        {
            throw new ArgumentException(
                "Unsupported media type.");
        }

        if (request.FileSize <= 0)
        {
            throw new ArgumentException(
                "Media file is empty.");
        }

        if (request.FileSize > MaxFileSize)
        {
            throw new ArgumentException(
                "File size cannot exceed 500 MB.");
        }

        var mediaId = Guid.NewGuid();

        var extension =
            Path.GetExtension(request.FileName);

        var storedFileName =
            $"{mediaId}{extension}";

        var fileUrl =
            await _fileStorageService.SaveAsync(
                request.FileStream,
                storedFileName,
                contentType);

        var media = new Media
        {
            Id = mediaId,

            OrganizationId = organizationId,

            Name = request.Name.Trim(),

            FileName = request.FileName,

            FileUrl = fileUrl,

            ContentType = request.ContentType,

            FileSize = request.FileSize,

            MediaType =
                GetMediaType(contentType),

            Status = 1,

            CreatedAt = DateTime.UtcNow,

            UpdatedAt = DateTime.UtcNow
        };

        await _mediaRepository.AddAsync(media);

        await _mediaRepository.SaveChangesAsync();

        return Map(media);
    }

    public async Task<List<MediaResponse>> GetAllAsync(
        Guid organizationId)
    {
        var media =
            await _mediaRepository
                .GetByOrganizationIdAsync(
                    organizationId);

        return media
            .Select(Map)
            .ToList();
    }

    public async Task<MediaResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id)
    {
        var media =
            await _mediaRepository
                .GetByIdAsync(id);

        if (media == null ||
            media.OrganizationId != organizationId)
        {
            return null;
        }

        return Map(media);
    }

    public async Task<bool> DeleteAsync(
        Guid organizationId,
        Guid id)
    {
        var media =
            await _mediaRepository
                .GetByIdAsync(id);

        if (media == null ||
            media.OrganizationId != organizationId)
        {
            return false;
        }

        await _fileStorageService.DeleteAsync(
            media.FileUrl);

        await _mediaRepository.DeleteAsync(media);

        await _mediaRepository.SaveChangesAsync();

        return true;
    }

    private static int GetMediaType(
        string contentType)
    {
        return contentType switch
        {
            "image/jpeg" => 1,
            "image/png" => 1,
            "image/webp" => 1,
            "image/svg+xml" => 1,
            "video/mp4" => 2,
            _ => 0
        };
    }

    private static MediaResponse Map(
        Media media)
    {
        return new MediaResponse
        {
            Id = media.Id,

            OrganizationId =
                media.OrganizationId,

            Name =
                media.Name,

            FileName =
                media.FileName,

            FileUrl =
                media.FileUrl,

            ContentType =
                media.ContentType,

            FileSize =
                media.FileSize,

            MediaType =
                media.MediaType,

            Status =
                media.Status,

            CreatedAt =
                media.CreatedAt,

            UpdatedAt =
                media.UpdatedAt
        };
    }
}