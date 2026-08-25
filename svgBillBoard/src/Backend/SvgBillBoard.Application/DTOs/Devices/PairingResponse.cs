namespace SvgBillBoard.Application.DTOs.Devices;

public class PairingResponse
{
    public string PairingCode { get; set; } = string.Empty;

    public Guid LocationId { get; set; }

    public DateTime ExpiresAt { get; set; }
}