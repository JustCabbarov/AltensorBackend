using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Contract.DTOs.Lead;
using Altensorcrm.Contract.Services.Lead;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altensorcrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;

    public LeadsController(ILeadService leadService)
    {
        _leadService = leadService;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewLeads")]
    public async Task<IActionResult> GetPagedList([FromQuery] LeadFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _leadService.GetPagedListAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanViewLeads")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _leadService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "CanCreateLeads")]
    public async Task<IActionResult> Create([FromBody] CreateLeadDto dto, CancellationToken cancellationToken)
    {
        var result = await _leadService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanUpdateLeads")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeadDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest("Route ID and DTO ID do not match.");
        }

        var result = await _leadService.UpdateAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanDeleteLeads")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _leadService.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/convert-to-deal")]
    [Authorize(Policy = "CanUpdateLeads")]
    public async Task<IActionResult> ConvertToDeal(Guid id, [FromBody] ConvertLeadToDealDto? dto, CancellationToken cancellationToken)
    {
        dto ??= new ConvertLeadToDealDto(0, null);
        var result = await _leadService.ConvertLeadToDealAsync(id, dto, cancellationToken);
        return Ok(result);
    }
}
