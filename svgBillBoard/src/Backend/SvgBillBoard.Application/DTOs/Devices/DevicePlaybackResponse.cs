namespace SvgBillBoard.Application.DTOs.Devices;

public class DevicePlaybackResponse
{
    public Guid DeviceId { get; set; }

    public Guid LocationId { get; set; }

    public string DeviceName { get; set; } = string.Empty;

    public Guid? PlaylistId { get; set; }

    public string? PlaylistName { get; set; }

    public List<DevicePlaybackItemResponse> Items { get; set; }
        = new();

    public int PlaybackVersion { get; set; }
}

public class DevicePlaybackItemResponse
{
    public Guid Id { get; set; }

    public Guid MediaId { get; set; }

    public string MediaName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public int DurationSeconds { get; set; }
}