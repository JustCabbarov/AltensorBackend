using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Contract.DTOs.Organization;
using Altensorcrm.Contract.Services.Organization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altensorcrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedList([FromQuery] OrganizationFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _organizationService.GetPagedListAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> GetLookup(CancellationToken cancellationToken)
    {
        var result = await _organizationService.GetLookupAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _organizationService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationDto dto, CancellationToken cancellationToken)
    {
        var result = await _organizationService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrganizationDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest("Route ID and DTO ID do not match.");
        }

        var result = await _organizationService.UpdateAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _organizationService.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/contacts")]
    public async Task<IActionResult> GetContacts(Guid id, CancellationToken cancellationToken)
    {
        var result = await _organizationService.GetContactsByOrganizationIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/deals")]
    public async Task<IActionResult> GetDeals(Guid id, CancellationToken cancellationToken)
    {
        var result = await _organizationService.GetDealsByOrganizationIdAsync(id, cancellationToken);
        return Ok(result);
    }
}
