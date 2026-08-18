using System;

namespace Altensorcrm.Contract.DTOs.EmailTemplate;

public record CreateEmailTemplateDto(
    string Name,
    string ForType,
    string Subject,
    string ContentType,
    string Content,
    bool Enabled = true
);

public record UpdateEmailTemplateDto(
    Guid Id,
    string Name,
    string ForType,
    string Subject,
    string ContentType,
    string Content,
    bool Enabled
);

public record EmailTemplateDetailDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = default!;
    public string ForType { get; init; } = default!;
    public string Subject { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public string Content { get; init; } = default!;
    public bool Enabled { get; init; }
    public DateTime CreatedAt { get; init; }
}

