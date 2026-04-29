using System.Net;
using System.Net.Http.Json;
using ApiTests.Integration.Generic;
using ApiTests.TestData;
using MoopelApi;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests;

namespace ApiTests.Integration;

public class WorkItemsControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public WorkItemsControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    [Fact]
    public async Task WorkItemsByGroupId_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("workitem/group/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WorkItemById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("workitem/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        CreateWorkItemRequest request = new()
        {
            GroupId = 1,
            Title = "Test Work Item"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.CreateWorkItem, request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWorkItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        UpdateWorkItemRequest request = new()
        {
            Title = "Updated Work Item"
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync("workitem/update/1", request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteWorkItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response =
            await client.DeleteAsync("workitem/delete/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddParent_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        AddWorkItemParentRequest request = new()
        {
            ParentId = 1,
            ChildId = 2
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.AddLink, request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RemoveParent_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        RemoveWorkItemParentRequest request = new()
        {
            ParentId = 1,
            ChildId = 2
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.RemoveLink, request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task WorkItemsByGroupId_ReturnsList_WhenAuthenticatedAndGroupExists(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        GroupRead group = await CreateGroupAsync(client, "Work Group");

        HttpResponseMessage response =
            await client.GetAsync($"workitem/group/{group.GroupId}", _cancelToken);

        List<WorkItemRead>? result =
            await response.Content.ReadFromJsonAsync<List<WorkItemRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task WorkItemsByGroupId_ReturnsNotFound_WhenGroupMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.GetAsync("workitem/group/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateWorkItem_ReturnsWorkItem_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        GroupRead group = await CreateGroupAsync(client, "Create Work Item Group");

        CreateWorkItemRequest request = new()
        {
            GroupId = group.GroupId,
            Title = "Buy Supplies"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.CreateWorkItem, request, _cancelToken);

        WorkItemRead? created =
            await response.Content.ReadFromJsonAsync<WorkItemRead>(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Assert.NotNull(created);
        Assert.True(created.WorkItemId > 0);
        Assert.Equal("Buy Supplies", created.Title);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateWorkItem_ReturnsNotFound_WhenGroupMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateWorkItemRequest request = new()
        {
            GroupId = 999999,
            Title = "Missing Group Work Item"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.CreateWorkItem, request, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task WorkItemById_ReturnsWorkItem_WhenItExists(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        GroupRead group = await CreateGroupAsync(client, "Get Work Item Group");
        WorkItemRead created = await CreateWorkItemAsync(client, group.GroupId, "Finish Project");

        HttpResponseMessage response =
            await client.GetAsync($"workitem/{created.WorkItemId}", _cancelToken);

        WorkItemRead? result =
            await response.Content.ReadFromJsonAsync<WorkItemRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.WorkItemId, result.WorkItemId);
        Assert.Equal(created.Title, result.Title);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task WorkItemById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.GetAsync("workitem/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateWorkItem_UpdatesWorkItem_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        GroupRead group = await CreateGroupAsync(client, "Update Work Item Group");
        WorkItemRead created = await CreateWorkItemAsync(client, group.GroupId, "Old Title");

        UpdateWorkItemRequest request = new()
        {
            Title = "New Title"
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync($"workitem/update/{created.WorkItemId}", request, _cancelToken);

        WorkItemRead? updated =
            await response.Content.ReadFromJsonAsync<WorkItemRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.WorkItemId, updated.WorkItemId);
        Assert.Equal("New Title", updated.Title);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateWorkItem_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UpdateWorkItemRequest request = new()
        {
            Title = "Missing Item"
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync("workitem/update/999999", request, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteWorkItem_RemovesWorkItem_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        GroupRead group = await CreateGroupAsync(client, "Delete Work Item Group");
        WorkItemRead created = await CreateWorkItemAsync(client, group.GroupId, "Delete Me");

        HttpResponseMessage deleteResponse =
            await client.DeleteAsync($"workitem/delete/{created.WorkItemId}", _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync($"workitem/{created.WorkItemId}", _cancelToken);

        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteWorkItem_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.DeleteAsync("workitem/delete/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersWorkItem()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "workitemuser1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "workitemuser2", "Password123!", _cancelToken);

        GroupRead group = await CreateGroupAsync(user1, "Private Work Group");
        WorkItemRead created = await CreateWorkItemAsync(user1, group.GroupId, "Private Work Item");

        HttpResponseMessage response =
            await user2.GetAsync($"workitem/{created.WorkItemId}", _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OneUser_CannotList_AnotherUsersGroupsWorkItems()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "groupwork1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "groupwork2", "Password123!", _cancelToken);

        GroupRead group = await CreateGroupAsync(user1, "Private Group Items");

        HttpResponseMessage response =
            await user2.GetAsync($"workitem/group/{group.GroupId}", _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task AddParent_AddsParentRelationship_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        GroupRead group = await CreateGroupAsync(client, "Parent Group");
        WorkItemRead parent = await CreateWorkItemAsync(client, group.GroupId, "Parent Item");
        WorkItemRead child = await CreateWorkItemAsync(client, group.GroupId, "Child Item");

        AddWorkItemParentRequest request = new()
        {
            ParentId = parent.WorkItemId,
            ChildId = child.WorkItemId
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.AddLink, request, _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created ||
            response.StatusCode == HttpStatusCode.NoContent);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task AddParent_ReturnsNotFound_WhenWorkItemMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        AddWorkItemParentRequest request = new()
        {
            ParentId = 999998,
            ChildId = 999999
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.AddLink, request, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task RemoveParent_RemovesParentRelationship_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        GroupRead group = await CreateGroupAsync(client, "Remove Parent Group");
        WorkItemRead parent = await CreateWorkItemAsync(client, group.GroupId, "Parent");
        WorkItemRead child = await CreateWorkItemAsync(client, group.GroupId, "Child");

        AddWorkItemParentRequest addRequest = new()
        {
            ParentId = parent.WorkItemId,
            ChildId = child.WorkItemId
        };

        HttpResponseMessage addResponse =
            await client.PostAsJsonAsync(Routes.WorkItem.AddLink, addRequest, _cancelToken);

        Assert.True(
            addResponse.StatusCode == HttpStatusCode.OK ||
            addResponse.StatusCode == HttpStatusCode.Created ||
            addResponse.StatusCode == HttpStatusCode.NoContent);

        RemoveWorkItemParentRequest removeRequest = new()
        {
            ParentId = parent.WorkItemId,
            ChildId = child.WorkItemId
        };

        HttpResponseMessage removeResponse =
            await client.PostAsJsonAsync(Routes.WorkItem.RemoveLink, removeRequest, _cancelToken);

        Assert.True(
            removeResponse.StatusCode == HttpStatusCode.OK ||
            removeResponse.StatusCode == HttpStatusCode.NoContent);
    }

    private async Task<GroupRead> CreateGroupAsync(HttpClient client, string name)
    {
        CreateGroupRequest request = new()
        {
            Name = name
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        GroupRead? created =
            await response.Content.ReadFromJsonAsync<GroupRead>(_cancelToken);

        Assert.NotNull(created);
        return created;
    }

    private async Task<WorkItemRead> CreateWorkItemAsync(HttpClient client, int groupId, string title)
    {
        CreateWorkItemRequest request = new()
        {
            GroupId = groupId,
            Title = title
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.CreateWorkItem, request, _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        WorkItemRead? created =
            await response.Content.ReadFromJsonAsync<WorkItemRead>(_cancelToken);

        Assert.NotNull(created);
        return created;
    }
}