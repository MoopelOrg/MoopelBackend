using System.Net;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

using static ApiTests.TestHelper;

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

    #region Get My Groups

    [Fact]
    public async Task MyGroups_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Group.MyGroups, _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MyGroups_ReturnsList_WhenAuthenticated(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Group.MyGroups, _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        List<Group>? groups = await response.Content.ReadFromJsonAsync<List<Group>>(_cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, response));
        Assert.True(groups is not null, $"Response body could not be deserialized into a list. Body: {responseBody}");
    }

    #endregion

    #region Get Group By Id

    [Fact]
    public async Task GroupById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Group.Generate.GroupById(1), _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task GroupById_ReturnsGroup_WhenItExists(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        GroupCreateRequest createRequest = GroupTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, createRequest, _cancelToken);

        string createBody = await createResponse.Content.ReadAsStringAsync(_cancelToken);
        Group? created = await createResponse.Content.ReadFromJsonAsync<Group>(_cancelToken);

        Assert.True(created is not null, $"Failed to create group. Status: {createResponse.StatusCode}, Body: {createBody}");

        HttpResponseMessage response =
            await client.GetAsync(Routes.Group.Generate.GroupById(created.GroupId), _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Group? result = await response.Content.ReadFromJsonAsync<Group>(_cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, response));
        Assert.True(result is not null, $"Response body could not be deserialized into a Group. Body: {responseBody}");
        Assert.Equal(created.GroupId, result.GroupId);
        Assert.Equal(created.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task GroupById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Group.Generate.GroupById(9999), _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
    }

    [Fact]
    public async Task GroupById_DoesNotExpose_AnotherUsersGroup()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        GroupCreateRequest request = GroupTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage createResponse =
            await user1.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);

        string createBody = await createResponse.Content.ReadAsStringAsync(_cancelToken);
        Group? created = await createResponse.Content.ReadFromJsonAsync<Group>(_cancelToken);

        Assert.True(created is not null, $"Failed to create group for user1. Status: {createResponse.StatusCode}, Body: {createBody}");

        HttpResponseMessage response =
            await user2.GetAsync(Routes.Group.Generate.GroupById(created.GroupId), _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound} but got {response.StatusCode}. Body: {responseBody}");
    }

    #endregion

    #region Create Group

    [Fact]
    public async Task CreateGroup_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        GroupCreateRequest request = GroupTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    [Theory]
    [MemberData(nameof(GroupTestData.ValidGroupCreateRequests), MemberType = typeof(GroupTestData))]
    public async Task CreateGroup_ReturnsGroup_WhenValid(GroupCreateRequest request)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);

        Group? created = await response.Content.ReadFromJsonAsync<Group>(_cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created,
            $"Expected {HttpStatusCode.OK} or {HttpStatusCode.Created} but got {response.StatusCode}. Body: {responseBody}");

        Assert.True(created is not null, $"Response body could not be deserialized into a Group. Body: {responseBody}");
        Assert.NotNull(request);
        Assert.True(created.GroupId > 0, $"Expected a positive GroupId but got {created.GroupId}");
        Assert.Equal(request.Name, created.Name);
    }

    [Theory]
    [MemberData(nameof(GroupTestData.ValidGroupCreateRequests), MemberType = typeof(GroupTestData))]
    public async Task CreateGroup_Fails_WhenCreatingDuplicateGroupNameForSameUser(GroupCreateRequest request)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage firstResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);
        string firstBody = await firstResponse.Content.ReadAsStringAsync(_cancelToken);

        HttpResponseMessage secondResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);
        string secondBody = await secondResponse.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            firstResponse.StatusCode == HttpStatusCode.OK || firstResponse.StatusCode == HttpStatusCode.Created,
            $"Expected first creation to succeed but got {firstResponse.StatusCode}. Body: {firstBody}");

        Assert.True(
            secondResponse.StatusCode == HttpStatusCode.BadRequest || secondResponse.StatusCode == HttpStatusCode.Conflict,
            $"Expected duplicate creation to fail with BadRequest or Conflict but got {secondResponse.StatusCode}. Body: {secondBody}");
    }

    #endregion

    #region Update Group

    [Theory]
    [MemberData(nameof(GroupTestData.ValidGroupUpdateCreateRequests), MemberType = typeof(GroupTestData))]
    public async Task UpdateGroup_UpdatesGroup_WhenValid(GroupCreateRequest createRequest, GroupUpdateRequest updateRequest)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, createRequest, _cancelToken);

        string createBody = await createResponse.Content.ReadAsStringAsync(_cancelToken);
        Group? created = await createResponse.Content.ReadFromJsonAsync<Group>(_cancelToken);

        Assert.True(created is not null, $"Failed to create group. Status: {createResponse.StatusCode}, Body: {createBody}");

        // Act
        HttpResponseMessage updateResponse =
            await client.PutAsJsonAsync(Routes.Group.Generate.UpdateGroup(created.GroupId), updateRequest, _cancelToken);
        string updateBody = await updateResponse.Content.ReadAsStringAsync(_cancelToken);
        Group? updated = await updateResponse.Content.ReadFromJsonAsync<Group>(_cancelToken);

        // Assert
        Assert.True(
            condition: updateResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, updateResponse));
        Assert.True(updated is not null, $"Response body could not be deserialized into a Group. Body: {updateBody}");
        Assert.NotNull(createRequest);
        Assert.NotNull(updateRequest);
        Assert.Equal(created.GroupId, updated.GroupId);
        Assert.Equal(updateRequest.Name, updated.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateGroup_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        GroupUpdateRequest updateRequest = GroupTestData.ValidUpdateRequest;

        // Act
        HttpResponseMessage response =
            await client.PutAsJsonAsync(Routes.Group.Generate.UpdateGroup(9999), updateRequest, _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
    }

    #endregion

    #region Delete Group

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteGroup_RemovesGroup_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        GroupCreateRequest createRequest = GroupTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, createRequest, _cancelToken);

        string createBody = await createResponse.Content.ReadAsStringAsync(_cancelToken);
        Group? created = await createResponse.Content.ReadFromJsonAsync<Group>(_cancelToken);

        Assert.True(created is not null, $"Failed to create group. Status: {createResponse.StatusCode}, Body: {createBody}");

        // Act
        HttpResponseMessage deleteResponse =
            await client.DeleteAsync(Routes.Group.Generate.DeleteGroup(created.GroupId), _cancelToken);
        string deleteBody = await deleteResponse.Content.ReadAsStringAsync(_cancelToken);
        HttpResponseMessage getResponse =
            await client.GetAsync(Routes.Group.Generate.GroupById(created.GroupId), _cancelToken);

        // Assert
        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK || deleteResponse.StatusCode == HttpStatusCode.NoContent,
            $"Expected {HttpStatusCode.OK} or {HttpStatusCode.NoContent} but got {deleteResponse.StatusCode}. Body: {deleteBody}");

        Assert.True(
            condition: getResponse.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, getResponse));
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteGroup_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.DeleteAsync(Routes.Group.Generate.DeleteGroup(9999), _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
    }

    #endregion
}