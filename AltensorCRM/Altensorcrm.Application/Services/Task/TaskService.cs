using AutoMapper;
using Altensorcrm.Application.Exceptions;
using Altensorcrm.Contract.DTOs.Task;
using Altensorcrm.Contract.Services.Task;
using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Repository;

namespace Altensorcrm.Application.Services.Task;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TaskService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<TaskListDto>> GetDepartmentTasksAsync(string departmentName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(departmentName))
        {
            throw new ValidationException("Department Name is required.");
        }

        var tasks = await _unitOfWork.Tasks.FindAsync(
            t => t.DepartmentName != null && t.DepartmentName.ToLower() == departmentName.ToLower(), cancellationToken);

        return _mapper.Map<List<TaskListDto>>(tasks);
    }

    public async Task<TaskDetailDto> CreateTaskAsync(CreateTaskDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ValidationException("Task Title is required.");
        }

        var task = _mapper.Map<TaskItem>(dto);
        task.CreatedAt = DateTime.UtcNow;

        if (dto.Checklists is not null && dto.Checklists.Count > 0)
        {
            foreach (var chkDto in dto.Checklists)
            {
                task.Checklists.Add(new TaskChecklist
                {
                    Id = Guid.NewGuid(),
                    Title = chkDto.Title,
                    IsDone = chkDto.IsDone,
                    TaskItemId = task.Id
                });
            }
        }

        await _unitOfWork.Tasks.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdTask = await _unitOfWork.Tasks.GetByIdAsync(task.Id, cancellationToken);
        return _mapper.Map<TaskDetailDto>(createdTask!);
    }

    public async Task<bool> ToggleChecklistItemAsync(Guid taskId, Guid checklistItemId, CancellationToken cancellationToken = default)
    {
        var checklistRepo = _unitOfWork.Repository<TaskChecklist>();
        var checklistItem = await checklistRepo.GetByIdAsync(checklistItemId, cancellationToken);

        if (checklistItem is null || checklistItem.TaskItemId != taskId)
        {
            throw new NotFoundException(nameof(TaskChecklist), checklistItemId);
        }

        checklistItem.IsDone = !checklistItem.IsDone;
        checklistRepo.Update(checklistItem);

        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public async Task<bool> UpdateTaskStatusAsync(Guid taskId, bool isCompleted, CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId, cancellationToken);
        if (task is null)
        {
            throw new NotFoundException(nameof(TaskItem), taskId);
        }

        task.IsCompleted = isCompleted;
        _unitOfWork.Tasks.Update(task);

        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
