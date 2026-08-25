namespace SvgBillBoard.Application.DTOs.Authentication;

public class LoginRequest
{
    public Guid OrganizationId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}