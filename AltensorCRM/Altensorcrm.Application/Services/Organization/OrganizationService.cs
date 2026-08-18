using AutoMapper;
using Altensorcrm.Application.Exceptions;
using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Contract.DTOs.Organization;
using Altensorcrm.Contract.Services.Organization;
using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Repository;

namespace Altensorcrm.Application.Services.Organization;

public class OrganizationService : IOrganizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrganizationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrganizationDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var org = await _unitOfWork.Organizations.GetOrganizationWithDetailsByIdAsync(id, cancellationToken);
        if (org is null)
        {
            throw new NotFoundException(nameof(Domain.Entity.Organization), id);
        }

        return _mapper.Map<OrganizationDetailDto>(org);
    }

    public async Task<PagedResult<OrganizationListDto>> GetPagedListAsync(OrganizationFilterDto filter, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Organizations.GetPagedResponseAsync(
            filter.Page,
            filter.PageSize,
            o => (string.IsNullOrWhiteSpace(filter.Company) || o.OrganizationName.ToLower().Contains(filter.Company.ToLower())) &&
                 (!filter.TerritoryId.HasValue || o.TerritoryId == filter.TerritoryId.Value) &&
                 (!filter.Industry.HasValue || o.Industry == filter.Industry.Value) &&
                 (string.IsNullOrWhiteSpace(filter.SearchTerm) || o.OrganizationName.ToLower().Contains(filter.SearchTerm.ToLower())),
            cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<OrganizationListDto>>(items);

        return new PagedResult<OrganizationListDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<IReadOnlyList<OrganizationListDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        var orgs = await _unitOfWork.Organizations.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<OrganizationListDto>>(orgs);
    }

    public async Task<OrganizationDetailDto> CreateAsync(CreateOrganizationDto dto, CancellationToken cancellationToken = default)
    {
        var org = _mapper.Map<Domain.Entity.Organization>(dto);
        if (string.IsNullOrWhiteSpace(org.OrganizationName))
        {
            org.OrganizationName = "New Organization";
        }

        org.CreatedAt = DateTime.UtcNow;

        if (dto.Address is not null)
        {
            var address = _mapper.Map<Address>(dto.Address);
            await _unitOfWork.Repository<Address>().AddAsync(address, cancellationToken);
            org.AddressId = address.Id;
        }

        await _unitOfWork.Organizations.AddAsync(org, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var created = await _unitOfWork.Organizations.GetOrganizationWithDetailsByIdAsync(org.Id, cancellationToken);
            if (created != null) return _mapper.Map<OrganizationDetailDto>(created);
        }
        catch { }

        return _mapper.Map<OrganizationDetailDto>(org);
    }

    public async Task<OrganizationDetailDto> UpdateAsync(UpdateOrganizationDto dto, CancellationToken cancellationToken = default)
    {
        var org = await _unitOfWork.Organizations.GetByIdAsync(dto.Id, cancellationToken);
        if (org is null)
        {
            throw new NotFoundException(nameof(Domain.Entity.Organization), dto.Id);
        }

        _mapper.Map(dto, org);

        if (string.IsNullOrWhiteSpace(org.OrganizationName)) org.OrganizationName = "Organization";

        if (dto.Address is not null)
        {
            if (org.AddressId.HasValue)
            {
                var existingAddress = await _unitOfWork.Repository<Address>().GetByIdAsync(org.AddressId.Value, cancellationToken);
                if (existingAddress is not null)
                {
                    _mapper.Map(dto.Address, existingAddress);
                    _unitOfWork.Repository<Address>().Update(existingAddress);
                }
            }
            else
            {
                var newAddress = _mapper.Map<Address>(dto.Address);
                await _unitOfWork.Repository<Address>().AddAsync(newAddress, cancellationToken);
                org.AddressId = newAddress.Id;
            }
        }

        _unitOfWork.Organizations.Update(org);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(dto.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var org = await _unitOfWork.Organizations.GetByIdAsync(id, cancellationToken);
        if (org is null)
        {
            throw new NotFoundException(nameof(Domain.Entity.Organization), id);
        }

        _unitOfWork.Organizations.Delete(org);
        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public async Task<IReadOnlyList<Altensorcrm.Contract.DTOs.Contact.ContactListDto>> GetContactsByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var contacts = await _unitOfWork.Repository<Altensorcrm.Domain.Entity.Contact>().FindAsync(c     => c.OrganizationId == organizationId, cancellationToken);
        return _mapper.Map<IReadOnlyList<Altensorcrm.Contract.DTOs.Contact.ContactListDto>>(contacts.ToList());
    }

    public async Task<IReadOnlyList<Altensorcrm.Contract.DTOs.Deal.DealListDto>> GetDealsByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var deals = await _unitOfWork.Repository<Altensorcrm.Domain.Entity.Deal>().FindAsync(d => d.OrganizationId == organizationId, cancellationToken);
        return _mapper.Map<IReadOnlyList<Altensorcrm.Contract.DTOs.Deal.DealListDto>>(deals.ToList());
    }
}
