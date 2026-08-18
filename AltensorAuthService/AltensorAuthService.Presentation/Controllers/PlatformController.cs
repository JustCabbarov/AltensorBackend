using AltensorAuthService.Application.Interfaces;
using AltensorAuthService.Contract.Tenants;
using AltensorAuthService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AltensorAuthService.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "PlatformSuperAdmin")]
    public class PlatformController : ControllerBase
    {
        private readonly ITenantManagementService _tenantManagementService;

        public PlatformController(ITenantManagementService tenantManagementService)
        {
            _tenantManagementService = tenantManagementService;
        }

        /// <summary>
        /// Bütün müştərilərin (tenant-ların) siyahısı
        /// </summary>
        [HttpGet("tenants")]
        [ProducesResponseType(typeof(List<TenantResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTenants([FromQuery] TenantStatus? status = null)
        {
            var tenants = await _tenantManagementService.GetAllTenantsAsync(status);
            return Ok(tenants);
        }

        /// <summary>
        /// Tək bir müştərinin tam detalları (abunəliklər, istifadəçi sayı və s.)
        /// </summary>
        [HttpGet("tenants/{id:guid}")]
        [ProducesResponseType(typeof(TenantDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTenantById([FromRoute] Guid id)
        {
            var tenant = await _tenantManagementService.GetTenantDetailAsync(id);
            return Ok(tenant);
        }

        /// <summary>
        /// Yeni müştəri (tenant) + onun ilk Tenant Admin istifadəçisini yaratmaq və modul abunəliklərini təyin etmək
        /// </summary>
        [HttpPost("tenants")]
        [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
        {
            var created = await _tenantManagementService.CreateTenantWithAdminAsync(request);
            return CreatedAtAction(nameof(GetTenantById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Müştərinin girişini tam bloklamaq/dondurmaq (ödəniş edilmədikdə). Bütün modullara giriş və refresh tokenlər dərhal bağlanır.
        /// </summary>
        [HttpPost("tenants/{id:guid}/suspend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SuspendTenant([FromRoute] Guid id, [FromBody] SuspendTenantRequest? request)
        {
            await _tenantManagementService.SuspendTenantAsync(id, request?.Reason);
            return Ok(new { message = "Müştərinin hesabı uğurla donduruldu və bütün sessiyaları ləğv edildi." });
        }

        /// <summary>
        /// Dondurulmuş müştərinin hesabını yenidən aktivləşdirmək (ödəniş edildikdə)
        /// </summary>
        [HttpPost("tenants/{id:guid}/unsuspend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnsuspendTenant([FromRoute] Guid id)
        {
            await _tenantManagementService.UnsuspendTenantAsync(id);
            return Ok(new { message = "Müştərinin hesabı və modulları yenidən aktivləşdirildi." });
        }

        /// <summary>
        /// Müştəriyə yeni modul abunəliyi əlavə etmək
        /// </summary>
        [HttpPost("tenants/{id:guid}/modules")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddModule([FromRoute] Guid id, [FromBody] ModuleSubscriptionRequest request)
        {
            await _tenantManagementService.AddModuleSubscriptionAsync(id, request);
            return Ok(new { message = "Modul abunəliyi əlavə edildi." });
        }

        /// <summary>
        /// Müştərinin modul abunəliyini silmək
        /// </summary>
        [HttpDelete("tenants/{id:guid}/modules/{moduleId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveModule([FromRoute] Guid id, [FromRoute] Guid moduleId)
        {
            await _tenantManagementService.RemoveModuleSubscriptionAsync(id, moduleId);
            return Ok(new { message = "Modul abunəliyi silindi." });
        }

        /// <summary>
        /// Müştərinin tək bir modulunu dondurmaq (həmin modula ödəniş kəsildikdə)
        /// </summary>
        [HttpPost("tenants/{id:guid}/modules/{moduleId:guid}/suspend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SuspendModule([FromRoute] Guid id, [FromRoute] Guid moduleId, [FromBody] SuspendModuleRequest? request)
        {
            await _tenantManagementService.SuspendModuleSubscriptionAsync(id, moduleId, request?.Reason);
            return Ok(new { message = "Modul abunəliyi donduruldu." });
        }

        /// <summary>
        /// Dondurulmuş modulu yenidən açmaq
        /// </summary>
        [HttpPost("tenants/{id:guid}/modules/{moduleId:guid}/unsuspend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UnsuspendModule([FromRoute] Guid id, [FromRoute] Guid moduleId)
        {
            await _tenantManagementService.UnsuspendModuleSubscriptionAsync(id, moduleId);
            return Ok(new { message = "Modul abunəliyi yenidən aktivləşdirildi." });
        }
    }
}
