namespace SvgBillBoard.Application.DTOs.Media;

public class CreateMediaRequest
{
    public string Name { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public Stream FileStream { get; set; } = null!;
}