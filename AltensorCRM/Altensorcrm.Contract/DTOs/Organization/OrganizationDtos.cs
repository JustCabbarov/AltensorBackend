using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Contract.DTOs.Organization;

public record CreateOrganizationDto(
    string? OrganizationName = null,
    decimal AnnualRevenue = 0,
    string? Website = null,
    Guid? TerritoryId = null,
    EmployeeCountRange? NoOfEmployees = null,
    IndustryType? Industry = null,
    Guid? AddressId = null,
    CreateAddressDto? Address = null
);

public record UpdateOrganizationDto(
    Guid Id,
    string? OrganizationName = null,
    decimal AnnualRevenue = 0,
    string? Website = null,
    Guid? TerritoryId = null,
    EmployeeCountRange? NoOfEmployees = null,
    IndustryType? Industry = null,
    Guid? AddressId = null,
    CreateAddressDto? Address = null
);

public record OrganizationDetailDto
{
    public Guid Id { get; init; }
    public string OrganizationName { get; init; } = default!;
    public decimal AnnualRevenue { get; init; }
    public string? Website { get; init; }
    public Guid? TerritoryId { get; init; }
    public string? TerritoryName { get; init; }
    public EmployeeCountRange? NoOfEmployees { get; init; }
    public IndustryType? Industry { get; init; }
    public Guid? AddressId { get; init; }
    public AddressDto? Address { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record OrganizationListDto
{
    public Guid Id { get; init; }
    public string OrganizationName { get; init; } = default!;
    public decimal AnnualRevenue { get; init; }
    public string? Website { get; init; }
    public string? TerritoryName { get; init; }
    public IndustryType? Industry { get; init; }
}
