using SvgBillBoard.Domain.Entities;

namespace SvgBillBoard.Application.Abstractions.Services;

public interface IDeviceJwtService
{
    string GenerateToken(Device device);
}