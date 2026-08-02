using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Enums;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

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
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.WorkItem.Generate.WorkItemsByGroupId(WorkItemTestData.MissingGroupId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Fact]
    public async Task WorkItemById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.WorkItem.Generate.WorkItemById(WorkItemTestData.MissingWorkItemId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Fact]
    public async Task CreateWorkItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                Routes.WorkItem.Generate.CreateWorkItem(WorkItemTestData.MissingGroupId),
                WorkItemTestData.ValidCreateRequest,
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Fact]
    public async Task UpdateWorkItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PutAsJsonAsync(
                Routes.WorkItem.Generate.UpdateWorkItem(WorkItemTestData.MissingWorkItemId),
                WorkItemTestData.ValidUpdateRequest,
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Fact]
    public async Task DeleteWorkItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.DeleteAsync(Routes.WorkItem.Generate.DeleteWorkItem(WorkItemTestData.MissingWorkItemId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Fact]
    public async Task AddParent_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.AddLink, WorkItemTestData.ValidLinkRequest, _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Fact]
    public async Task RemoveParent_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.RemoveLink, WorkItemTestData.ValidLinkRequest, _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task WorkItemsByGroupId_ReturnsList_WhenAuthenticatedAndGroupExists(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        Group group = await CreateGroupAsync(client, WorkItemTestData.GroupName);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.WorkItem.Generate.WorkItemsByGroupId(group.GroupId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        List<WorkItem>? result = JsonSerializer.Deserialize<List<WorkItem>>(responseBody);
        Assert.True(result is not null, $"Response body could not be deserialized into a List<WorkItem>. Body: {responseBody}");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task WorkItemsByGroupId_ReturnsNotFound_WhenGroupMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.WorkItem.Generate.WorkItemsByGroupId(WorkItemTestData.MissingGroupId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateWorkItem_ReturnsWorkItem_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        Group group = await CreateGroupAsync(client, WorkItemTestData.CreateGroupName);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                Routes.WorkItem.Generate.CreateWorkItem(group.GroupId),
                WorkItemTestData.ValidCreateRequest,
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.Created}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        WorkItem? created = JsonSerializer.Deserialize<WorkItem>(responseBody);
        Assert.True(created is not null, $"Response body could not be deserialized into a WorkItem. Body: {responseBody}");
        Assert.True(created!.ItemId > 0);
        Assert.Equal(WorkItemTestData.WorkItemName, created.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateWorkItem_ReturnsNotFound_WhenGroupMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                Routes.WorkItem.Generate.CreateWorkItem(WorkItemTestData.MissingGroupId),
                WorkItemTestData.MissingGroupCreateRequest,
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task WorkItemById_ReturnsWorkItem_WhenItExists(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        Group group = await CreateGroupAsync(client, WorkItemTestData.ReadGroupName);
        WorkItem created = await CreateWorkItemAsync(client, group.GroupId, WorkItemTestData.ExistingWorkItemName);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.WorkItem.Generate.WorkItemById(created.ItemId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        WorkItem? result = JsonSerializer.Deserialize<WorkItem>(responseBody);
        Assert.True(result is not null, $"Response body could not be deserialized into a WorkItem. Body: {responseBody}");
        Assert.Equal(created.ItemId, result!.ItemId);
        Assert.Equal(created.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task WorkItemById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.WorkItem.Generate.WorkItemById(WorkItemTestData.MissingWorkItemId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateWorkItem_UpdatesWorkItem_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        Group group = await CreateGroupAsync(client, WorkItemTestData.UpdateGroupName);
        WorkItem created = await CreateWorkItemAsync(client, group.GroupId, WorkItemTestData.OldWorkItemName);

        // Act
        HttpResponseMessage response =
            await client.PutAsJsonAsync(
                Routes.WorkItem.Generate.UpdateWorkItem(created.ItemId),
                WorkItemTestData.ValidUpdateRequest,
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        WorkItem? updated = JsonSerializer.Deserialize<WorkItem>(responseBody);
        Assert.True(updated is not null, $"Response body could not be deserialized into a WorkItem. Body: {responseBody}");
        Assert.Equal(created.ItemId, updated!.ItemId);
        Assert.Equal(WorkItemTestData.UpdatedWorkItemName, updated.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateWorkItem_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PutAsJsonAsync(
                Routes.WorkItem.Generate.UpdateWorkItem(WorkItemTestData.MissingWorkItemId),
                WorkItemTestData.MissingWorkItemUpdateRequest,
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteWorkItem_RemovesWorkItem_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        Group group = await CreateGroupAsync(client, WorkItemTestData.DeleteGroupName);
        WorkItem created = await CreateWorkItemAsync(client, group.GroupId, WorkItemTestData.DeleteWorkItemName);

        // Act
        HttpResponseMessage deleteResponse =
            await client.DeleteAsync(Routes.WorkItem.Generate.DeleteWorkItem(created.ItemId), _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync(Routes.WorkItem.Generate.WorkItemById(created.ItemId), _cancelToken);

        // Assert
        string deleteResponseBody = await deleteResponse.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.NoContent}, actual {deleteResponse.StatusCode}, endpoint {deleteResponse.RequestMessage?.RequestUri}, raw body: {deleteResponseBody}");

        string getResponseBody = await getResponse.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(getResponse.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.NotFound}, actual {getResponse.StatusCode}, endpoint {getResponse.RequestMessage?.RequestUri}, raw body: {getResponseBody}");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteWorkItem_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.DeleteAsync(Routes.WorkItem.Generate.DeleteWorkItem(WorkItemTestData.MissingWorkItemId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersWorkItem()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        Group group = await CreateGroupAsync(user1, WorkItemTestData.PrivateGroupName);
        WorkItem created = await CreateWorkItemAsync(user1, group.GroupId, WorkItemTestData.PrivateWorkItemName);

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.WorkItem.Generate.WorkItemById(created.ItemId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Fact]
    public async Task OneUser_CannotList_AnotherUsersGroupsWorkItems()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        Group group = await CreateGroupAsync(user1, WorkItemTestData.PrivateListGroupName);

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.WorkItem.Generate.WorkItemsByGroupId(group.GroupId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task AddParent_AddsParentRelationship_WhenValid(string username, string password)
    {
        // Arrange
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

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.AddLink, request, _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created ||
            response.StatusCode == HttpStatusCode.NoContent,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.Created} or {HttpStatusCode.NoContent}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task AddParent_ReturnsNotFound_WhenWorkItemMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.AddLink, WorkItemTestData.MissingLinkRequest, _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task RemoveParent_RemovesParentRelationship_WhenValid(string username, string password)
    {
        // Arrange
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

        // Act
        HttpResponseMessage addResponse =
            await client.PostAsJsonAsync(Routes.WorkItem.AddLink, addRequest, _cancelToken);

        WorkItemLinkRequest removeRequest = new()
        {
            ParentWorkItemId = parent.ItemId,
            ChildWorkItemId = child.ItemId
        };

        HttpResponseMessage removeResponse =
            await client.PostAsJsonAsync(Routes.WorkItem.RemoveLink, removeRequest, _cancelToken);

        // Assert
        string addResponseBody = await addResponse.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            addResponse.StatusCode == HttpStatusCode.OK ||
            addResponse.StatusCode == HttpStatusCode.Created ||
            addResponse.StatusCode == HttpStatusCode.NoContent,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.Created} or {HttpStatusCode.NoContent}, actual {addResponse.StatusCode}, endpoint {addResponse.RequestMessage?.RequestUri}, raw body: {addResponseBody}");
        string removeResponseBody = await removeResponse.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            removeResponse.StatusCode == HttpStatusCode.OK ||
            removeResponse.StatusCode == HttpStatusCode.NoContent,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.NoContent}, actual {removeResponse.StatusCode}, endpoint {removeResponse.RequestMessage?.RequestUri}, raw body: {removeResponseBody}");
    }

    private async Task<Group> CreateGroupAsync(HttpClient client, string name)
    {
        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                Routes.Group.CreateGroup,
                new GroupCreateRequest { Name = name },
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.Created}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        Group? created = JsonSerializer.Deserialize<Group>(responseBody);
        Assert.True(created is not null, $"Response body could not be deserialized into a Group. Body: {responseBody}");
        return created!;
    }

    private async Task<WorkItem> CreateWorkItemAsync(HttpClient client, int groupId, string title)
    {
        WorkItemCreateRequest request = new()
        {
            Name = title,
            Description = WorkItemTestData.Description,
            Points = WorkItemTestData.Points,
            Type = WorkItemType.Task
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.WorkItem.Generate.CreateWorkItem(groupId), request, _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.Created}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        WorkItem? created = JsonSerializer.Deserialize<WorkItem>(responseBody);
        Assert.True(created is not null, $"Response body could not be deserialized into a WorkItem. Body: {responseBody}");
        return created!;
    }
}
