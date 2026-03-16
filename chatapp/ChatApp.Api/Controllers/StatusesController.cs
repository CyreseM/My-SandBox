using System.Security.Claims;
using ChatApp.Api.DTOs;
using ChatApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers;

[ApiController, Route("api/statuses"), Authorize]
public class StatusesController : ControllerBase
{
    private readonly StatusService _statuses;
    public StatusesController(StatusService s) => _statuses = s;

    [HttpGet]
    public async Task<IActionResult> GetContactStatuses() =>
        Ok(await _statuses.GetContactStatusesAsync(GetUserId()));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        var s = await _statuses.GetByIdAsync(id);
        if (s == null) return NotFound();
        return Ok(StatusDto.From(s, GetUserId()));
    }

    [HttpPost]
    public async Task<IActionResult> PostStatus([FromBody] CreateStatusDto dto)
    {
        var status = await _statuses.CreateAsync(dto, GetUserId());
        return Ok(StatusDto.From(status, GetUserId()));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _statuses.DeleteAsync(id, GetUserId()); return NoContent();
    }

    [HttpPost("{id}/view")]
    public async Task<IActionResult> MarkViewed(Guid id)
    {
        await _statuses.RecordViewAsync(id, GetUserId()); return NoContent();
    }

    [HttpGet("{id}/views")]
    public async Task<IActionResult> GetViewers(Guid id) =>
        Ok(await _statuses.GetViewersAsync(id, GetUserId()));

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
