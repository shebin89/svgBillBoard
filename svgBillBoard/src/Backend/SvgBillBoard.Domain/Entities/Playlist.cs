namespace SvgBillBoard.Domain.Entities;

public class Playlist
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<PlaylistItem> Items { get; set; }
        = new List<PlaylistItem>();
}