using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Playlists;
using System.Security.Claims;

namespace SvgBillBoard.API.Controllers;

[ApiController]
[Route("api/playlists")]
[Authorize]
public class PlaylistsController : ControllerBase
{
    private readonly IPlaylistService _playlistService;

    public PlaylistsController(
        IPlaylistService playlistService)
    {
        _playlistService = playlistService;
    }

    [HttpPost]
    public async Task<ActionResult<PlaylistResponse>> Create(
        CreatePlaylistRequest request)
    {
        var organizationId = GetOrganizationId();

        try
        {
            var result =
                await _playlistService.CreateAsync(
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
    }

    [HttpGet]
    public async Task<ActionResult<List<PlaylistResponse>>>
        GetAll()
    {
        var organizationId = GetOrganizationId();

        var result =
            await _playlistService.GetAllAsync(
                organizationId);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlaylistResponse>>
        GetById(Guid id)
    {
        var organizationId = GetOrganizationId();

        var result =
            await _playlistService.GetByIdAsync(
                organizationId,
                id);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Playlist was not found."
            });
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var organizationId = GetOrganizationId();

        var deleted =
            await _playlistService.DeleteAsync(
                organizationId,
                id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Playlist was not found."
            });
        }

        return NoContent();
    }

    [HttpPost("{playlistId:guid}/items")]
    public async Task<ActionResult<PlaylistItemResponse>>
        AddItem(
            Guid playlistId,
            AddPlaylistItemRequest request)
    {
        var organizationId = GetOrganizationId();

        try
        {
            var result =
                await _playlistService.AddItemAsync(
                    organizationId,
                    playlistId,
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

    [HttpDelete(
        "{playlistId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(
        Guid playlistId,
        Guid itemId)
    {
        var organizationId = GetOrganizationId();

        var deleted =
            await _playlistService.DeleteItemAsync(
                organizationId,
                playlistId,
                itemId);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Playlist item was not found."
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
    [HttpPut("{playlistId:guid}/items/reorder")]
    public async Task<IActionResult> ReorderItems(
    Guid playlistId,
    ReorderPlaylistItemsRequest request)
    {
        var organizationId = GetOrganizationId();

        try
        {
            var result =
                await _playlistService.ReorderItemsAsync(
                    organizationId,
                    playlistId,
                    request);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Playlist was not found."
                });
            }

            return NoContent();
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
}