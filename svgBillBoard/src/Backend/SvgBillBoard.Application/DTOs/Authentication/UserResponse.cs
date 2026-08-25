namespace SvgBillBoard.Application.DTOs.Authentication;

public class UserResponse
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public List<string> Roles { get; set; } = [];
}