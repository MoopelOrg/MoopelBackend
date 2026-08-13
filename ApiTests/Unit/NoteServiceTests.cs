using ApiTests.TestData;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

using MoopelApi.Services;

using MoopelObjects.Dto;
using MoopelObjects.Enums;
using MoopelObjects.Exceptions;

using static MoopelApi.ConstantValues.Database;

namespace ApiTests.Unit;

public class NoteServiceTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public NoteServiceTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    #region GetUserNotes

    [Fact]
    public async Task GetUserNotes_ReturnsNotes_WhenUserHasNotes()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NoteService noteService = ActivatorUtilities.CreateInstance<NoteService>(scope.ServiceProvider);

        int userId = await InsertUserAsync(NoteServiceTestData.PrimaryUsername);
        await noteService.CreateNote(NoteServiceTestData.ValidCreateRequest, userId);

        // Act
        List<Note> notes = await noteService.GetUserNotes(userId);

        // Assert
        Assert.NotEmpty(notes);
        Assert.Contains(notes, note => note.Title == NoteServiceTestData.PrimaryTitle);
    }

    [Fact]
    public async Task GetUserNotes_ReturnsEmptyList_WhenUserHasNoNotes()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NoteService noteService = ActivatorUtilities.CreateInstance<NoteService>(scope.ServiceProvider);

        int userId = await InsertUserAsync(NoteServiceTestData.PrimaryUsername);

        // Act
        List<Note> notes = await noteService.GetUserNotes(userId);

        // Assert
        Assert.Empty(notes);
    }

    #endregion

    #region GetNoteById

    [Fact]
    public async Task GetNoteById_ReturnsNote_WhenItExists()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NoteService noteService = ActivatorUtilities.CreateInstance<NoteService>(scope.ServiceProvider);

        int userId = await InsertUserAsync(NoteServiceTestData.PrimaryUsername);
        Note created = await noteService.CreateNote(NoteServiceTestData.ValidCreateRequest, userId);

        // Act
        Note? note = await noteService.GetNoteById(created.NoteId);

        // Assert
        Assert.NotNull(note);
        Assert.Equal(created.NoteId, note!.NoteId);
        Assert.Equal(created.UserId, note.UserId);
        Assert.Equal(created.Title, note.Title);
        Assert.Equal(created.Content, note.Content);
    }

    [Fact]
    public async Task GetNoteById_ReturnsNull_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NoteService noteService = ActivatorUtilities.CreateInstance<NoteService>(scope.ServiceProvider);

        // Act
        Note? note = await noteService.GetNoteById(NoteServiceTestData.MissingNoteId);

        // Assert
        Assert.Null(note);
    }

    #endregion

    #region CreateNote

    [Fact]
    public async Task CreateNote_ReturnsCreatedNote_WhenValid()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NoteService noteService = ActivatorUtilities.CreateInstance<NoteService>(scope.ServiceProvider);

        int userId = await InsertUserAsync(NoteServiceTestData.PrimaryUsername);

        // Act
        Note note = await noteService.CreateNote(NoteServiceTestData.ValidCreateRequest, userId);

        // Assert
        Assert.True(note.NoteId > 0);
        Assert.Equal(userId, note.UserId);
        Assert.Equal(NoteServiceTestData.ValidCreateRequest.Type, note.Type);
        Assert.Equal(NoteServiceTestData.ValidCreateRequest.Title, note.Title);
        Assert.Equal(NoteServiceTestData.ValidCreateRequest.Content, note.Content);
    }

    #endregion

    #region UpdateNote

    [Fact]
    public async Task UpdateNote_ReturnsUpdatedNote_WhenItExists()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NoteService noteService = ActivatorUtilities.CreateInstance<NoteService>(scope.ServiceProvider);

        int userId = await InsertUserAsync(NoteServiceTestData.PrimaryUsername);
        Note created = await noteService.CreateNote(NoteServiceTestData.ValidCreateRequest, userId);

        // Act
        Note? updated = await noteService.UpdateNote(NoteServiceTestData.ValidUpdateRequest, created.NoteId);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(created.NoteId, updated!.NoteId);
        Assert.Equal(created.UserId, updated.UserId);
        Assert.Equal(NoteServiceTestData.ValidUpdateRequest.Type, updated.Type);
        Assert.Equal(NoteServiceTestData.ValidUpdateRequest.Title, updated.Title);
        Assert.Equal(NoteServiceTestData.ValidUpdateRequest.Content, updated.Content);
    }

    [Fact]
    public async Task UpdateNote_Throws_WhenNoteIsMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NoteService noteService = ActivatorUtilities.CreateInstance<NoteService>(scope.ServiceProvider);

        // Act / Assert
        await Assert.ThrowsAsync<NotFoundException>(() => noteService.UpdateNote(NoteServiceTestData.ValidUpdateRequest, NoteServiceTestData.MissingNoteId));
    }

    #endregion

    #region DeleteNote

    [Fact]
    public async Task DeleteNote_RemovesNote_WhenItExists()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NoteService noteService = ActivatorUtilities.CreateInstance<NoteService>(scope.ServiceProvider);

        int userId = await InsertUserAsync(NoteServiceTestData.PrimaryUsername);
        Note created = await noteService.CreateNote(NoteServiceTestData.ValidCreateRequest, userId);

        // Act
        await noteService.DeleteNote(created.NoteId);
        Note? deleted = await noteService.GetNoteById(created.NoteId);

        // Assert
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteNote_DoesNotThrow_WhenNoteIsMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NoteService noteService = ActivatorUtilities.CreateInstance<NoteService>(scope.ServiceProvider);

        // Act
        await noteService.DeleteNote(NoteServiceTestData.MissingNoteId);

        // Assert
        Assert.True(true);
    }

    #endregion

    private async Task<int> InsertUserAsync(string username)
    {
        await using SqlConnection connection = new(_apiFactory.ConnectionString);
        await connection.OpenAsync(_cancelToken);

        await using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
             INSERT INTO {UsersTable.SelectName}
             (
                 {UserNameField.SelectName},
                 {UserPasswordHashField.SelectName},
                 {UserRoleField.SelectName},
                 {UserDeactivatedField.SelectName},
                 {UserThemeIdField.SelectName}
             )
             OUTPUT INSERTED.{UserIdField.SelectName}
             VALUES
             (
                 @Username,
                 @PasswordHash,
                 @Role,
                 @Deactivated,
                 @ThemeId
             );
             """;
        command.Parameters.Add(UserNameField.GenerateParameter("@Username", username));
        command.Parameters.Add(UserPasswordHashField.GenerateParameter("@PasswordHash", "hash"));
        command.Parameters.Add(UserRoleField.GenerateParameter("@Role", UserRole.Standard.ToString()));
        command.Parameters.Add(UserDeactivatedField.GenerateParameter("@Deactivated", false));
        command.Parameters.Add(UserThemeIdField.GenerateParameter("@ThemeId", null));

        object? insertedUserId = await command.ExecuteScalarAsync(_cancelToken);
        return Convert.ToInt32(insertedUserId);
    }
}
