using AutoMapper;
using Altensorcrm.Application.Exceptions;
using Altensorcrm.Contract.DTOs.CallLog;
using Altensorcrm.Contract.Services.CallLog;
using Altensorcrm.Domain.Repository;

namespace Altensorcrm.Application.Services.CallLog;

public class CallLogService : ICallLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CallLogService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CallLogDetailDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var logs = await _unitOfWork.CallLogs.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CallLogDetailDto>>(logs);
    }

    public async Task<CallLogDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var log = await _unitOfWork.CallLogs.GetByIdAsync(id, cancellationToken);
        if (log is null)
        {
            throw new NotFoundException(nameof(Domain.Entity.CallLog), id);
        }

        return _mapper.Map<CallLogDetailDto>(log);
    }

    public async Task<CallLogDetailDto> CreateAsync(CreateCallLogDto dto, CancellationToken cancellationToken = default)
    {
        var log = _mapper.Map<Domain.Entity.CallLog>(dto);
        if (string.IsNullOrWhiteSpace(log.ToNumber))
        {
            log.ToNumber = "0550000000";
        }
        if (string.IsNullOrWhiteSpace(log.FromNumber))
        {
            log.FromNumber = "0500000000";
        }

        // Safely resolve foreign keys for CallReceivedById and CallerUserId
        if (dto.CallReceivedById.HasValue && dto.CallReceivedById.Value != Guid.Empty)
        {
            var userExists = await _unitOfWork.Repository<Domain.Entity.User>().ExistsAsync(u => u.Id == dto.CallReceivedById.Value, cancellationToken);
            if (!userExists) log.CallReceivedById = null;
        }
        if (dto.CallerUserId.HasValue && dto.CallerUserId.Value != Guid.Empty)
        {
            var userExists = await _unitOfWork.Repository<Domain.Entity.User>().ExistsAsync(u => u.Id == dto.CallerUserId.Value, cancellationToken);
            if (!userExists) log.CallerUserId = null;
        }

        log.CreatedOn = DateTime.UtcNow;

        await _unitOfWork.CallLogs.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var created = await _unitOfWork.CallLogs.GetByIdAsync(log.Id, cancellationToken);
            if (created != null) return _mapper.Map<CallLogDetailDto>(created);
        }
        catch { }

        return _mapper.Map<CallLogDetailDto>(log);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var log = await _unitOfWork.CallLogs.GetByIdAsync(id, cancellationToken);
        if (log is null)
        {
            throw new NotFoundException(nameof(Domain.Entity.CallLog), id);
        }

        _unitOfWork.CallLogs.Delete(log);
        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
