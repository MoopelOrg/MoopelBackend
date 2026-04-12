using System.Net;
using System.Net.Http.Json;
using ApiTests.Integration.Generic;
using ApiTests.TestData;
using MoopelApi;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests;

namespace ApiTests.Integration;

public class GroupsControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public GroupsControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    [Fact]
    public async Task MyGroups_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(Routes.Group.MyGroups, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GroupById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("group/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateGroup_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        CreateGroupRequest request = new()
        {
            Name = "Test Group"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MyGroups_ReturnsList_WhenAuthenticated(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync(Routes.Group.MyGroups, _cancelToken);
        List<GroupRead>? groups = await response.Content.ReadFromJsonAsync<List<GroupRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(groups);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateGroup_ReturnsGroup_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateGroupRequest request = new()
        {
            Name = "Chores"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);

        GroupRead? created = await response.Content.ReadFromJsonAsync<GroupRead>(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Assert.NotNull(created);
        Assert.True(created.GroupId > 0);
        Assert.Equal("Chores", created.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task GroupById_ReturnsGroup_WhenItExists(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateGroupRequest createRequest = new()
        {
            Name = "Projects"
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, createRequest, _cancelToken);

        GroupRead? created = await createResponse.Content.ReadFromJsonAsync<GroupRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage response =
            await client.GetAsync($"group/{created.GroupId}", _cancelToken);

        GroupRead? result = await response.Content.ReadFromJsonAsync<GroupRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.GroupId, result.GroupId);
        Assert.Equal(created.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task GroupById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync("group/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateGroup_UpdatesGroup_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateGroupRequest createRequest = new()
        {
            Name = "Old Name"
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, createRequest, _cancelToken);

        GroupRead? created = await createResponse.Content.ReadFromJsonAsync<GroupRead>(_cancelToken);

        Assert.NotNull(created);

        UpdateGroupRequest updateRequest = new()
        {
            Name = "New Name"
        };

        HttpResponseMessage updateResponse =
            await client.PutAsJsonAsync($"group/update/{created.GroupId}", updateRequest, _cancelToken);

        GroupRead? updated = await updateResponse.Content.ReadFromJsonAsync<GroupRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.GroupId, updated.GroupId);
        Assert.Equal("New Name", updated.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateGroup_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UpdateGroupRequest updateRequest = new()
        {
            Name = "Missing Group"
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync("group/update/999999", updateRequest, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteGroup_RemovesGroup_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateGroupRequest createRequest = new()
        {
            Name = "Delete Me"
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, createRequest, _cancelToken);

        GroupRead? created = await createResponse.Content.ReadFromJsonAsync<GroupRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage deleteResponse =
            await client.DeleteAsync($"group/delete/{created.GroupId}", _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync($"group/{created.GroupId}", _cancelToken);

        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteGroup_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.DeleteAsync("group/delete/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GroupById_DoesNotExpose_AnotherUsersGroup()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "groupuser1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "groupuser2", "Password123!", _cancelToken);

        CreateGroupRequest request = new()
        {
            Name = "Private Group"
        };

        HttpResponseMessage createResponse =
            await user1.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);

        GroupRead? created = await createResponse.Content.ReadFromJsonAsync<GroupRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage response =
            await user2.GetAsync($"group/{created.GroupId}", _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateGroup_Fails_WhenCreatingDuplicateGroupNameForSameUser(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateGroupRequest request = new()
        {
            Name = "Duplicate Name"
        };

        HttpResponseMessage firstResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);

        HttpResponseMessage secondResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);

        Assert.True(
            firstResponse.StatusCode == HttpStatusCode.OK ||
            firstResponse.StatusCode == HttpStatusCode.Created);

        Assert.True(
            secondResponse.StatusCode == HttpStatusCode.BadRequest ||
            secondResponse.StatusCode == HttpStatusCode.Conflict);
    }
}