using System.Net;
using System.Net.Http.Json;
using ApiTests.Integration.Generic;
using ApiTests.TestData;
using MoopelApi;

namespace ApiTests.Integration;

public class TagControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public TagControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    [Fact]
    public async Task UserTags_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(Routes.Tag.UserTags, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GroupTags_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("tag/group/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TagById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("tag/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTag_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        CreateTagRequest request = new()
        {
            Name = "Urgent",
            Color = "#FF0000",
            ForNotes = true,
            ForWorkItems = false
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Tag.CreateTag, request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UserTags_ReturnsList_WhenAuthenticated(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync(Routes.Tag.UserTags, _cancelToken);
        List<TagRead>? tags = await response.Content.ReadFromJsonAsync<List<TagRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(tags);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateUserTag_ReturnsTag_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateTagRequest request = new()
        {
            Name = "Urgent",
            Color = "#FF0000",
            ForNotes = true,
            ForWorkItems = false
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Tag.CreateTag, request, _cancelToken);

        TagRead? created = await response.Content.ReadFromJsonAsync<TagRead>(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Assert.NotNull(created);
        Assert.True(created.TagId > 0);
        Assert.Equal("Urgent", created.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task TagById_ReturnsTag_WhenItExists(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateTagRequest createRequest = new()
        {
            Name = "Important",
            Color = "#00FF00",
            ForNotes = true,
            ForWorkItems = true
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Tag.CreateTag, createRequest, _cancelToken);

        TagRead? created = await createResponse.Content.ReadFromJsonAsync<TagRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage response =
            await client.GetAsync($"tag/{created.TagId}", _cancelToken);

        TagRead? result = await response.Content.ReadFromJsonAsync<TagRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.TagId, result.TagId);
        Assert.Equal(created.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task TagById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync("tag/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateTag_UpdatesTag_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateTagRequest createRequest = new()
        {
            Name = "Old Tag",
            Color = "#111111",
            ForNotes = true,
            ForWorkItems = false
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Tag.CreateTag, createRequest, _cancelToken);

        TagRead? created = await createResponse.Content.ReadFromJsonAsync<TagRead>(_cancelToken);

        Assert.NotNull(created);

        UpdateTagRequest updateRequest = new()
        {
            Name = "New Tag",
            Color = "#222222",
            ForNotes = false,
            ForWorkItems = true
        };

        HttpResponseMessage updateResponse =
            await client.PutAsJsonAsync($"tag/update/{created.TagId}", updateRequest, _cancelToken);

        TagRead? updated = await updateResponse.Content.ReadFromJsonAsync<TagRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.TagId, updated.TagId);
        Assert.Equal("New Tag", updated.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateTag_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UpdateTagRequest updateRequest = new()
        {
            Name = "Missing",
            Color = "#333333",
            ForNotes = true,
            ForWorkItems = true
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync("tag/update/999999", updateRequest, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteTag_RemovesTag_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateTagRequest createRequest = new()
        {
            Name = "Delete Me",
            Color = "#444444",
            ForNotes = true,
            ForWorkItems = false
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Tag.CreateTag, createRequest, _cancelToken);

        TagRead? created = await createResponse.Content.ReadFromJsonAsync<TagRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage deleteResponse =
            await client.DeleteAsync($"tag/delete/{created.TagId}", _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync($"tag/{created.TagId}", _cancelToken);

        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteTag_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.DeleteAsync("tag/delete/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersTag()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "taguser1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "taguser2", "Password123!", _cancelToken);

        CreateTagRequest request = new()
        {
            Name = "Private Tag",
            Color = "#555555",
            ForNotes = true,
            ForWorkItems = false
        };

        HttpResponseMessage createResponse =
            await user1.PostAsJsonAsync(Routes.Tag.CreateTag, request, _cancelToken);

        TagRead? created = await createResponse.Content.ReadFromJsonAsync<TagRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage response =
            await user2.GetAsync($"tag/{created.TagId}", _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task GroupTags_ReturnsList_WhenGroupExistsAndUserHasAccess(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateGroupRequest createGroupRequest = new()
        {
            Name = "Tag Group"
        };

        HttpResponseMessage groupResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, createGroupRequest, _cancelToken);

        GroupRead? group = await groupResponse.Content.ReadFromJsonAsync<GroupRead>(_cancelToken);

        Assert.NotNull(group);

        HttpResponseMessage response =
            await client.GetAsync($"tag/group/{group.GroupId}", _cancelToken);

        List<TagRead>? tags = await response.Content.ReadFromJsonAsync<List<TagRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(tags);
    }

    [Fact]
    public async Task GroupTags_DoesNotExpose_AnotherUsersGroupTags()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "grouptag1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "grouptag2", "Password123!", _cancelToken);

        CreateGroupRequest createGroupRequest = new()
        {
            Name = "Private Group"
        };

        HttpResponseMessage groupResponse =
            await user1.PostAsJsonAsync(Routes.Group.CreateGroup, createGroupRequest, _cancelToken);

        GroupRead? group = await groupResponse.Content.ReadFromJsonAsync<GroupRead>(_cancelToken);

        Assert.NotNull(group);

        HttpResponseMessage response =
            await user2.GetAsync($"tag/group/{group.GroupId}", _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }
}