namespace SvgBillBoard.Application.DTOs.Devices;

public class DeviceHeartbeatResponse
{
    public Guid DeviceId { get; set; }

    public bool IsOnline { get; set; }

    public DateTime ServerTime { get; set; }
}