using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Domain.Entities;

namespace SvgBillBoard.Infrastructure.Authentication;

public class DeviceJwtService : IDeviceJwtService
{
    private readonly IConfiguration _configuration;

    public DeviceJwtService(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Device device)
    {
        var jwtSettings =
            _configuration.GetSection("Jwt");

        var secret =
            jwtSettings["Secret"]
            ?? throw new InvalidOperationException(
                "JWT Secret is not configured.");

        var issuer =
            jwtSettings["Issuer"]
            ?? throw new InvalidOperationException(
                "JWT Issuer is not configured.");

        var audience =
            jwtSettings["Audience"]
            ?? throw new InvalidOperationException(
                "JWT Audience is not configured.");

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                device.Id.ToString()),

            new(
                "deviceId",
                device.Id.ToString()),

            new(
                "organizationId",
                device.OrganizationId.ToString()),

            new(
                "locationId",
                device.LocationId.ToString()),

            new(
                "deviceCode",
                device.DeviceCode),

            new(
                "deviceIdentifier",
                device.DeviceIdentifier),

            new(
                "device",
                "true")
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secret));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var expires =
            DateTime.UtcNow.AddDays(30);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}