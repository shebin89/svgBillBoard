using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Devices;

namespace SvgBillBoard.API.Controllers;

[ApiController]
[Route("api/devices")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly IDeviceHeartbeatService _heartbeatService;

    public DevicesController(
        IDeviceService deviceService, IDeviceHeartbeatService heartbeatService)
    {
        _deviceService = deviceService;
        _heartbeatService = heartbeatService;
    }

    [HttpPost("pairing")]
    public async Task<ActionResult<PairingResponse>>
        GeneratePairing(
            GeneratePairingRequest request)
    {
        var organizationId =
            GetOrganizationId();

        try
        {
            var result =
                await _deviceService
                    .GeneratePairingAsync(
                        organizationId,
                        request);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [AllowAnonymous]
    [HttpPost("pair")]
    public async Task<ActionResult<DeviceResponse>>
        PairDevice([FromBody]
            PairDeviceRequest request)
    {
        try
        {
            var result =
                await _deviceService
                    .PairDeviceAsync(request);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<
        List<DeviceResponse>>> GetAll()
    {
        var organizationId =
            GetOrganizationId();

        var result =
            await _deviceService
                .GetAllAsync(organizationId);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeviceResponse>>
        GetById(Guid id)
    {
        var organizationId =
            GetOrganizationId();

        var result =
            await _deviceService
                .GetByIdAsync(
                    organizationId,
                    id);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    private Guid GetOrganizationId()
    {
        var value =
            User.FindFirstValue(
                "organizationId");

        if (!Guid.TryParse(
                value,
                out var organizationId))
        {
            throw new UnauthorizedAccessException(
                "Organization information is missing from the token.");
        }

        return organizationId;
    }

    [Authorize(Policy = "DeviceOnly")]
    [HttpPost("heartbeat")]
    public async Task<ActionResult<DeviceHeartbeatResponse>>
    Heartbeat()
    {
        var deviceIdValue =
            User.FindFirst("deviceId")?.Value;

        if (!Guid.TryParse(
                deviceIdValue,
                out var deviceId))
        {
            return Unauthorized(new
            {
                message = "Device information is missing from token."
            });
        }

        var result =
            await _heartbeatService
                .HeartbeatAsync(deviceId);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Device not found."
            });
        }

        return Ok(result);
    }
}