using SvgBillBoard.Application.Abstractions.Services;

namespace SvgBillBoard.Infrastructure.Storage;

public class LocalFileStorageService
    : IFileStorageService
{
    private readonly string _uploadDirectory;

    public LocalFileStorageService()
    {
        _uploadDirectory =
            Path.Combine(
                AppContext.BaseDirectory,
                "uploads",
                "media");

        Directory.CreateDirectory(
            _uploadDirectory);
    }

    public async Task<string> SaveAsync(
        Stream fileStream,
        string fileName,
        string contentType)
    {
        var filePath =
            Path.Combine(
                _uploadDirectory,
                fileName);

        await using var output =
            new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write);

        await fileStream.CopyToAsync(output);

        return $"/uploads/media/{fileName}";
    }

    public Task DeleteAsync(string fileUrl)
    {
        var fileName =
            Path.GetFileName(fileUrl);

        var filePath =
            Path.Combine(
                _uploadDirectory,
                fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}