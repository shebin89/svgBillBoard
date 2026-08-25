namespace SvgBillBoard.Application.DTOs.Devices;

public class DeviceResponse
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid LocationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string DeviceIdentifier { get; set; } = string.Empty;

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

    public byte Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}