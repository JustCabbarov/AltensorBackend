using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Contract.DTOs.Contact;
using Altensorcrm.Contract.Services.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altensorcrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactsController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewContacts")]
    public async Task<IActionResult> GetPagedList([FromQuery] ContactFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _contactService.GetPagedListAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("lookup")]
    [Authorize(Policy = "CanViewContacts")]
    public async Task<IActionResult> GetLookup(CancellationToken cancellationToken)
    {
        var result = await _contactService.GetLookupAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanViewContacts")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contactService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "CanCreateContacts")]
    public async Task<IActionResult> Create([FromBody] CreateContactDto dto, CancellationToken cancellationToken)
    {
        var result = await _contactService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanUpdateContacts")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest("Route ID and DTO ID do not match.");
        }

        var result = await _contactService.UpdateAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanDeleteContacts")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contactService.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }
}
