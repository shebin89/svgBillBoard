using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.PlaylistSchedules;
using System.Security.Claims;

namespace SvgBillBoard.API.Controllers;

[ApiController]
[Route("api/playlist-schedules")]
[Authorize]
public class PlaylistSchedulesController : ControllerBase
{
    private readonly IPlaylistScheduleService _scheduleService;

    public PlaylistSchedulesController(
        IPlaylistScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpPost]
    public async Task<ActionResult<PlaylistScheduleResponse>>
        Create(
            CreatePlaylistScheduleRequest request)
    {
        var organizationId = GetOrganizationId();

        try
        {
            var result =
                await _scheduleService.CreateAsync(
                    organizationId,
                    request);

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
    public async Task<
        ActionResult<List<PlaylistScheduleResponse>>>
        GetAll()
    {
        var organizationId = GetOrganizationId();

        var result =
            await _scheduleService.GetAllAsync(
                organizationId);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<
        ActionResult<PlaylistScheduleResponse>>
        GetById(Guid id)
    {
        var organizationId = GetOrganizationId();

        var result =
            await _scheduleService.GetByIdAsync(
                organizationId,
                id);

        if (result == null)
        {
            return NotFound(new
            {
                message =
                    "Playlist schedule was not found."
            });
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var organizationId = GetOrganizationId();

        var deleted =
            await _scheduleService.DeleteAsync(
                organizationId,
                id);

        if (!deleted)
        {
            return NotFound(new
            {
                message =
                    "Playlist schedule was not found."
            });
        }

        return NoContent();
    }

    private Guid GetOrganizationId()
    {
        var value =
            User.FindFirstValue("organizationId");

        if (!Guid.TryParse(
                value,
                out var organizationId))
        {
            throw new UnauthorizedAccessException(
                "Organization information is missing from token.");
        }

        return organizationId;
    }
}