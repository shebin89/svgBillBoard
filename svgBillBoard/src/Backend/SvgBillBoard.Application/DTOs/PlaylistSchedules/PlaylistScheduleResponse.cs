namespace SvgBillBoard.Application.DTOs.PlaylistSchedules;

public class PlaylistScheduleResponse
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid PlaylistId { get; set; }

    public string PlaylistName { get; set; } = string.Empty;

    public Guid LocationId { get; set; }

    public string LocationName { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public int DaysOfWeek { get; set; }

    public int Priority { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}