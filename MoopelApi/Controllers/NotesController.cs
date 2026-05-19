using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoopelApi.Models;
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
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to retrieve note [{NoteId}] from context[{Context}]", noteId, context);

        Note? note = await _noteService.GetNoteById(noteId);
        if (note is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get note[{NoteId}] which was not found.",
                context, noteId);
            return NotFound();
        }
        if (note.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to get note[{NoteId}] without sufficient permissions.",
                context, noteId);
            return Forbid();
        }

        return Ok(note);
    }

    [HttpPost(Routes.Note.CreateNote)]
    public async Task<ActionResult<Note>> CreateNote([FromBody] NoteCreationRequest request)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to create note from context[{Context}]", context);

        Note createdNote = await _noteService.CreateNote(request, contextUser.UserId);

        return CreatedAtAction(nameof(GetNoteById), new { noteId = createdNote.NoteId }, createdNote);
    }

    [HttpPatch(Routes.Note.UpdateNote)]
    public async Task<ActionResult<Note>> UpdateNote(int noteId, [FromBody] NoteUpdateRequest request)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to update note [{NoteId}] from context[{Context}]", noteId, context);

        Note? noteToBeUpdated = await _noteService.GetNoteById(noteId);
        if (noteToBeUpdated is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to update note[{NoteId}] which was not found.",
                context, noteId);
            return NotFound();
        }
        if (noteToBeUpdated.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to update note[{NoteId}] without sufficient permissions.",
                context, noteId);
            return Forbid();
        }

        Note updatedNote = await _noteService.UpdateNote(request, noteId)
            ?? throw new("Note could note be found, which should have been caught");


        return Ok(updatedNote);
    }

    [HttpDelete(Routes.Note.DeleteNote)]
    public async Task<ActionResult> DeleteNote(int noteId)
    {
        RequestContext context = await _contextService.GetRequestContext(HttpContext);
        User contextUser = await _contextService.GetContextUser(HttpContext);

        _logger.LogInformation("Received request to delete note [{NoteId}] from context[{Context}]", noteId, context);

        Note? noteToBeDeleted = await _noteService.GetNoteById(noteId);
        if (noteToBeDeleted is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to delete note[{NoteId}] which was not found.",
                context, noteId);
            return NotFound();
        }
        if (noteToBeDeleted.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to delete note[{NoteId}] without sufficient permissions.",
                context, noteId);
            return Forbid();
        }

        await _noteService.DeleteNote(noteId);

        return NoContent();
    }
}
