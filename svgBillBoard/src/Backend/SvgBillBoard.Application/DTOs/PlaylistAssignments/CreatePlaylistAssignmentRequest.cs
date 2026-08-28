namespace SvgBillBoard.Application.DTOs.PlaylistAssignments;

public class CreatePlaylistAssignmentRequest
{
    public Guid PlaylistId { get; set; }

    public Guid LocationId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}