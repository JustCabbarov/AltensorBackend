using Contract.DTOs;
using Contract.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkGroupController : ControllerBase
    {
        private readonly IWorkGroupService _workGroupService;

        public WorkGroupController(IWorkGroupService workGroupService)
        {
            _workGroupService = workGroupService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkGroup([FromBody] WorkGroupDTO workgroupDto)
        {
            var result = await _workGroupService.CreateWorkGroupAsync(workgroupDto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllWorkGroups()
        {
            var workgroups = await _workGroupService.GetAllWorkGroupsAsync();
            return Ok(workgroups);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetWorkGroup(Guid id)
        {
            var workgroup = await _workGroupService.GetWorkGroupByIdAsync(id);
            return Ok(workgroup);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateWorkGroup(Guid id, [FromBody] WorkGroupDTO workgroupDto)
        {
            workgroupDto.Id = id;
            await _workGroupService.UpdateWorkGroupAsync(workgroupDto);
            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteWorkGroup(Guid id)
        {
            await _workGroupService.DeleteWorkGroupAsync(id);
            return Ok();
        }

        [HttpPost("{workGroupId:guid}/AddUser/{userId}")]
        public async Task<IActionResult> AddUserToWorkGroup(Guid workGroupId, string userId)
        {
            await _workGroupService.AddUserToWorkGroupAsync(workGroupId, userId);
            return Ok(new { Message = "User added to workgroup successfully" });
        }

        [HttpPost("{workGroupId:guid}/RemoveUser/{userId}")]
        public async Task<IActionResult> RemoveUserFromWorkGroup(Guid workGroupId, string userId)
        {
            await _workGroupService.RemoveUserFromWorkGroupAsync(workGroupId, userId);
            return Ok(new { Message = "User removed from workgroup successfully" });
        }

        [HttpPost("AssignTask")]
        public async Task<IActionResult> AssignTaskToGroup(Guid taskId, Guid targetWorkGroupId)
        {
            var leaderId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            await _workGroupService.AssignTaskToGroupAsync(taskId, leaderId!, targetWorkGroupId);
            return Ok(new { Message = "Task assigned to target group successfully" });
        }
    }
}
