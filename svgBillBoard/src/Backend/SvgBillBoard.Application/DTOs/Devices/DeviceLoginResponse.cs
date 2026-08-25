namespace SvgBillBoard.Application.DTOs.Devices;

public class DeviceLoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DeviceResponse Device { get; set; } = null!;
}