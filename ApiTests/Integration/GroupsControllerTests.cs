using System.Net;
using System.Net.Http.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

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
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MyGroups_ReturnsList_WhenAuthenticated(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Group.MyGroups, _cancelToken);
        List<Group>? groups = await response.Content.ReadFromJsonAsync<List<Group>>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(groups);
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
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task GroupById_ReturnsGroup_WhenItExists(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        GroupCreateRequest createRequest = GroupTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, createRequest, _cancelToken);

        Group? created = await createResponse.Content.ReadFromJsonAsync<Group>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage response =
            await client.GetAsync(Routes.Group.Generate.GroupById(created.GroupId), _cancelToken);

        Group? result = await response.Content.ReadFromJsonAsync<Group>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
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

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Group.Generate.GroupById(9999), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GroupById_DoesNotExpose_AnotherUsersGroup()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        GroupCreateRequest request = GroupTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage createResponse =
            await user1.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);

        Group? created = await createResponse.Content.ReadFromJsonAsync<Group>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage response =
            await user2.GetAsync(Routes.Group.Generate.GroupById(created.GroupId), _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
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
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GroupTestData.ValidGroupCreateRequests), MemberType = typeof(GroupTestData))]
    public async Task CreateGroup_ReturnsGroup_WhenValid(GroupCreateRequest request)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);

        Group? created = await response.Content.ReadFromJsonAsync<Group>(_cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Assert.NotNull(created);
        Assert.NotNull(request);
        Assert.True(created.GroupId > 0);
        Assert.Equal(request.Name, created.Name);
    }

    [Theory]
    [MemberData(nameof(GroupTestData.ValidGroupCreateRequests), MemberType = typeof(GroupTestData))]
    public async Task CreateGroup_Fails_WhenCreatingDuplicateGroupNameForSameUser(GroupCreateRequest request)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage firstResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);

        HttpResponseMessage secondResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, request, _cancelToken);

        // Assert
        Assert.True(
            firstResponse.StatusCode == HttpStatusCode.OK ||
            firstResponse.StatusCode == HttpStatusCode.Created);

        Assert.True(
            secondResponse.StatusCode == HttpStatusCode.BadRequest ||
            secondResponse.StatusCode == HttpStatusCode.Conflict);
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

        await GenericAuthActions.CreateAuthenticatedClient(client, _cancelToken);

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, createRequest, _cancelToken);

        Group? created = await createResponse.Content.ReadFromJsonAsync<Group>(_cancelToken);

        Assert.NotNull(created);

        // Act
        HttpResponseMessage updateResponse =
            await client.PutAsJsonAsync(Routes.Group.Generate.UpdateGroup(created.GroupId), updateRequest, _cancelToken);

        Group? updated = await updateResponse.Content.ReadFromJsonAsync<Group>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
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

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        GroupUpdateRequest updateRequest = GroupTestData.ValidUpdateRequest;

        // Act
        HttpResponseMessage response =
            await client.PutAsJsonAsync(Routes.Group.Generate.UpdateGroup(9999), updateRequest, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        GroupCreateRequest createRequest = GroupTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, createRequest, _cancelToken);

        Group? created = await createResponse.Content.ReadFromJsonAsync<Group>(_cancelToken);

        Assert.NotNull(created);

        // Act
        HttpResponseMessage deleteResponse =
            await client.DeleteAsync(Routes.Group.Generate.DeleteGroup(created.GroupId), _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync(Routes.Group.Generate.GroupById(created.GroupId), _cancelToken);

        // Assert
        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteGroup_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.DeleteAsync(Routes.Group.Generate.DeleteGroup(9999), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
}