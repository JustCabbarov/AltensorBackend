using Altensorcrm.Contract.DTOs.Task;

namespace Altensorcrm.Contract.Services.Task;

public interface ITaskService
{
    Task<List<TaskListDto>> GetDepartmentTasksAsync(string departmentName, CancellationToken cancellationToken = default);
    Task<TaskDetailDto> CreateTaskAsync(CreateTaskDto dto, CancellationToken cancellationToken = default);
    Task<bool> ToggleChecklistItemAsync(Guid taskId, Guid checklistItemId, CancellationToken cancellationToken = default);
    Task<bool> UpdateTaskStatusAsync(Guid taskId, bool isCompleted, CancellationToken cancellationToken = default);
}
