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
    public class TaskAttachmentController : ControllerBase
    {
        private readonly ITaskAttachmentService _taskAttachmentService;
        public TaskAttachmentController(ITaskAttachmentService taskAttachmentService)
        {
            _taskAttachmentService = taskAttachmentService;
        }

        [HttpGet("{attachmentId:guid}/download")]
        public async Task<IActionResult> DownloadAttachment(Guid attachmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var fileDto = await _taskAttachmentService.DownloadAsync(attachmentId, userId!);
            if (fileDto == null)
            {
                return NotFound();
            }
            return File(fileDto.Content, fileDto.ContentType, fileDto.FileName);
        }

        [HttpGet("{attachmentId:guid}/preview-url")]
        public async Task<IActionResult> GetPresignedUrl(Guid attachmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var url = await _taskAttachmentService.GetPreviewUrlAsync(attachmentId, userId!);
            if (url == null)
            {
                return NotFound();
            }
            return Ok(new { Url = url });
        }
    }
}
