using Altensorcrm.Contract.DTOs.Note;
using Altensorcrm.Contract.Services.Note;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altensorcrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;

    public NotesController(INoteService noteService)
    {
        _noteService = noteService;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewNotes")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _noteService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanViewNotes")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _noteService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "CanCreateNotes")]
    public async Task<IActionResult> Create([FromBody] CreateNoteDto dto, CancellationToken cancellationToken)
    {
        var result = await _noteService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanUpdateNotes")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNoteDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest("Route ID and DTO ID do not match.");
        }

        var result = await _noteService.UpdateAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanDeleteNotes")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _noteService.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }
}
