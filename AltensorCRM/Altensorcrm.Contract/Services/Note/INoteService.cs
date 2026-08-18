using Altensorcrm.Contract.DTOs.Note;

namespace Altensorcrm.Contract.Services.Note;

public interface INoteService
{
    Task<IReadOnlyList<NoteDetailDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<NoteDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NoteDetailDto> CreateAsync(CreateNoteDto dto, CancellationToken cancellationToken = default);
    Task<NoteDetailDto> UpdateAsync(UpdateNoteDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
