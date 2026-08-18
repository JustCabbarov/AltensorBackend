using Altensorcrm.Contract.DTOs.CustomView;
using Altensorcrm.Contract.Services.CustomView;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altensorcrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomViewsController : ControllerBase
{
    private readonly ICustomViewService _customViewService;

    public CustomViewsController(ICustomViewService customViewService)
    {
        _customViewService = customViewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByModule([FromQuery] string module, CancellationToken cancellationToken)
    {
        var result = await _customViewService.GetByModuleAsync(module, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomViewDto dto, CancellationToken cancellationToken)
    {
        var result = await _customViewService.CreateAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _customViewService.DeleteAsync(id, cancellationToken);
        return Ok(new { success = result });
    }
}
