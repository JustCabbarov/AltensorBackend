namespace Altensorcrm.Contract.DTOs.Note;

public record CreateNoteDto(
    string Title,
    string Content,
    Guid? CreatedById,
    Guid? LeadId,
    Guid? DealId
);

public record UpdateNoteDto(
    Guid Id,
    string Title,
    string Content
);

public record NoteDetailDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public string Content { get; init; } = default!;
    public Guid? CreatedById { get; init; }
    public string? CreatedByName { get; init; }
    public Guid? LeadId { get; init; }
    public Guid? DealId { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record NoteListDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public string? CreatedByName { get; init; }
    public DateTime CreatedAt { get; init; }
}
