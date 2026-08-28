using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvgBillBoard.API.Models.Media;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Media;
using System.Security.Claims;

namespace SvgBillBoard.API.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(
        IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<MediaResponse>> Create(
    [FromForm] UploadMediaRequest request)
    {
        if (request.File == null ||
            request.File.Length == 0)
        {
            return BadRequest(new
            {
                message = "Media file is required."
            });
        }

        var organizationIdValue =
            User.FindFirstValue("organizationId");

        if (!Guid.TryParse(
                organizationIdValue,
                out var organizationId))
        {
            return Unauthorized(new
            {
                message =
                    "Organization information is missing."
            });
        }

        try
        {
            await using var stream =
                request.File.OpenReadStream();

            var applicationRequest =
                new CreateMediaRequest
                {
                    Name = request.Name,

                    FileName =
                        request.File.FileName,

                    ContentType =
                        request.File.ContentType,

                    FileSize =
                        request.File.Length,

                    FileStream = stream
                };

            var result =
                await _mediaService.CreateAsync(
                    organizationId,
                    applicationRequest);

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
    public async Task<ActionResult<List<MediaResponse>>>
        GetAll()
    {
        var organizationIdValue =
            User.FindFirstValue("organizationId");

        if (!Guid.TryParse(
                organizationIdValue,
                out var organizationId))
        {
            return Unauthorized();
        }

        var result =
            await _mediaService.GetAllAsync(
                organizationId);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MediaResponse>>
        GetById(Guid id)
    {
        var organizationIdValue =
            User.FindFirstValue("organizationId");

        if (!Guid.TryParse(
                organizationIdValue,
                out var organizationId))
        {
            return Unauthorized();
        }

        var result =
            await _mediaService.GetByIdAsync(
                organizationId,
                id);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Media was not found."
            });
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var organizationIdValue =
            User.FindFirstValue("organizationId");

        if (!Guid.TryParse(
                organizationIdValue,
                out var organizationId))
        {
            return Unauthorized();
        }

        var deleted =
            await _mediaService.DeleteAsync(
                organizationId,
                id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Media was not found."
            });
        }

        return NoContent();
    }
}