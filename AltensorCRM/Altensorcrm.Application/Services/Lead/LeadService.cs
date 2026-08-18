using AutoMapper;
using Altensorcrm.Application.Exceptions;
using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Contract.DTOs.Deal;
using Altensorcrm.Contract.DTOs.Lead;
using Altensorcrm.Contract.Services.Lead;
using Altensorcrm.Domain.Enums;
using Altensorcrm.Domain.Repository;

using LeadEntity = Altensorcrm.Domain.Entity.Lead;
using DealEntity = Altensorcrm.Domain.Entity.Deal;
using ContactEntity = Altensorcrm.Domain.Entity.Contact;
using OrganizationEntity = Altensorcrm.Domain.Entity.Organization;
using CommentEntity = Altensorcrm.Domain.Entity.Comment;
using AttachmentEntity = Altensorcrm.Domain.Entity.Attachment;

namespace Altensorcrm.Application.Services.Lead;

public class LeadService : ILeadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LeadService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<LeadDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await _unitOfWork.Leads.GetLeadWithDetailsByIdAsync(id, cancellationToken);
        if (lead is null)
        {
            throw new NotFoundException(nameof(LeadEntity), id);
        }

        return _mapper.Map<LeadDetailDto>(lead);
    }

    public async Task<PagedResult<LeadListDto>> GetPagedListAsync(LeadFilterDto filter, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Leads.GetFilteredLeadsAsync(
            filter.SearchTerm,
            filter.Status,
            filter.OwnerId,
            filter.Page,
            filter.PageSize,
            cancellationToken);

        var listDtos = _mapper.Map<IReadOnlyList<LeadListDto>>(items);

        return new PagedResult<LeadListDto>
        {
            Items = listDtos,
            TotalCount = totalCount,
            PageNumber = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<LeadDetailDto> CreateAsync(CreateLeadDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName))
        {
            throw new ValidationException("First Name is required.");
        }

        var leadEntity = _mapper.Map<LeadEntity>(dto);
        if (string.IsNullOrWhiteSpace(leadEntity.LastName))
        {
            leadEntity.LastName = string.Empty;
        }

        // Safely resolve LeadOwnerId foreign key constraint
        if (!dto.LeadOwnerId.HasValue || dto.LeadOwnerId.Value == Guid.Empty)
        {
            var firstUser = (await _unitOfWork.Repository<Domain.Entity.User>().GetAllAsync(cancellationToken)).FirstOrDefault();
            leadEntity.LeadOwnerId = firstUser?.Id;
        }
        else
        {
            var userExists = await _unitOfWork.Repository<Domain.Entity.User>().ExistsAsync(u => u.Id == dto.LeadOwnerId.Value, cancellationToken);
            if (!userExists)
            {
                var firstUser = (await _unitOfWork.Repository<Domain.Entity.User>().GetAllAsync(cancellationToken)).FirstOrDefault();
                leadEntity.LeadOwnerId = firstUser?.Id;
            }
        }

        leadEntity.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Leads.AddAsync(leadEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(leadEntity.Id, cancellationToken);
    }

    public async Task<LeadDetailDto> UpdateAsync(UpdateLeadDto dto, CancellationToken cancellationToken = default)
    {
        var lead = await _unitOfWork.Leads.GetByIdAsync(dto.Id, cancellationToken);
        if (lead is null)
        {
            throw new NotFoundException(nameof(LeadEntity), dto.Id);
        }

        _mapper.Map(dto, lead);
        _unitOfWork.Leads.Update(lead);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(dto.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await _unitOfWork.Leads.GetByIdAsync(id, cancellationToken);
        if (lead is null)
        {
            throw new NotFoundException(nameof(LeadEntity), id);
        }

        _unitOfWork.Leads.Delete(lead);
        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public async Task<DealDetailDto> ConvertLeadToDealAsync(Guid leadId, ConvertLeadToDealDto dto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var lead = await _unitOfWork.Leads.GetLeadWithDetailsByIdAsync(leadId, cancellationToken);
            if (lead is null)
            {
                throw new NotFoundException(nameof(LeadEntity), leadId);
            }

            lead.Status = LeadStatus.ConvertToDeal;
            _unitOfWork.Leads.Update(lead);

            var existingOrganizations = await _unitOfWork.Organizations.FindAsync(
                o => o.OrganizationName.ToLower() == lead.CompanyName.ToLower(), cancellationToken);

            OrganizationEntity organization;
            if (existingOrganizations.Count > 0)
            {
                organization = existingOrganizations[0];
            }
            else
            {
                organization = new OrganizationEntity
                {
                    Id = Guid.NewGuid(),
                    OrganizationName = lead.CompanyName,
                    AnnualRevenue = lead.AnnualRevenue,
                    Website = lead.Website,
                    TerritoryId = lead.TerritoryId,
                    NoOfEmployees = lead.NoOfEmployees,
                    Industry = lead.Industry,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Organizations.AddAsync(organization, cancellationToken);
            }

            Guid? assignedUserId = dto.AssignedUserId;
            if (assignedUserId.HasValue && assignedUserId.Value != Guid.Empty)
            {
                var userExists = await _unitOfWork.Repository<Domain.Entity.User>().ExistsAsync(u => u.Id == assignedUserId.Value, cancellationToken);
                if (!userExists) assignedUserId = null;
            }
            if (!assignedUserId.HasValue || assignedUserId.Value == Guid.Empty)
            {
                assignedUserId = lead.LeadOwnerId;
                if (!assignedUserId.HasValue || assignedUserId.Value == Guid.Empty)
                {
                    var firstUser = (await _unitOfWork.Repository<Domain.Entity.User>().GetAllAsync(cancellationToken)).FirstOrDefault();
                    assignedUserId = firstUser?.Id;
                }
            }

            var contact = new ContactEntity
            {
                Id = Guid.NewGuid(),
                Salutation = lead.Salutation,
                FirstName = string.IsNullOrWhiteSpace(lead.FirstName) ? "Contact" : lead.FirstName,
                LastName = lead.LastName ?? string.Empty,
                EmailAddress = lead.Email ?? string.Empty,
                MobileNo = lead.MobileNo ?? string.Empty,
                Gender = lead.Gender,
                CompanyName = lead.CompanyName,
                OrganizationId = organization.Id,
                AssignedUserId = assignedUserId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Contacts.AddAsync(contact, cancellationToken);

            var randomSuffix = new Random().Next(10000, 99999);
            var dealCode = $"CRM-DEAL-{DateTime.UtcNow.Year}-{randomSuffix}";

            var deal = new DealEntity
            {
                Id = Guid.NewGuid(),
                OrganizationName = lead.CompanyName,
                PrimaryEmail = lead.Email ?? string.Empty,
                PrimaryMobileNo = lead.MobileNo ?? string.Empty,
                Salutation = lead.Salutation,
                FirstName = string.IsNullOrWhiteSpace(lead.FirstName) ? "Contact" : lead.FirstName,
                LastName = lead.LastName ?? string.Empty,
                Gender = lead.Gender,
                Website = lead.Website,
                NoOfEmployees = lead.NoOfEmployees,
                TerritoryId = lead.TerritoryId,
                AnnualRevenue = dto.DealAmount,
                Industry = lead.Industry,
                Status = DealStatus.Qualification,
                DealOwnerId = assignedUserId,
                SourceLeadId = lead.Id,
                OrganizationId = organization.Id,
                ContactId = contact.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Deals.AddAsync(deal, cancellationToken);

            foreach (var note in lead.Notes)
            {
                note.DealId = deal.Id;
                _unitOfWork.Notes.Update(note);
            }

            foreach (var callLog in lead.CallLogs)
            {
                callLog.DealId = deal.Id;
                _unitOfWork.CallLogs.Update(callLog);
            }

            foreach (var comment in lead.Comments)
            {
                comment.DealId = deal.Id;
                _unitOfWork.Repository<CommentEntity>().Update(comment);
            }

            foreach (var attachment in lead.Attachments)
            {
                attachment.DealId = deal.Id;
                _unitOfWork.Repository<AttachmentEntity>().Update(attachment);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var createdDeal = await _unitOfWork.Deals.GetDealWithDetailsByIdAsync(deal.Id, cancellationToken);
            return _mapper.Map<DealDetailDto>(createdDeal!);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
