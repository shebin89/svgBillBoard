namespace SvgBillBoard.Application.DTOs.Playlists;

public class CreatePlaylistRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}