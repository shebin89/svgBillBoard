namespace SvgBillBoard.Application.DTOs.Playlists;

public class PlaylistItemResponse
{
    public Guid Id { get; set; }

    public Guid MediaId { get; set; }

    public string MediaName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public int DurationSeconds { get; set; }
}