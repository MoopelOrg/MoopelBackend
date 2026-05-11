using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace MoopelApi.Services.Interfaces;

public interface INoteService
{
    public Task<List<Note>> GetUserNotes(int userId);
    public Task<Note?> GetNoteById(int noteId);

    public Task<Note> CreateNote(NoteCreationRequest request, int userId);
    public Task<Note?> UpdateNote(NoteUpdateRequest request, int noteId);

    public Task<bool> DeleteNote(int noteId);
}
