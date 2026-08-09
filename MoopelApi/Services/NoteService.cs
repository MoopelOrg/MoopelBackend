using Microsoft.Data.SqlClient;

using MoopelApi.Helpers;
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

    public async Task<Note> CreateNote(NoteCreateRequest request, int userId)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Creating note for user [{UserId}]", userId);
        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            transaction = connection.BeginTransaction();
            command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"""

                                 INSERT INTO {NotesTable.SelectName}
                                 ({NoteUserIdField.SelectName},
                                  {NoteTypeField.SelectName},
                                  {NoteTitleField.SelectName},
                                  {NoteContentField.SelectName})
                                 {NotesTable.GenerateOutput()}
                                 VALUES
                                 (@UserId, @Type, @Title, @Content)
                             
                 """;
            command.Parameters.Add(NoteUserIdField.GenerateParameter("@UserId", userId));
            command.Parameters.Add(NoteTypeField.GenerateParameter("@Type", request.Type));
            command.Parameters.Add(NoteTitleField.GenerateParameter("@Title", request.Title));
            command.Parameters.Add(NoteContentField.GenerateParameter("@Content", request.Content));

            Note note;
            await using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                {
                    throw new("Could not retrieve inserted note");
                }

                note = DtoReaders.ReadNote(reader);
            }

            await transaction.CommitAsync();

            return note;
        }
        catch (Exception ex)
        {
            if (transaction is not null && transaction.Connection is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating Note",
                ex.Message,
                [
                    ("CreateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task DeleteNote(int noteId)
    {
        _logger.LogInformation("Deleting note [{NoteId}]", noteId);
        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            transaction = connection.BeginTransaction();
            command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"""

                                 DELETE FROM {NotesTable.SelectName}
                                 WHERE {NoteIdField.SelectName} = @NoteId
                             
                 """;
            command.Parameters.Add(NoteIdField.GenerateParameter("@NoteId", noteId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected > 1)
            {
                throw new InvalidOperationException($"Deleted [{rowsAffected}] notes when only one note should have been deleted.");
            }

            await transaction.CommitAsync();
            if (rowsAffected == 0)
            {
                _logger.LogInformation("No note found to delete for [{NoteId}]", noteId);
            }
            else if (rowsAffected > 1)
            {
                throw new InvalidOperationException($"Deleted [{rowsAffected}] notes when only one note should have been deleted.");
            }
        }
        catch (Exception ex)
        {
            if (transaction is not null && transaction.Connection is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Deleting Note",
                ex.Message,
                [
                    ("NoteId", noteId)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<Note?> GetNoteById(int noteId)
    {
        _logger.LogInformation("Getting note [{NoteId}]", noteId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""
                        {NotesTable.GenerateSelect()}
                        WHERE {NoteIdField.SelectName} = @NoteId
                 """;
            command.Parameters.Add(NoteIdField.GenerateParameter("@NoteId", noteId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                Note note = DtoReaders.ReadNote(reader);
                _logger.LogInformation("Found [{Note}] for [{NoteId}]", note, noteId);
                return note;
            }
            else
            {
                _logger.LogInformation("No note found for [{NoteId}]", noteId);
                return null;
            }
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Note",
                ex.Message,
                [
                    ("NoteId", noteId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<List<Note>> GetUserNotes(int userId)
    {
        _logger.LogInformation("Getting notes for user [{UserId}]", userId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
            $"""
                 {NotesTable.GenerateSelect()}
                 WHERE {NoteUserIdField.SelectName} = @UserId
             """;
            command.Parameters.Add(NoteUserIdField.GenerateParameter("@UserId", userId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            List<Note> notes = [];

            while (await reader.ReadAsync())
            {
                notes.Add(DtoReaders.ReadNote(reader));
            }

            if (notes.Count is 0)
            {
                _logger.LogInformation("No notes found for user [{UserId}]", userId);
            }

            return notes;
        }
        catch (Exception ex)
        {
            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting User Notes",
                ex.Message,
                [
                    ("UserId", userId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<Note?> UpdateNote(NoteUpdateRequest request, int noteId)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Updating note [{NoteId}]", noteId);
        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            transaction = connection.BeginTransaction();
            command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"""

                                 UPDATE {NotesTable.SelectName}
                                 SET {NoteTypeField.SelectName} = @Type,
                                     {NoteTitleField.SelectName} = @Title,
                                     {NoteContentField.SelectName} = @Content
                                 {NotesTable.GenerateOutput()}
                                 WHERE {NoteIdField.SelectName} = @NoteId
                             
                 """;
            command.Parameters.Add(NoteIdField.GenerateParameter("@NoteId", noteId));
            command.Parameters.Add(NoteTypeField.GenerateParameter("@Type", request.Type));
            command.Parameters.Add(NoteTitleField.GenerateParameter("@Title", request.Title));
            command.Parameters.Add(NoteContentField.GenerateParameter("@Content", request.Content));

            Note note;
            await using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                {
                    throw new("Could not retrieve inserted note");
                }

                note = DtoReaders.ReadNote(reader);
            }

            return note;
        }
        catch (Exception ex)
        {
            if (transaction is not null && transaction.Connection is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Updating Note",
                ex.Message,
                [
                    ("UpdateRequest", request),
                    ("NoteId", noteId)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }
}
