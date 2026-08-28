using Microsoft.AspNetCore.Http;

namespace SvgBillBoard.API.Models.Media;

public class UploadMediaRequest
{
    public string Name { get; set; } = string.Empty;

    public IFormFile File { get; set; } = null!;
}