using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Contract.DTOs.Task;

public record TaskChecklistDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public bool IsDone { get; init; }
}

public record CreateTaskChecklistDto(
    string Title,
    bool IsDone
);

public record CreateTaskDto(
    string Title,
    string Description,
    Priority Priority,
    DateTime DueDate,
    Guid AssignedUserId,
    string? DepartmentName,
    Guid? LeadId,
    Guid? DealId,
    List<CreateTaskChecklistDto>? Checklists
);

public record UpdateTaskDto(
    Guid Id,
    string Title,
    string Description,
    Priority Priority,
    DateTime DueDate,
    bool IsCompleted,
    Guid AssignedUserId,
    string? DepartmentName
);

public record TaskDetailDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public Priority Priority { get; init; }
    public DateTime DueDate { get; init; }
    public bool IsCompleted { get; init; }
    public Guid AssignedUserId { get; init; }
    public string AssignedUserName { get; init; } = default!;
    public string? DepartmentName { get; init; }
    public Guid? LeadId { get; init; }
    public Guid? DealId { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<TaskChecklistDto> Checklists { get; init; } = new();
    public List<CommentDto> Comments { get; init; } = new();
    public List<AttachmentDto> Attachments { get; init; } = new();
}

public record TaskListDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public Priority Priority { get; init; }
    public DateTime DueDate { get; init; }
    public bool IsCompleted { get; init; }
    public string AssignedUserName { get; init; } = default!;
    public string? DepartmentName { get; init; }
}
