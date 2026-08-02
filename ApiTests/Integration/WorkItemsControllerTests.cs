using System.Net;
using System.Net.Http.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Enums;
using MoopelApi.Models.Requests.Creation;

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

        HttpResponseMessage response =
            await client.GetAsync(Routes.WorkItem.Generate.WorkItemsByGroupId(WorkItemTestData.MissingGroupId), _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WorkItemById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response =
            await client.GetAsync(Routes.WorkItem.Generate.WorkItemById(WorkItemTestData.MissingWorkItemId), _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                Routes.WorkItem.Generate.CreateWorkItem(WorkItemTestData.MissingGroupId),
                WorkItemTestData.ValidCreateRequest,
                _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWorkItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response =
            await client.PutAsJsonAsync(
                Routes.WorkItem.Generate.UpdateWorkItem(WorkItemTestData.MissingWorkItemId),
                WorkItemTestData.ValidUpdateRequest,
                _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteWorkItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response =
            await client.DeleteAsync(Routes.WorkItem.Generate.DeleteWorkItem(WorkItemTestData.MissingWorkItemId), _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddParent_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.AddLink, WorkItemTestData.ValidLinkRequest, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RemoveParent_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.RemoveLink, WorkItemTestData.ValidLinkRequest, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WorkItemsByGroupIdAndType_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(
            Routes.WorkItem.Generate.WorkItemsByGroupIdAndType(
                WorkItemTestData.MissingGroupId,
                WorkItemTestData.Type.ToString()),
            _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected {HttpStatusCode.Unauthorized} but got {response.StatusCode} when filtering work items without authentication.");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task WorkItemsByGroupId_ReturnsList_WhenAuthenticatedAndGroupExists(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        Group group = await CreateGroupAsync(client, WorkItemTestData.GroupName);

        HttpResponseMessage response =
            await client.GetAsync(Routes.WorkItem.Generate.WorkItemsByGroupId(group.GroupId), _cancelToken);

        List<WorkItem>? result =
            await response.Content.ReadFromJsonAsync<List<WorkItem>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task WorkItemsByGroupId_ReturnsNotFound_WhenGroupMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.GetAsync(Routes.WorkItem.Generate.WorkItemsByGroupId(WorkItemTestData.MissingGroupId), _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task WorkItemsByGroupIdAndType_ReturnsOnlyMatchingItems_WhenGroupContainsDifferentTypes(
        string username,
        string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        Group group = await CreateGroupAsync(client, WorkItemTestData.FilterGroupName);
        WorkItem task = await CreateWorkItemAsync(
            client,
            group.GroupId,
            WorkItemTestData.FilterTaskWorkItemName,
            WorkItemType.Task);
        WorkItem story = await CreateWorkItemAsync(
            client,
            group.GroupId,
            WorkItemTestData.FilterStoryWorkItemName,
            WorkItemType.Story);

        // Act
        HttpResponseMessage response = await client.GetAsync(
            Routes.WorkItem.Generate.WorkItemsByGroupIdAndType(group.GroupId, WorkItemType.Task.ToString()),
            _cancelToken);
        List<WorkItem>? items = await response.Content.ReadFromJsonAsync<List<WorkItem>>(_cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected {HttpStatusCode.OK} but got {response.StatusCode} when filtering work items by type.");
        Assert.True(items is not null, "Expected a work item list when filtering by an existing type.");
        Assert.True(
            items!.Any(item => item.ItemId == task.ItemId),
            $"Expected filtered work items to include task {task.ItemId}.");
        Assert.True(
            items.All(item => item.ItemId != story.ItemId),
            $"Expected filtered work items to exclude story {story.ItemId}.");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task WorkItemsByGroupIdAndType_ReturnsBadRequest_WhenTypeIsInvalid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(
            Routes.WorkItem.Generate.WorkItemsByGroupIdAndType(
                WorkItemTestData.MissingGroupId,
                WorkItemTestData.InvalidWorkItemType),
            _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected {HttpStatusCode.BadRequest} but got {response.StatusCode} when filtering work items with an invalid type.");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateWorkItem_ReturnsWorkItem_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        Group group = await CreateGroupAsync(client, WorkItemTestData.CreateGroupName);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                Routes.WorkItem.Generate.CreateWorkItem(group.GroupId),
                WorkItemTestData.ValidCreateRequest,
                _cancelToken);

        WorkItem? created =
            await response.Content.ReadFromJsonAsync<WorkItem>(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Assert.NotNull(created);
        Assert.True(created.ItemId > 0);
        Assert.Equal(WorkItemTestData.WorkItemName, created.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateWorkItem_ReturnsNotFound_WhenGroupMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                Routes.WorkItem.Generate.CreateWorkItem(WorkItemTestData.MissingGroupId),
                WorkItemTestData.MissingGroupCreateRequest,
                _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task WorkItemById_ReturnsWorkItem_WhenItExists(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        Group group = await CreateGroupAsync(client, WorkItemTestData.ReadGroupName);
        WorkItem created = await CreateWorkItemAsync(client, group.GroupId, WorkItemTestData.ExistingWorkItemName);

        HttpResponseMessage response =
            await client.GetAsync(Routes.WorkItem.Generate.WorkItemById(created.ItemId), _cancelToken);

        WorkItem? result =
            await response.Content.ReadFromJsonAsync<WorkItem>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.ItemId, result.ItemId);
        Assert.Equal(created.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task WorkItemById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.GetAsync(Routes.WorkItem.Generate.WorkItemById(WorkItemTestData.MissingWorkItemId), _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateWorkItem_UpdatesWorkItem_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        Group group = await CreateGroupAsync(client, WorkItemTestData.UpdateGroupName);
        WorkItem created = await CreateWorkItemAsync(client, group.GroupId, WorkItemTestData.OldWorkItemName);

        HttpResponseMessage response =
            await client.PutAsJsonAsync(
                Routes.WorkItem.Generate.UpdateWorkItem(created.ItemId),
                WorkItemTestData.ValidUpdateRequest,
                _cancelToken);

        WorkItem? updated =
            await response.Content.ReadFromJsonAsync<WorkItem>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.ItemId, updated.ItemId);
        Assert.Equal(WorkItemTestData.UpdatedWorkItemName, updated.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateWorkItem_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.PutAsJsonAsync(
                Routes.WorkItem.Generate.UpdateWorkItem(WorkItemTestData.MissingWorkItemId),
                WorkItemTestData.MissingWorkItemUpdateRequest,
                _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteWorkItem_RemovesWorkItem_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        Group group = await CreateGroupAsync(client, WorkItemTestData.DeleteGroupName);
        WorkItem created = await CreateWorkItemAsync(client, group.GroupId, WorkItemTestData.DeleteWorkItemName);

        HttpResponseMessage deleteResponse =
            await client.DeleteAsync(Routes.WorkItem.Generate.DeleteWorkItem(created.ItemId), _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync(Routes.WorkItem.Generate.WorkItemById(created.ItemId), _cancelToken);

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
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.DeleteAsync(Routes.WorkItem.Generate.DeleteWorkItem(WorkItemTestData.MissingWorkItemId), _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersWorkItem()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        Group group = await CreateGroupAsync(user1, WorkItemTestData.PrivateGroupName);
        WorkItem created = await CreateWorkItemAsync(user1, group.GroupId, WorkItemTestData.PrivateWorkItemName);

        HttpResponseMessage response =
            await user2.GetAsync(Routes.WorkItem.Generate.WorkItemById(created.ItemId), _cancelToken);

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

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        Group group = await CreateGroupAsync(user1, WorkItemTestData.PrivateListGroupName);

        HttpResponseMessage response =
            await user2.GetAsync(Routes.WorkItem.Generate.WorkItemsByGroupId(group.GroupId), _cancelToken);

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
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        Group group = await CreateGroupAsync(client, WorkItemTestData.ParentGroupName);
        WorkItem parent = await CreateWorkItemAsync(client, group.GroupId, WorkItemTestData.ParentWorkItemName);
        WorkItem child = await CreateWorkItemAsync(client, group.GroupId, WorkItemTestData.ChildWorkItemName);

        WorkItemLinkRequest request = new()
        {
            ParentWorkItemId = parent.ItemId,
            ChildWorkItemId = child.ItemId
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
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.AddLink, WorkItemTestData.MissingLinkRequest, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task RemoveParent_RemovesParentRelationship_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        Group group = await CreateGroupAsync(client, WorkItemTestData.RemoveParentGroupName);
        WorkItem parent = await CreateWorkItemAsync(client, group.GroupId, WorkItemTestData.RemoveParentWorkItemName);
        WorkItem child = await CreateWorkItemAsync(client, group.GroupId, WorkItemTestData.RemoveChildWorkItemName);

        WorkItemLinkRequest addRequest = new()
        {
            ParentWorkItemId = parent.ItemId,
            ChildWorkItemId = child.ItemId
        };

        HttpResponseMessage addResponse =
            await client.PostAsJsonAsync(Routes.WorkItem.AddLink, addRequest, _cancelToken);

        Assert.True(
            addResponse.StatusCode == HttpStatusCode.OK ||
            addResponse.StatusCode == HttpStatusCode.Created ||
            addResponse.StatusCode == HttpStatusCode.NoContent);

        WorkItemLinkRequest removeRequest = new()
        {
            ParentWorkItemId = parent.ItemId,
            ChildWorkItemId = child.ItemId
        };

        HttpResponseMessage removeResponse =
            await client.PostAsJsonAsync(Routes.WorkItem.RemoveLink, removeRequest, _cancelToken);

        Assert.True(
            removeResponse.StatusCode == HttpStatusCode.OK ||
            removeResponse.StatusCode == HttpStatusCode.NoContent);
    }

    private async Task<Group> CreateGroupAsync(HttpClient client, string name)
    {
        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                Routes.Group.CreateGroup,
                new GroupCreateRequest { Name = name },
                _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Group? created =
            await response.Content.ReadFromJsonAsync<Group>(_cancelToken);

        Assert.NotNull(created);
        return created;
    }

    private async Task<WorkItem> CreateWorkItemAsync(
        HttpClient client,
        int groupId,
        string title,
        WorkItemType type = WorkItemType.Task)
    {
        WorkItemCreateRequest request = new()
        {
            Name = title,
            Description = WorkItemTestData.Description,
            Points = WorkItemTestData.Points,
            Type = type
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.Generate.CreateWorkItem(groupId), request, _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        WorkItem? created =
            await response.Content.ReadFromJsonAsync<WorkItem>(_cancelToken);

        Assert.NotNull(created);
        return created;
    }
}
