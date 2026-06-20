using System.Net;
using System.Net.Http.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Attributes;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace ApiTests.Integration;

public class NoteControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public NoteControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    #region Get My Notes

    [Fact]
    [TestDocumentation("Get 'my' notes should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task MyNotes_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Note.MyNotes, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    [TestDocumentation("Get 'my' notes should return an empty list when user is authenticated", HttpStatusCode.OK)]
    public async Task MyNotes_ReturnsList_WhenAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Note.MyNotes, _cancelToken);
        List<Note>? notes = await response.Content.ReadFromJsonAsync<List<Note>>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(notes);
        Assert.Empty(notes);
    }

    [Fact]
    [TestDocumentation("Get 'my' notes should return created notes", HttpStatusCode.OK)]
    public async Task MyNotes_ReturnsListWithCreatedItem_AfterCreating_WhenAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        NoteCreateRequest request = NoteTestData.ValidCreateRequest;

        // Act
        await client.PostAsJsonAsync(Routes.Note.CreateNote, request, _cancelToken);
        HttpResponseMessage response = await client.GetAsync(Routes.Note.MyNotes, _cancelToken);
        List<Note>? notes = await response.Content.ReadFromJsonAsync<List<Note>>(_cancelToken);
        Note? note = notes?.FirstOrDefault();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(notes);
        Assert.NotEmpty(notes);

        Assert.NotNull(note);
        Assert.Equal(request.Type, note.Type);
        Assert.Equal(request.Title, note.Title);
        Assert.Equal(request.Content, note.Content);
    }

    #endregion

    #region Get Note By Id

    [Fact]
    [TestDocumentation("Get note by id should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task NoteById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        string endpoint = Routes.Note.Generate.NoteById(NoteTestData.MissingNoteId);

        // Act
        HttpResponseMessage response = await client.GetAsync(endpoint, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    [TestDocumentation("Get note by id should return the note when it exists and belongs to the user", HttpStatusCode.OK)]
    public async Task NoteById_ReturnsNote_WhenItExists()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        NoteCreateRequest createRequest = NoteTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Note.CreateNote, createRequest, _cancelToken);

        Note? created = await createResponse.Content.ReadFromJsonAsync<Note>(_cancelToken);

        Assert.NotNull(created);

        string endpoint = Routes.Note.Generate.NoteById(created.NoteId);
        HttpResponseMessage response = await client.GetAsync(endpoint, _cancelToken);

        Note? result = await response.Content.ReadFromJsonAsync<Note>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.NoteId, result.NoteId);
        Assert.Equal(created.Type, result.Type);
        Assert.Equal(created.Title, result.Title);
        Assert.Equal(created.Content, result.Content);
    }

    [Fact]
    [TestDocumentation("Get note by id should return not found when the note does not exist", HttpStatusCode.NotFound)]
    public async Task NoteById_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);
        string endpoint = Routes.Note.Generate.NoteById(NoteTestData.MissingNoteId);

        // Act
        HttpResponseMessage response = await client.GetAsync(endpoint, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    [TestDocumentation("One user should not be able to retrieve another user's note", HttpStatusCode.Forbidden)]
    public async Task OneUser_CannotAccess_AnotherUsersNote()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, _cancelToken);

        NoteCreateRequest request = NoteTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage createResponse =
            await user1.PostAsJsonAsync(Routes.Note.CreateNote, request, _cancelToken);

        Note? created = await createResponse.Content.ReadFromJsonAsync<Note>(_cancelToken);

        Assert.NotNull(created);

        string endpoint = Routes.Note.Generate.NoteById(created.NoteId);
        HttpResponseMessage response = await user2.GetAsync(endpoint, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Create Note

    [Fact]
    [TestDocumentation("Create note should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task CreateNote_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        NoteCreateRequest request = NoteTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Note.CreateNote, request, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [TestDocumentation("Create note should return created note when request is valid", HttpStatusCode.Created)]
    [MemberData(nameof(NoteTestData.ValidNoteCreateRequests), MemberType = typeof(NoteTestData))]
    public async Task CreateNote_ReturnsNote_WhenValid(NoteCreateRequest request)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Note.CreateNote, request, _cancelToken);

        Note? created = await response.Content.ReadFromJsonAsync<Note>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.NotNull(request);
        Assert.True(created.NoteId > 0);
        Assert.Equal(request.Type, created.Type);
        Assert.Equal(request.Title, created.Title);
        Assert.Equal(request.Content, created.Content);
    }

    #endregion

    #region Update Note

    [Fact]
    [TestDocumentation("Update note should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task UpdateNote_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        NoteUpdateRequest updateRequest = NoteTestData.ValidUpdateRequest;
        string endpoint = Routes.Note.Generate.UpdateNote(NoteTestData.MissingNoteId);

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(endpoint, updateRequest, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [TestDocumentation("Update note should return updated note when request is valid", HttpStatusCode.OK)]
    [MemberData(nameof(NoteTestData.ValidNoteUpdateCreateRequests), MemberType = typeof(NoteTestData))]
    public async Task UpdateNote_UpdatesNote_WhenValid(NoteCreateRequest createRequest, NoteUpdateRequest updateRequest)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Note.CreateNote, createRequest, _cancelToken);

        Note? created = await createResponse.Content.ReadFromJsonAsync<Note>(_cancelToken);

        Assert.NotNull(created);

        string updateEndpoint = Routes.Note.Generate.UpdateNote(created.NoteId);
        HttpResponseMessage updateResponse =
            await client.PatchAsJsonAsync(updateEndpoint, updateRequest, _cancelToken);

        Note? updated = await updateResponse.Content.ReadFromJsonAsync<Note>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.NotNull(updateRequest);
        Assert.NotNull(createRequest);
        Assert.Equal(created.NoteId, updated.NoteId);
        Assert.Equal(updateRequest.Type, updated.Type);
        Assert.Equal(updateRequest.Title, updated.Title);
        Assert.Equal(updateRequest.Content, updated.Content);
    }

    [Fact]
    [TestDocumentation("Update note should return not found when the note does not exist", HttpStatusCode.NotFound)]
    public async Task UpdateNote_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        NoteUpdateRequest updateRequest = NoteTestData.ValidUpdateRequest;
        string endpoint = Routes.Note.Generate.UpdateNote(NoteTestData.MissingNoteId);

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(endpoint, updateRequest, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Delete Note

    [Fact]
    [TestDocumentation("Delete note should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task DeleteNote_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        string endpoint = Routes.Note.Generate.DeleteNote(NoteTestData.MissingNoteId);

        // Act
        HttpResponseMessage response = await client.DeleteAsync(endpoint, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [TestDocumentation("Delete note should remove the note when it exists and belongs to the user", HttpStatusCode.NoContent)]
    [MemberData(nameof(NoteTestData.ValidNoteCreateRequests), MemberType = typeof(NoteTestData))]
    public async Task DeleteNote_RemovesNote_WhenValid(NoteCreateRequest createRequest)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Note.CreateNote, createRequest, _cancelToken);

        Note? created = await createResponse.Content.ReadFromJsonAsync<Note>(_cancelToken);

        Assert.NotNull(created);

        string deleteEndpoint = Routes.Note.Generate.DeleteNote(created.NoteId);
        HttpResponseMessage deleteResponse = await client.DeleteAsync(deleteEndpoint, _cancelToken);

        string getEndpoint = Routes.Note.Generate.NoteById(created.NoteId);
        HttpResponseMessage getResponse = await client.GetAsync(getEndpoint, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    [TestDocumentation("Delete note should return not found when the note does not exist", HttpStatusCode.NotFound)]
    public async Task DeleteNote_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);
        string endpoint = Routes.Note.Generate.DeleteNote(NoteTestData.MissingNoteId);

        // Act
        HttpResponseMessage response = await client.DeleteAsync(endpoint, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
}
