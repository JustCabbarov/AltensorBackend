using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Contract.DTOs.Deal;
using Altensorcrm.Contract.Services.Deal;
using Altensorcrm.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altensorcrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DealsController : ControllerBase
{
    private readonly IDealService _dealService;

    public DealsController(IDealService dealService)
    {
        _dealService = dealService;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewDeals")]
    public async Task<IActionResult> GetPagedList([FromQuery] DealFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _dealService.GetPagedListAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanViewDeals")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _dealService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "CanCreateDeals")]
    public async Task<IActionResult> Create([FromBody] CreateDealDto dto, CancellationToken cancellationToken)
    {
        var result = await _dealService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanUpdateDeals")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDealDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest("Route ID and DTO ID do not match.");
        }

        var result = await _dealService.UpdateAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/stage")]
    [Authorize(Policy = "CanUpdateDeals")]
    public async Task<IActionResult> UpdateStage(Guid id, [FromQuery] string newStatus, [FromQuery] string? lostReason, CancellationToken cancellationToken)
    {
        DealStatus parsedStatus = DealStatus.Qualification;
        if (!string.IsNullOrWhiteSpace(newStatus))
        {
            var cleaned = newStatus.Replace("/", "").Replace(" ", "").Replace("_", "").ToLower();
            if (cleaned.Contains("demo")) parsedStatus = DealStatus.Demo;
            else if (cleaned.Contains("proposal") || cleaned.Contains("quote")) parsedStatus = DealStatus.Proposal;
            else if (cleaned.Contains("negotiat")) parsedStatus = DealStatus.Negotiation;
            else if (cleaned.Contains("ready") || cleaned.Contains("close")) parsedStatus = DealStatus.ReadyToClose;
            else if (cleaned.Contains("won")) parsedStatus = DealStatus.Won;
            else if (cleaned.Contains("lost")) parsedStatus = DealStatus.Lost;
            else if (Enum.TryParse<DealStatus>(newStatus, true, out var resultEnum)) parsedStatus = resultEnum;
        }

        var result = await _dealService.UpdateStageAsync(id, parsedStatus, lostReason, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanDeleteDeals")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _dealService.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }
}
