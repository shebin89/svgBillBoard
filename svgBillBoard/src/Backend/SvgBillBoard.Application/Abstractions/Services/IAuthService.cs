using SvgBillBoard.Application.DTOs.Authentication;

namespace SvgBillBoard.Application.Abstractions.Services;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(
        RegisterRequest request);

    Task<LoginResponse> LoginAsync(
        LoginRequest request);
}