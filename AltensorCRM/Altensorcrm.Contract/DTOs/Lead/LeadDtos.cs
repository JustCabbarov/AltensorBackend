using Altensorcrm.Contract.DTOs.CallLog;
using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Contract.DTOs.Note;
using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Contract.DTOs.Lead;

public record CreateLeadDto(
    Salutation? Salutation,
    string FirstName,
    string? LastName,
    string Email,
    string MobileNo,
    Gender? Gender,
    string CompanyName,
    string? Website,
    EmployeeCountRange? NoOfEmployees,
    Guid? TerritoryId,
    decimal AnnualRevenue,
    IndustryType? Industry,
    LeadStatus Status,
    Guid? LeadOwnerId
);

public record UpdateLeadDto(
    Guid Id,
    Salutation? Salutation,
    string FirstName,
    string? LastName,
    string Email,
    string MobileNo,
    Gender? Gender,
    string CompanyName,
    string? Website,
    EmployeeCountRange? NoOfEmployees,
    Guid? TerritoryId,
    decimal AnnualRevenue,
    IndustryType? Industry,
    LeadStatus Status,
    Guid? LeadOwnerId
);


public record LeadDetailDto
{
    public Guid Id { get; init; }
    public Salutation? Salutation { get; init; }
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string MobileNo { get; init; } = default!;
    public Gender? Gender { get; init; }
    public string CompanyName { get; init; } = default!;
    public string? Website { get; init; }
    public EmployeeCountRange? NoOfEmployees { get; init; }
    public Guid? TerritoryId { get; init; }
    public string? TerritoryName { get; init; }
    public decimal AnnualRevenue { get; init; }
    public IndustryType? Industry { get; init; }
    public string? IndustryName { get; init; }
    public LeadStatus Status { get; init; }
    public string StatusName { get; init; } = default!;
    public Guid? LeadOwnerId { get; init; }
    public string LeadOwnerName { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
    public List<NoteDetailDto> Notes { get; init; } = new();
    public List<CallLogDetailDto> CallLogs { get; init; } = new();
    public List<CommentDto> Comments { get; init; } = new();
    public List<AttachmentDto> Attachments { get; init; } = new();
}

public record LeadListDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = default!;
    public string CompanyName { get; init; } = default!;
    public LeadStatus Status { get; init; }
    public string StatusName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string MobileNo { get; init; } = default!;
    public string LeadOwnerName { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
}
