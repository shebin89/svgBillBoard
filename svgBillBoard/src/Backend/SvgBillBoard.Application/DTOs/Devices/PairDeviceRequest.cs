namespace SvgBillBoard.Application.DTOs.Devices;

public class PairDeviceRequest
{
    public string PairingCode { get; set; } = string.Empty;

    public string DeviceIdentifier { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? DeviceType { get; set; }

    public string? Platform { get; set; }

    public string? AppVersion { get; set; }

    public string? Model { get; set; }

    public string? Manufacturer { get; set; }

    public string? SerialNumber { get; set; }

    public string? MacAddress { get; set; }

    public string? IpAddress { get; set; }
}