using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Controllers;

[Authorize]
[ApiController]
public class NotesController : ControllerBase
{
    private readonly ILogger<NotesController> _logger;
    private readonly IContextService _contextService;
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly INoteService _noteService;

    public NotesController(ILogger<NotesController> logger,
                           IContextService contextService,
                           IAuthService authService,
                           IUserService userService,
                           INoteService noteService)
    {
        _logger = logger;
        _authService = authService;
        _userService = userService;
        _noteService = noteService;
        _contextService = contextService;
    }

    [HttpGet(Routes.Note.MyNotes)]
    public async Task<ActionResult<List<Note>>> GetMyNotes()
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("User {UserId} requested their notes", contextUser.UserId);

        List<Note> notes = await _noteService.GetUserNotes(contextUser.UserId);

        return Ok(notes);
    }

    [HttpGet(Routes.Note.NoteById)]
    public async Task<ActionResult<Note>> GetNoteById(int noteId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} requested note {NoteId}", contextUser.UserId, noteId);

        Note? note = await _noteService.GetNoteById(noteId);
        if (note is null)
        {
            _logger.LogWarning("Note {NoteId} not found for user {UserId}", noteId, contextUser.UserId);
            return NotFound();
        }
        if (note.UserId != contextUser.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to access note {NoteId} which they do not own", contextUser.UserId, noteId);
            return Forbid();
        }

        return Ok(note);
    }

    [HttpPost(Routes.Note.CreateNote)]
    public async Task<ActionResult<Note>> CreateNote([FromBody] NoteCreationRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} is creating a new note", contextUser.UserId);

        Note createdNote = await _noteService.CreateNote(request);

        return CreatedAtAction(nameof(GetNoteById), new { noteId = createdNote.NoteId }, createdNote);
    }

    [HttpPatch(Routes.Note.UpdateNote)]
    public async Task<ActionResult<Note>> UpdateNote([FromBody] NoteUpdateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} is updating note {NoteId}", contextUser.UserId, request.NoteId);

        Note? noteToBeUpdated = await _noteService.GetNoteById(request.NoteId);
        if (noteToBeUpdated is null)
        {
            _logger.LogWarning("Note {NoteId} not found for user {UserId}", request.NoteId, contextUser.UserId);
            return NotFound();
        }
        if (noteToBeUpdated.UserId != contextUser.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to update note {NoteId} which they do not own", contextUser.UserId, request.NoteId);
            return Forbid();
        }

        Note? updatedNote = await _noteService.UpdateNote(request);
        if (updatedNote is null)
        {
            _logger.LogCritical("Note could note be found, which should have been caught");
            return NotFound();
        }


        return Ok(updatedNote);
    }

    [HttpDelete(Routes.Note.DeleteNote)]
    public async Task<ActionResult> DeleteNote(int noteId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} is deleting note {NoteId}", contextUser.UserId, noteId);

        Note? noteToBeDeleted = await _noteService.GetNoteById(noteId);
        if (noteToBeDeleted is null)
        {
            _logger.LogWarning("Note {NoteId} not found for user {UserId}", noteId, contextUser.UserId);
            return NotFound();
        }
        if (noteToBeDeleted.UserId != contextUser.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to delete note {NoteId} which they do not own", contextUser.UserId, noteId);
            return Forbid();
        }

        bool deleted = await _noteService.DeleteNote(noteId);
        if (!deleted)
        {
            _logger.LogError("Failed to delete note {NoteId} for user {UserId}", noteId, contextUser.UserId);
            return StatusCode(500, "Failed to delete the note");
        }

        return NoContent();
    }
}
