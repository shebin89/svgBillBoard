namespace SvgBillBoard.Domain.Entities;

public class PlaylistAssignment
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid PlaylistId { get; set; }

    public Guid LocationId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Playlist Playlist { get; set; } = null!;

    public Location Location { get; set; } = null!;
}