using Contract.DTOs;
using Contract.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly IGenericService<TaskDTO, TaskItem> _genericService;
        private readonly ITaksService _taskService;
        private readonly ITaskAttachmentService _taskAttachmentService;
        private readonly INotificationService _notificationService;

        public TaskController(
            IGenericService<TaskDTO, TaskItem> genericService,
            ITaksService taskService,
            ITaskAttachmentService taskAttachmentService,
            INotificationService notificationService)
        {
            _genericService = genericService;
            _taskService = taskService;
            _taskAttachmentService = taskAttachmentService;
            _notificationService = notificationService;
        }

        [Authorize]
        [HttpPost("CreateTask")]
        public async Task<IActionResult> CreateTask([FromForm] TaskDTO taskDto, [FromForm] List<IFormFile> files)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(taskDto.CreatedByUserId))
            {
                taskDto.CreatedByUserId = userId ?? Guid.Empty.ToString();
            }

            var createdTask = await _genericService.AddAsync(taskDto);

            if (files != null && files.Count > 0)
            {
                var uploadTasks = files.Select(async file =>
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);

                    var fileDto = new FileDto
                    {
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        Content = ms.ToArray()
                    };

                    await _taskAttachmentService.UploadAndSaveAsync(createdTask.Id, fileDto, taskDto.CreatedByUserId);
                });
                await Task.WhenAll(uploadTasks);
            }

            if (taskDto.AssignedToUserId != null)
            {
                await _notificationService.NotifyTaskAssignedAsync(taskDto.AssignedToUserId, createdTask.Title, createdTask.Id);
            }

            return Ok(createdTask);
        }

        [Authorize]
        [HttpGet("GetTask/{id}")]
        public async Task<IActionResult> GetTask(Guid id)
        {
            var task = await _genericService.GetByIdAsync(id, query => query.Include(t => t.Attachments));
            if (task == null)
            {
                return NotFound();
            }
            return Ok(task);
        }

        [Authorize]
        [HttpPut("UpdateTask")]
        public async Task<IActionResult> UpdateTask([FromBody] TaskDTO taskDto)
        {
            if (taskDto == null || taskDto.Id == Guid.Empty)
            {
                return BadRequest("Yanlış və ya boş tapşırıq məlumatı.");
            }
            var updatedTask = await _genericService.UpdateAsync(taskDto);
            return Ok("Dəyişikliklər Qeyd Olundu");
        }

        [Authorize]
        [HttpPost("AddFilesToTask/{taskId}")]
        public async Task<IActionResult> AddFilesToTask(Guid taskId, [FromForm] List<IFormFile>? files)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (files == null || files.Count == 0)
                return BadRequest("Fayl göndərilməyib.");

            var uploadTasks = files.Select(async file =>
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                var fileDto = new FileDto
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Content = ms.ToArray()
                };

                await _taskAttachmentService.UploadAndSaveAsync(taskId, fileDto, userId!);
            });

            await Task.WhenAll(uploadTasks);
            return Ok("Fayllar uğurla əlavə olundu.");
        }

        [Authorize]
        [HttpGet("GetAllTask")]
        public async Task<IActionResult> GetAllTask()
        {
            var tasks = await _genericService.GetAllAsync();
            return Ok(tasks);
        }

        [Authorize]
        [HttpDelete("DeleteTask/{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var result = await _genericService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [Authorize]
        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment(Guid taskId, string comment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            await _taskService.AddComment(taskId, userId!, comment);
            return Ok();
        }

        [Authorize]
        [HttpPost("AssignTask")]
        public async Task<IActionResult> AssignTask(Guid taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            await _taskService.AssignTaskAsync(taskId, userId!);
            return Ok();
        }

        [Authorize]
        [HttpPost("UnAssignTask")]
        public async Task<IActionResult> UnAssignTask(Guid taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            await _taskService.UnAssingTaskAsync(taskId, userId!);
            return Ok();
        }

        [Authorize]
        [HttpPost("AcceptTask")]
        public async Task<IActionResult> AcceptTask(Guid taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            await _taskService.AcceptTask(taskId, userId!);
            return Ok();
        }

        [Authorize]
        [HttpPost("reject")]
        public async Task<IActionResult> RejectTask(Guid taskId, string reason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            await _taskService.RejectTask(taskId, userId!, reason);
            return Ok();
        }

        [Authorize]
        [HttpPost("FinishTask")]
        public async Task<IActionResult> FinishTask(Guid taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            await _taskService.FinishTask(taskId, userId!);
            return Ok();
        }

        [Authorize]
        [HttpPost("ReopenTask")]
        public async Task<IActionResult> ReturnedForRevision(Guid taskId, string userId, string reason)
        {
            await _taskService.ReturnedForRevision(taskId, userId, reason);
            return Ok();
        }
    }
}
