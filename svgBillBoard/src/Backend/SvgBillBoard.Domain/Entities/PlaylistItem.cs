namespace SvgBillBoard.Domain.Entities;

public class PlaylistItem
{
    public Guid Id { get; set; }

    public Guid PlaylistId { get; set; }

    public Guid MediaId { get; set; }

    public int DisplayOrder { get; set; }

    public int DurationSeconds { get; set; }

    public DateTime CreatedAt { get; set; }

    public Playlist Playlist { get; set; } = null!;

    public Media Media { get; set; } = null!;
}