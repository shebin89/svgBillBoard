public class DevicePlayback
{
    public Guid DeviceId { get; set; }

    public Guid LocationId { get; set; }

    public string DeviceName { get; set; } = string.Empty;

    public int PlaybackVersion { get; set; }

    public Guid? PlaylistId { get; set; }

    public string? PlaylistName { get; set; }

    public List<DevicePlaybackItem> Items { get; set; }
        = new();
}

public class DevicePlaybackItem
{
    public Guid Id { get; set; }

    public Guid MediaId { get; set; }

    public string MediaName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public int DurationSeconds { get; set; }
}