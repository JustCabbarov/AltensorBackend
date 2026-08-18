using Altensorcrm.Contract.DTOs.Task;
using Altensorcrm.Contract.Services.Task;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altensorcrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("department/{departmentName}")]
    public async Task<IActionResult> GetDepartmentTasks(string departmentName, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetDepartmentTasksAsync(departmentName, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto, CancellationToken cancellationToken)
    {
        var result = await _taskService.CreateTaskAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{taskId:guid}/checklist/{checklistItemId:guid}/toggle")]
    public async Task<IActionResult> ToggleChecklistItem(Guid taskId, Guid checklistItemId, CancellationToken cancellationToken)
    {
        var result = await _taskService.ToggleChecklistItemAsync(taskId, checklistItemId, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{taskId:guid}/status")]
    public async Task<IActionResult> UpdateTaskStatus(Guid taskId, [FromQuery] bool isCompleted, CancellationToken cancellationToken)
    {
        var result = await _taskService.UpdateTaskStatusAsync(taskId, isCompleted, cancellationToken);
        return Ok(result);
    }
}
