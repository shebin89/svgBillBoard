namespace SvgBillBoard.Domain.Entities;

public class DevicePairing
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid LocationId { get; set; }

    public string PairingCode { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public Guid? DeviceId { get; set; }

    public byte Status { get; set; } = 1;

    public DateTime CreatedAt { get; set; }

    public Device? Device { get; set; }
}