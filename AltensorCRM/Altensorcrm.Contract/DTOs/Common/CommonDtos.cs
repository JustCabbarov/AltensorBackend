using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Contract.DTOs.Common;

public record AddressDto
{
    public Guid Id { get; init; }
    public string AddressTitle { get; init; } = default!;
    public AddressType AddressType { get; init; }
    public string AddressLine1 { get; init; } = default!;
    public string? AddressLine2 { get; init; }
    public string Country { get; init; } = default!;
    public string? StateProvince { get; init; }
    public string CityTown { get; init; } = default!;
    public string? PostalCode { get; init; }
}

public record CreateAddressDto(
    string AddressTitle,
    AddressType AddressType,
    string AddressLine1,
    string? AddressLine2,
    string Country,
    string? StateProvince,
    string CityTown,
    string? PostalCode
);

public record TerritoryDto
{
    public Guid Id { get; init; }
    public string TerritoryName { get; init; } = default!;
    public Guid? TerritoryManagerId { get; init; }
    public string? TerritoryManagerName { get; init; }
    public Guid? ParentTerritoryId { get; init; }
    public string? ParentTerritoryName { get; init; }
    public bool IsGroup { get; init; }
}

public record CreateTerritoryDto(
    string TerritoryName,
    Guid? TerritoryManagerId,
    Guid? ParentTerritoryId,
    bool IsGroup
);

public record CommentDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = default!;
}

public record CreateCommentDto(
    string Text,
    Guid AuthorId,
    Guid? LeadId,
    Guid? DealId,
    Guid? TaskItemId
);

public record AttachmentDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = default!;
    public string FilePath { get; init; } = default!;
    public long FileSize { get; init; }
    public DateTime UploadedAt { get; init; }
}
