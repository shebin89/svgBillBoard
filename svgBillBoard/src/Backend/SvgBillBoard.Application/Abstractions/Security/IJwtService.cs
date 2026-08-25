using SvgBillBoard.Domain.Entities;

namespace SvgBillBoard.Application.Abstractions.Security;

public interface IJwtService
{
    JwtTokenResult GenerateAccessToken(
        User user,
        IEnumerable<string> roles);
}

public sealed class JwtTokenResult
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}