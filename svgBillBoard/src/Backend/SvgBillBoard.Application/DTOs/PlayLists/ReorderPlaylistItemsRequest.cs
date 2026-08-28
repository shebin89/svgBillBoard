namespace SvgBillBoard.Application.DTOs.Playlists;

public class ReorderPlaylistItemsRequest
{
    public List<PlaylistItemOrderRequest> Items { get; set; }
        = new();
}

public class PlaylistItemOrderRequest
{
    public Guid ItemId { get; set; }

    public int DisplayOrder { get; set; }
}