namespace SvgBillBoard.Application.DTOs.Devices;

public class DeviceStatusChangedResponse
{
    public Guid DeviceId { get; set; }

    public Guid OrganizationId { get; set; }

    public string DeviceCode { get; set; } = string.Empty;

    public string DeviceName { get; set; } = string.Empty;

    public bool IsOnline { get; set; }

    public DateTime ChangedAt { get; set; }
}