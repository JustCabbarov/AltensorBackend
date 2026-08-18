using Altensorcrm.Contract.DTOs.EmailTemplate;

namespace Altensorcrm.Contract.Services.EmailTemplate;

public interface IEmailTemplateService
{
    Task<IReadOnlyList<EmailTemplateDetailDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EmailTemplateDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmailTemplateDetailDto> CreateAsync(CreateEmailTemplateDto dto, CancellationToken cancellationToken = default);
    Task<EmailTemplateDetailDto> UpdateAsync(UpdateEmailTemplateDto dto, CancellationToken cancellationToken = default);
    Task<EmailTemplateDetailDto> ToggleEnabledAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
