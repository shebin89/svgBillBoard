namespace SvgBillBoard.Application.DTOs.PlaylistSchedules;

public class CreatePlaylistScheduleRequest
{
    public Guid PlaylistId { get; set; }

    public Guid LocationId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    // 1 = Sunday, 2 = Monday, 4 = Tuesday, etc.
    // Combine values for multiple days.
    public int DaysOfWeek { get; set; }

    public int Priority { get; set; } = 1;
}