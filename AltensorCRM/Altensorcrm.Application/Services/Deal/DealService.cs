using AutoMapper;
using Altensorcrm.Application.Exceptions;
using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Contract.DTOs.Deal;
using Altensorcrm.Contract.Services.Deal;
using Altensorcrm.Domain.Enums;
using Altensorcrm.Domain.Repository;

using DealEntity = Altensorcrm.Domain.Entity.Deal;

namespace Altensorcrm.Application.Services.Deal;

public class DealService : IDealService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DealService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DealDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deal = await _unitOfWork.Deals.GetDealWithDetailsByIdAsync(id, cancellationToken);
        if (deal is null)
        {
            throw new NotFoundException(nameof(DealEntity), id);
        }

        return _mapper.Map<DealDetailDto>(deal);
    }

    public async Task<PagedResult<DealListDto>> GetPagedListAsync(DealFilterDto filter, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Deals.GetPagedResponseAsync(
            filter.Page,
            filter.PageSize,
            d => (!filter.Status.HasValue || d.Status == filter.Status.Value) &&
                 (!filter.OwnerId.HasValue || d.DealOwnerId == filter.OwnerId.Value) &&
                 (!filter.Industry.HasValue || d.Industry == filter.Industry.Value) &&
                 (!filter.TerritoryId.HasValue || d.TerritoryId == filter.TerritoryId.Value) &&
                 (string.IsNullOrWhiteSpace(filter.SearchTerm) ||
                  d.OrganizationName.ToLower().Contains(filter.SearchTerm.ToLower()) ||
                  d.PrimaryEmail.ToLower().Contains(filter.SearchTerm.ToLower()) ||
                  d.PrimaryMobileNo.Contains(filter.SearchTerm)),
            cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<DealListDto>>(items);

        return new PagedResult<DealListDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<DealDetailDto> CreateAsync(CreateDealDto dto, CancellationToken cancellationToken = default)
    {
        var deal = _mapper.Map<DealEntity>(dto);
        if (string.IsNullOrWhiteSpace(deal.OrganizationName))
        {
            deal.OrganizationName = "Default Organization";
        }
        if (string.IsNullOrWhiteSpace(deal.FirstName))
        {
            deal.FirstName = "Contact";
        }
        if (string.IsNullOrWhiteSpace(deal.LastName))
        {
            deal.LastName = string.Empty;
        }
        if (string.IsNullOrWhiteSpace(deal.PrimaryEmail))
        {
            deal.PrimaryEmail = "user@example.com";
        }
        if (string.IsNullOrWhiteSpace(deal.PrimaryMobileNo))
        {
            deal.PrimaryMobileNo = "0551234567";
        }

        // Safely resolve DealOwnerId foreign key constraint
        if (!dto.DealOwnerId.HasValue || dto.DealOwnerId.Value == Guid.Empty)
        {
            var firstUser = (await _unitOfWork.Repository<Domain.Entity.User>().GetAllAsync(cancellationToken)).FirstOrDefault();
            deal.DealOwnerId = firstUser?.Id;
        }
        else
        {
            var userExists = await _unitOfWork.Repository<Domain.Entity.User>().ExistsAsync(u => u.Id == dto.DealOwnerId.Value, cancellationToken);
            if (!userExists)
            {
                var firstUser = (await _unitOfWork.Repository<Domain.Entity.User>().GetAllAsync(cancellationToken)).FirstOrDefault();
                deal.DealOwnerId = firstUser?.Id;
            }
        }

        deal.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Deals.AddAsync(deal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var created = await _unitOfWork.Deals.GetDealWithDetailsByIdAsync(deal.Id, cancellationToken);
            if (created != null) return _mapper.Map<DealDetailDto>(created);
        }
        catch { }

        return _mapper.Map<DealDetailDto>(deal);
    }

    public async Task<DealDetailDto> UpdateAsync(UpdateDealDto dto, CancellationToken cancellationToken = default)
    {
        var deal = await _unitOfWork.Deals.GetByIdAsync(dto.Id, cancellationToken);
        if (deal is null)
        {
            throw new NotFoundException(nameof(DealEntity), dto.Id);
        }

        _mapper.Map(dto, deal);

        if (string.IsNullOrWhiteSpace(deal.OrganizationName)) deal.OrganizationName = "Company";
        if (string.IsNullOrWhiteSpace(deal.FirstName)) deal.FirstName = "Contact";
        if (deal.LastName is null) deal.LastName = string.Empty;
        if (deal.PrimaryEmail is null) deal.PrimaryEmail = string.Empty;
        if (deal.PrimaryMobileNo is null) deal.PrimaryMobileNo = string.Empty;

        if (dto.DealOwnerId.HasValue && dto.DealOwnerId.Value != Guid.Empty)
        {
            var userExists = await _unitOfWork.Repository<Domain.Entity.User>().ExistsAsync(u => u.Id == dto.DealOwnerId.Value, cancellationToken);
            if (!userExists) deal.DealOwnerId = null;
        }

        _unitOfWork.Deals.Update(deal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(dto.Id, cancellationToken);
    }

    public async Task<bool> UpdateStageAsync(Guid dealId, DealStatus newStatus, string? lostReason, CancellationToken cancellationToken = default)
    {
        var deal = await _unitOfWork.Deals.GetByIdAsync(dealId, cancellationToken);
        if (deal is null)
        {
            throw new NotFoundException(nameof(DealEntity), dealId);
        }

        if (newStatus == DealStatus.Lost && string.IsNullOrWhiteSpace(lostReason))
        {
            throw new ValidationException("A lost reason is mandatory when setting deal status to Lost.");
        }

        deal.Status = newStatus;
        if (newStatus == DealStatus.Lost)
        {
            deal.LostReason = lostReason;
        }

        _unitOfWork.Deals.Update(deal);
        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deal = await _unitOfWork.Deals.GetByIdAsync(id, cancellationToken);
        if (deal is null)
        {
            throw new NotFoundException(nameof(DealEntity), id);
        }

        _unitOfWork.Deals.Delete(deal);
        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
