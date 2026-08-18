using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Contract.DTOs.CallLog;

public record CreateCallLogDto(
    CallType Type,
    string ToNumber,
    string FromNumber,
    CallStatus Status,
    int DurationInSeconds,
    Guid? CallReceivedById,
    Guid? CallerUserId,
    Guid? LeadId,
    Guid? DealId
);

public record CallLogDetailDto
{
    public Guid Id { get; init; }
    public CallType Type { get; init; }
    public string ToNumber { get; init; } = default!;
    public string FromNumber { get; init; } = default!;
    public CallStatus Status { get; init; }
    public int DurationInSeconds { get; init; }
    public string FormattedDuration { get; init; } = default!;
    public Guid? CallReceivedById { get; init; }
    public string? CallReceivedByName { get; init; }
    public Guid? CallerUserId { get; init; }
    public string? CallerUserName { get; init; }
    public Guid? LeadId { get; init; }
    public Guid? DealId { get; init; }
    public DateTime CreatedOn { get; init; }
}

public record CallLogListDto
{
    public Guid Id { get; init; }
    public CallType Type { get; init; }
    public string ToNumber { get; init; } = default!;
    public string FromNumber { get; init; } = default!;
    public CallStatus Status { get; init; }
    public string FormattedDuration { get; init; } = default!;
    public DateTime CreatedOn { get; init; }
}
