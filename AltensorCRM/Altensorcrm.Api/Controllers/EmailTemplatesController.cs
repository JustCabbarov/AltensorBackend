using Altensorcrm.Contract.DTOs.EmailTemplate;
using Altensorcrm.Contract.Services.EmailTemplate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altensorcrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmailTemplatesController : ControllerBase
{
    private readonly IEmailTemplateService _emailTemplateService;

    public EmailTemplatesController(IEmailTemplateService emailTemplateService)
    {
        _emailTemplateService = emailTemplateService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmailTemplateDetailDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _emailTemplateService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmailTemplateDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _emailTemplateService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<EmailTemplateDetailDto>> Create([FromBody] CreateEmailTemplateDto dto, CancellationToken cancellationToken)
    {
        var result = await _emailTemplateService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmailTemplateDetailDto>> Update(Guid id, [FromBody] UpdateEmailTemplateDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest("Route ID and body ID mismatch");
        }

        var result = await _emailTemplateService.UpdateAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/toggle")]
    public async Task<ActionResult<EmailTemplateDetailDto>> ToggleEnabled(Guid id, CancellationToken cancellationToken)
    {
        var result = await _emailTemplateService.ToggleEnabledAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _emailTemplateService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
