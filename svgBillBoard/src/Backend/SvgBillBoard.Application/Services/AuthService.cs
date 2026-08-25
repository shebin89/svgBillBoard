using SvgBillBoard.Application.Abstractions.Security;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Authentication;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;

namespace SvgBillBoard.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<UserResponse> RegisterAsync(
        RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var exists = await _userRepository
            .ExistsByEmailAsync(
                request.OrganizationId,
                email);

        if (exists)
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var role = await _roleRepository.GetByCodeAsync(
            request.OrganizationId,
            request.RoleCode);

        if (role == null)
        {
            throw new InvalidOperationException(
                $"Role '{request.RoleCode}' was not found.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash =
                _passwordHasher.Hash(request.Password),
            Phone = request.Phone,
            Status = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow
        });

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return new UserResponse
        {
            Id = user.Id,
            OrganizationId = user.OrganizationId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Roles = [role.Code]
        };
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request)
    {
        var email = request.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(
            request.OrganizationId,
            email);

        if (user == null ||
            user.Status != 1 ||
            !_passwordHasher.Verify(
                request.Password,
                user.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var roles = user.UserRoles
            .Where(x => x.Role != null)
            .Select(x => x.Role!.Code)
            .ToList();

        var token = _jwtService.GenerateAccessToken(
            user,
            roles);

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync();

        return new LoginResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAt = token.ExpiresAt,
            User = new UserResponse
            {
                Id = user.Id,
                OrganizationId = user.OrganizationId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Roles = roles
            }
        };
    }
}