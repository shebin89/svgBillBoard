namespace SvgBillBoard.Application.DTOs.PlaylistAssignments;

public class PlaylistAssignmentResponse
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid PlaylistId { get; set; }

    public string PlaylistName { get; set; } = string.Empty;

    public Guid LocationId { get; set; }

    public string LocationName { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}