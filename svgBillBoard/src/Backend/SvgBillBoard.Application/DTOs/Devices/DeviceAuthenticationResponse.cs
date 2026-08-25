namespace SvgBillBoard.Application.DTOs.Devices;

public class DeviceAuthenticationResponse
{
    public DeviceResponse Device { get; set; } = null!;

    public string DeviceToken { get; set; } = string.Empty;

    public DateTime? ExpiresAt { get; set; }
}