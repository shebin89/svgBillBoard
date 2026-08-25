using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Devices;

namespace SvgBillBoard.API.Controllers;

[ApiController]
[Route("api/device-auth")]
public class DeviceAuthenticationController
    : ControllerBase
{
    private readonly IDeviceAuthenticationService
        _authenticationService;

    public DeviceAuthenticationController(
        IDeviceAuthenticationService authenticationService)
    {
        _authenticationService =
            authenticationService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<DeviceLoginResponse>>
        Login(DeviceLoginRequest request)
    {
        try
        {
            var result =
                await _authenticationService
                    .LoginAsync(request);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
    }
}