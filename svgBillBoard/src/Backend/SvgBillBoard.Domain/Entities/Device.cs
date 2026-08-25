namespace SvgBillBoard.Domain.Entities;

public class Device
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid LocationId { get; set; }

    public string Name { get; set; } = string.Empty;

    // Unique identifier generated/read by the Android TV app
    public string DeviceIdentifier { get; set; } = string.Empty;

    // Human-friendly code shown in dashboard
    public string DeviceCode { get; set; } = string.Empty;

    public string? DeviceType { get; set; }

    public string? Platform { get; set; }

    public string? AppVersion { get; set; }

    public string? Model { get; set; }

    public string? Manufacturer { get; set; }

    public string? SerialNumber { get; set; }

    public string? MacAddress { get; set; }

    public string? IpAddress { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public byte Status { get; set; } = 1;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Organization? Organization { get; set; }

    public Location? Location { get; set; }

    public DateTime? LastHeartbeatAt { get; set; }

    public bool IsOnline { get; set; }

    public DateTime? LastOnlineAt { get; set; }
}