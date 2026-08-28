namespace SvgBillBoard.Application.DTOs.Playlists;

public class AddPlaylistItemRequest
{
    public Guid MediaId { get; set; }

    public int DisplayOrder { get; set; }

    public int DurationSeconds { get; set; }
}