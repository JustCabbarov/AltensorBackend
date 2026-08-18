using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Altensorcrm.Contract.DTOs.Email;
using Altensorcrm.Contract.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altensorcrm.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmailsController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailsController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ToEmail))
            {
                return BadRequest(new { message = "Recipient email (ToEmail) is required." });
            }

            Guid? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var parsedId))
            {
                userId = parsedId;
            }

            var result = await _emailService.SendEmailAsync(dto, userId);
            return Ok(result);
        }

        [HttpGet("lead/{leadId}")]
        public async Task<IActionResult> GetByLeadId(Guid leadId)
        {
            var logs = await _emailService.GetLogsByLeadIdAsync(leadId);
            return Ok(logs);
        }

        [HttpGet("deal/{dealId}")]
        public async Task<IActionResult> GetByDealId(Guid dealId)
        {
            var logs = await _emailService.GetLogsByDealIdAsync(dealId);
            return Ok(logs);
        }
    }
}
