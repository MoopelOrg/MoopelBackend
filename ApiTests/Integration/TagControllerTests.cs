using System.Net;
using System.Net.Http.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

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

    #region Get My User Tags

    [Fact]
    public async Task UserTags_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Tag.UserTags, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UserTags_ReturnsList_WhenAuthenticated(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Tag.UserTags, _cancelToken);
        List<Tag>? tags = await response.Content.ReadFromJsonAsync<List<Tag>>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(tags);
    }

    #endregion

    #region Get User Tag By Id

    [Fact]
    public async Task TagById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Tag.Generate.UserTagById(1), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task TagById_ReturnsTag_WhenItExists(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        TagCreateRequest createRequest = TagTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Tag.CreateUserTag, createRequest, _cancelToken);

        Tag? created = await createResponse.Content.ReadFromJsonAsync<Tag>(_cancelToken);

        Assert.NotNull(created);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Tag.Generate.UserTagById(created.TagId), _cancelToken);

        Tag? result = await response.Content.ReadFromJsonAsync<Tag>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.TagId, result.TagId);
        Assert.Equal(created.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task TagById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Tag.Generate.UserTagById(9999), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersTag()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        TagCreateRequest request = TagTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await user1.PostAsJsonAsync(Routes.Tag.CreateUserTag, request, _cancelToken);

        Tag? created = await createResponse.Content.ReadFromJsonAsync<Tag>(_cancelToken);

        Assert.NotNull(created);

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.Tag.Generate.UserTagById(created.TagId), _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    #endregion

    #region Create User Tag

    [Fact]
    public async Task CreateTag_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        TagCreateRequest request = TagTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Tag.CreateUserTag, request, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateUserTag_ReturnsTag_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        TagCreateRequest request = TagTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Tag.CreateUserTag, request, _cancelToken);

        Tag? created = await response.Content.ReadFromJsonAsync<Tag>(_cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Assert.NotNull(created);
        Assert.True(created.TagId > 0);
        Assert.Equal(TagTestData.ValidCreateRequest.Name, created.Name);
    }

    #endregion

    #region Update User Tag

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateTag_UpdatesTag_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        TagCreateRequest createRequest = TagTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Tag.CreateUserTag, createRequest, _cancelToken);

        Tag? created = await createResponse.Content.ReadFromJsonAsync<Tag>(_cancelToken);

        Assert.NotNull(created);

        TagUpdateRequest updateRequest = TagTestData.ValidUpdateRequest;

        // Act
        HttpResponseMessage updateResponse =
            await client.PutAsJsonAsync(Routes.Tag.Generate.UpdateUserTag(created.TagId), updateRequest, _cancelToken);

        Tag? updated = await updateResponse.Content.ReadFromJsonAsync<Tag>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.TagId, updated.TagId);
        Assert.Equal(TagTestData.ValidUpdateRequest.Name, updated.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateTag_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        TagUpdateRequest updateRequest = TagTestData.ValidUpdateRequest;

        // Act
        HttpResponseMessage response =
            await client.PutAsJsonAsync(Routes.Tag.Generate.UpdateUserTag(9999), updateRequest, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Delete User Tag

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteTag_RemovesTag_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        TagCreateRequest createRequest = TagTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Tag.CreateUserTag, createRequest, _cancelToken);

        Tag? created = await createResponse.Content.ReadFromJsonAsync<Tag>(_cancelToken);

        Assert.NotNull(created);

        // Act
        HttpResponseMessage deleteResponse =
            await client.DeleteAsync(Routes.Tag.Generate.DeleteUserTag(created.TagId), _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync(Routes.Tag.Generate.UserTagById(created.TagId), _cancelToken);

        // Assert
        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteTag_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.DeleteAsync(Routes.Tag.Generate.DeleteUserTag(9999), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Get Group Tags

    [Fact]
    public async Task GroupTags_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Tag.Generate.GroupTagsByGroupId(1), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task GroupTags_ReturnsList_WhenGroupExistsAndUserHasAccess(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        GroupCreateRequest GroupCreateRequest = GroupTestData.ValidCreateRequest;

        HttpResponseMessage groupResponse =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, GroupCreateRequest, _cancelToken);

        Group? group = await groupResponse.Content.ReadFromJsonAsync<Group>(_cancelToken);

        Assert.NotNull(group);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Tag.Generate.GroupTagsByGroupId(group.GroupId), _cancelToken);

        List<Tag>? tags = await response.Content.ReadFromJsonAsync<List<Tag>>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(tags);
    }

    [Fact]
    public async Task GroupTags_DoesNotExpose_AnotherUsersGroupTags()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        GroupCreateRequest GroupCreateRequest = GroupTestData.ValidCreateRequest;

        HttpResponseMessage groupResponse =
            await user1.PostAsJsonAsync(Routes.Group.CreateGroup, GroupCreateRequest, _cancelToken);

        Group? group = await groupResponse.Content.ReadFromJsonAsync<Group>(_cancelToken);

        Assert.NotNull(group);

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.Tag.Generate.GroupTagsByGroupId(group.GroupId), _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    #endregion
}