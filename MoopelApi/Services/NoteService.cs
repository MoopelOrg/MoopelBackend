using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi.Services;

public class NoteService : INoteService
{
    private readonly ILogger<NoteService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public NoteService(ILogger<NoteService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }

    public Task<Note> CreateNote(NoteCreationRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteNote(int noteId)
    {
        throw new NotImplementedException();
    }

    public Task<Note?> GetNoteById(int noteId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Note>> GetUserNotes(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<Note?> UpdateNote(NoteUpdateRequest request)
    {
        throw new NotImplementedException();
    }
}
