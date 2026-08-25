namespace SvgBillBoard.Domain.Entities;

public class DeviceCredential
{
    public Guid Id { get; set; }

    public Guid DeviceId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public Device? Device { get; set; }
}