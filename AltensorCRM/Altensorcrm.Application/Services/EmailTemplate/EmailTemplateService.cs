using AutoMapper;
using Altensorcrm.Application.Exceptions;
using Altensorcrm.Contract.DTOs.EmailTemplate;
using Altensorcrm.Contract.Services.EmailTemplate;
using Altensorcrm.Domain.Repository;

namespace Altensorcrm.Application.Services.EmailTemplate;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EmailTemplateService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EmailTemplateDetailDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _unitOfWork.EmailTemplates.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<EmailTemplateDetailDto>>(templates.OrderByDescending(t => t.CreatedAt));
    }

    public async Task<EmailTemplateDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _unitOfWork.EmailTemplates.GetByIdAsync(id, cancellationToken);
        if (template is null)
        {
            throw new NotFoundException(nameof(Domain.Entity.EmailTemplate), id);
        }

        return _mapper.Map<EmailTemplateDetailDto>(template);
    }

    public async Task<EmailTemplateDetailDto> CreateAsync(CreateEmailTemplateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Domain.Entity.EmailTemplate>(dto);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.EmailTemplates.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<EmailTemplateDetailDto>(entity);
    }

    public async Task<EmailTemplateDetailDto> UpdateAsync(UpdateEmailTemplateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.EmailTemplates.GetByIdAsync(dto.Id, cancellationToken);
        if (entity is null)
        {
            throw new NotFoundException(nameof(Domain.Entity.EmailTemplate), dto.Id);
        }

        _mapper.Map(dto, entity);
        _unitOfWork.EmailTemplates.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<EmailTemplateDetailDto>(entity);
    }

    public async Task<EmailTemplateDetailDto> ToggleEnabledAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.EmailTemplates.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new NotFoundException(nameof(Domain.Entity.EmailTemplate), id);
        }

        entity.Enabled = !entity.Enabled;
        _unitOfWork.EmailTemplates.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<EmailTemplateDetailDto>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.EmailTemplates.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new NotFoundException(nameof(Domain.Entity.EmailTemplate), id);
        }

        _unitOfWork.EmailTemplates.Delete(entity);
        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
