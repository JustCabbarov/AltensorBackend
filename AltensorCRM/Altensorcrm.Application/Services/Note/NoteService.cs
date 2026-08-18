using AutoMapper;
using Altensorcrm.Application.Exceptions;
using Altensorcrm.Contract.DTOs.Note;
using Altensorcrm.Contract.Services.Note;
using Altensorcrm.Domain.Repository;

namespace Altensorcrm.Application.Services.Note;

public class NoteService : INoteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public NoteService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<NoteDetailDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var notes = await _unitOfWork.Notes.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<NoteDetailDto>>(notes);
    }

    public async Task<NoteDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await _unitOfWork.Notes.GetByIdAsync(id, cancellationToken);
        if (note is null)
        {
            throw new NotFoundException(nameof(Domain.Entity.Note), id);
        }

        return _mapper.Map<NoteDetailDto>(note);
    }

    public async Task<NoteDetailDto> CreateAsync(CreateNoteDto dto, CancellationToken cancellationToken = default)
    {
        var note = _mapper.Map<Domain.Entity.Note>(dto);
        if (string.IsNullOrWhiteSpace(note.Title))
        {
            note.Title = "Untitled Note";
        }
        if (string.IsNullOrWhiteSpace(note.Content))
        {
            note.Content = string.Empty;
        }

        // Safely resolve CreatedById foreign key constraint
        if (!dto.CreatedById.HasValue || dto.CreatedById.Value == Guid.Empty)
        {
            var firstUser = (await _unitOfWork.Repository<Domain.Entity.User>().GetAllAsync(cancellationToken)).FirstOrDefault();
            note.CreatedById = firstUser?.Id;
        }
        else
        {
            var userExists = await _unitOfWork.Repository<Domain.Entity.User>().ExistsAsync(u => u.Id == dto.CreatedById.Value, cancellationToken);
            if (!userExists)
            {
                var firstUser = (await _unitOfWork.Repository<Domain.Entity.User>().GetAllAsync(cancellationToken)).FirstOrDefault();
                note.CreatedById = firstUser?.Id;
            }
        }

        note.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Notes.AddAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var created = await _unitOfWork.Notes.GetByIdAsync(note.Id, cancellationToken);
            if (created != null) return _mapper.Map<NoteDetailDto>(created);
        }
        catch { }

        return _mapper.Map<NoteDetailDto>(note);
    }

    public async Task<NoteDetailDto> UpdateAsync(UpdateNoteDto dto, CancellationToken cancellationToken = default)
    {
        var note = await _unitOfWork.Notes.GetByIdAsync(dto.Id, cancellationToken);
        if (note is null)
        {
            throw new NotFoundException(nameof(Domain.Entity.Note), dto.Id);
        }

        _mapper.Map(dto, note);
        _unitOfWork.Notes.Update(note);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(dto.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await _unitOfWork.Notes.GetByIdAsync(id, cancellationToken);
        if (note is null)
        {
            throw new NotFoundException(nameof(Domain.Entity.Note), id);
        }

        _unitOfWork.Notes.Delete(note);
        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
