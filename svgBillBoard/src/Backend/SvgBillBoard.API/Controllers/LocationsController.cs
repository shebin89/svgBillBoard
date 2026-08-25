using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Locations;

namespace SvgBillBoard.API.Controllers;

[ApiController]
[Route("api/locations")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(
        ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpPost]
    public async Task<ActionResult<LocationResponse>> Create(
        CreateLocationRequest request)
    {
        var organizationId = GetOrganizationId();

        try
        {
            var result =
                await _locationService.CreateAsync(
                    organizationId,
                    request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
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
        List<LocationResponse>>> GetAll()
    {
        var organizationId = GetOrganizationId();

        var result =
            await _locationService.GetAllAsync(
                organizationId);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LocationResponse>> GetById(
        Guid id)
    {
        var organizationId = GetOrganizationId();

        var result =
            await _locationService.GetByIdAsync(
                organizationId,
                id);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LocationResponse>> Update(
        Guid id,
        UpdateLocationRequest request)
    {
        var organizationId = GetOrganizationId();

        try
        {
            var result =
                await _locationService.UpdateAsync(
                    organizationId,
                    id,
                    request);

            if (result == null)
            {
                return NotFound();
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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var organizationId = GetOrganizationId();

        var deleted =
            await _locationService.DeleteAsync(
                organizationId,
                id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private Guid GetOrganizationId()
    {
        var value =
            User.FindFirstValue("organizationId");

        if (!Guid.TryParse(value, out var organizationId))
        {
            throw new UnauthorizedAccessException(
                "Organization information is missing from the token.");
        }

        return organizationId;
    }
}