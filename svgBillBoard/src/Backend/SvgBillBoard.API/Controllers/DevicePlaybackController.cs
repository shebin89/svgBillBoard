using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Devices;
using System.Security.Claims;

namespace SvgBillBoard.API.Controllers;

[ApiController]
[Route("api/device")]
[Authorize(Policy = "DeviceOnly")]
public class DevicePlaybackController : ControllerBase
{
    private readonly IDevicePlaybackService _playbackService;

    public DevicePlaybackController(
        IDevicePlaybackService playbackService)
    {
        _playbackService = playbackService;
    }

    [HttpGet("playback")]
    public async Task<ActionResult<DevicePlaybackResponse>>GetPlayback([FromQuery] int? version)
    {
        var deviceIdValue =
            User.FindFirst("deviceId")?.Value;

        if (!Guid.TryParse(
                deviceIdValue,
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
            var result =
                await _playbackService
                    .GetPlaybackAsync(deviceId);

            if (version.HasValue &&
                version.Value == result.PlaybackVersion)
            {
                return StatusCode(
                    StatusCodes.Status304NotModified);
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("playback/sync")]
    public async Task<ActionResult<DevicePlaybackResponse>>
    SyncPlayback([FromQuery] int? version)
    {
        var deviceIdValue =
            User.FindFirst("deviceId")?.Value;

        if (!Guid.TryParse(
                deviceIdValue,
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
            var result =
                await _playbackService
                    .GetPlaybackAsync(deviceId);

            if (version.HasValue &&
                version.Value == result.PlaybackVersion)
            {
                return StatusCode(
                    StatusCodes.Status304NotModified);
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }
}