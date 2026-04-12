using System.Net;
using System.Net.Http.Json;
using ApiTests.Integration.Generic;
using ApiTests.TestData;
using MoopelApi;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests;

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

    [Fact]
    public async Task MyNotes_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(Routes.Note.MyNotes, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task NoteById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("note/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateNote_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        CreateNoteRequest request = new()
        {
            Title = "Test Note",
            Content = "Test Content"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Note.CreateNote, request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MyNotes_ReturnsList_WhenAuthenticated(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync(Routes.Note.MyNotes, _cancelToken);
        List<NoteRead>? notes = await response.Content.ReadFromJsonAsync<List<NoteRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(notes);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateNote_ReturnsNote_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateNoteRequest request = new()
        {
            Title = "Groceries",
            Content = "Milk, Bread, Eggs"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Note.CreateNote, request, _cancelToken);

        NoteRead? created = await response.Content.ReadFromJsonAsync<NoteRead>(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Assert.NotNull(created);
        Assert.True(created.NoteId > 0);
        Assert.Equal("Groceries", created.Title);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task NoteById_ReturnsNote_WhenItExists(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateNoteRequest createRequest = new()
        {
            Title = "Todo",
            Content = "Finish homework"
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Note.CreateNote, createRequest, _cancelToken);

        NoteRead? created = await createResponse.Content.ReadFromJsonAsync<NoteRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage response =
            await client.GetAsync($"note/{created.NoteId}", _cancelToken);

        NoteRead? result = await response.Content.ReadFromJsonAsync<NoteRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.NoteId, result.NoteId);
        Assert.Equal(created.Title, result.Title);
        Assert.Equal(created.Content, result.Content);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task NoteById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync("note/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateNote_UpdatesNote_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateNoteRequest createRequest = new()
        {
            Title = "Old Title",
            Content = "Old Content"
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Note.CreateNote, createRequest, _cancelToken);

        NoteRead? created = await createResponse.Content.ReadFromJsonAsync<NoteRead>(_cancelToken);

        Assert.NotNull(created);

        UpdateNoteRequest updateRequest = new()
        {
            Title = "New Title",
            Content = "New Content"
        };

        HttpResponseMessage updateResponse =
            await client.PutAsJsonAsync($"note/update/{created.NoteId}", updateRequest, _cancelToken);

        NoteRead? updated = await updateResponse.Content.ReadFromJsonAsync<NoteRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.NoteId, updated.NoteId);
        Assert.Equal("New Title", updated.Title);
        Assert.Equal("New Content", updated.Content);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateNote_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UpdateNoteRequest updateRequest = new()
        {
            Title = "Missing",
            Content = "Does not exist"
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync("note/update/999999", updateRequest, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteNote_RemovesNote_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateNoteRequest createRequest = new()
        {
            Title = "Delete Me",
            Content = "Temporary note"
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Note.CreateNote, createRequest, _cancelToken);

        NoteRead? created = await createResponse.Content.ReadFromJsonAsync<NoteRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage deleteResponse =
            await client.DeleteAsync($"note/delete/{created.NoteId}", _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync($"note/{created.NoteId}", _cancelToken);

        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteNote_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.DeleteAsync("note/delete/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersNote()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "noteuser1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "noteuser2", "Password123!", _cancelToken);

        CreateNoteRequest request = new()
        {
            Title = "Private Note",
            Content = "User1 only"
        };

        HttpResponseMessage createResponse =
            await user1.PostAsJsonAsync(Routes.Note.CreateNote, request, _cancelToken);

        NoteRead? created = await createResponse.Content.ReadFromJsonAsync<NoteRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage response =
            await user2.GetAsync($"note/{created.NoteId}", _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }
}