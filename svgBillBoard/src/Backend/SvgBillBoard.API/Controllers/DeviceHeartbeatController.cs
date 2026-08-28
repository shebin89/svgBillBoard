using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvgBillBoard.Application.Abstractions.Services;

namespace SvgBillBoard.API.Controllers;

[ApiController]
[Route("api/device")]
[Authorize(Policy = "DeviceOnly")]
public class DeviceHeartbeatController : ControllerBase
{
    private readonly IDeviceHeartbeatService _heartbeatService;

    public DeviceHeartbeatController(
        IDeviceHeartbeatService heartbeatService)
    {
        _heartbeatService = heartbeatService;
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat()
    {
        var deviceIdClaim =
            User.FindFirst("deviceId")?.Value;

        if (!Guid.TryParse(
                deviceIdClaim,
                out var deviceId))
        {
            return Unauthorized(new
            {
                message =
                    "Device information is missing from token."
            });
        }

        try
        {
            await _heartbeatService
                .HeartbeatAsync(deviceId);

            return Ok(new
            {
                message = "Heartbeat received.",
                deviceId,
                serverTime = DateTime.UtcNow
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }
}