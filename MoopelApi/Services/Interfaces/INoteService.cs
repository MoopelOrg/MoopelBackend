using MoopelObjects.Dto;
using MoopelObjects.Requests.Creation;
using MoopelObjects.Requests.Update;

namespace MoopelApi.Services.Interfaces;

public interface INoteService
{
    public Task<List<Note>> GetUserNotes(int userId);
    public Task<Note?> GetNoteById(int noteId);

    public Task<Note> CreateNote(NoteCreateRequest request, int userId);
    public Task<Note?> UpdateNote(NoteUpdateRequest request, int noteId);

    public Task DeleteNote(int noteId);
}
