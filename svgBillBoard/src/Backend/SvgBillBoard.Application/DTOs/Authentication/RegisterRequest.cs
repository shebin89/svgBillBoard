namespace SvgBillBoard.Application.DTOs.Authentication;

public class RegisterRequest
{
    public Guid OrganizationId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string RoleCode { get; set; } = "VIEWER";
}