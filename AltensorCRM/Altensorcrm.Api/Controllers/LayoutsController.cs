using Altensorcrm.Contract.DTOs.Layout;
using Altensorcrm.Contract.Services.Layout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altensorcrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LayoutsController : ControllerBase
{
    private readonly ILayoutService _layoutService;

    public LayoutsController(ILayoutService layoutService)
    {
        _layoutService = layoutService;
    }

    [HttpGet("{moduleName}")]
    public async Task<IActionResult> GetByModule(string moduleName, CancellationToken cancellationToken)
    {
        var result = await _layoutService.GetByModuleAsync(moduleName, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{moduleName}")]
    public async Task<IActionResult> UpdateByModule(string moduleName, [FromBody] UpdateLayoutDto dto, CancellationToken cancellationToken)
    {
        var result = await _layoutService.UpdateByModuleAsync(moduleName, dto, cancellationToken);
        return Ok(result);
    }
}
