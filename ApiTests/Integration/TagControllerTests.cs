using System.Net;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Attributes;
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
    [TestDocumentation("Get user tags should be unauthorized with no token", HttpStatusCode.Unauthorized)]
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

    [Fact]
    [TestDocumentation("Get user tags should return an empty list when user is authenticated", HttpStatusCode.OK)]
    public async Task UserTags_ReturnsList_WhenAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Tag.UserTags, _cancelToken);
        List<UserTag>? tags = await response.Content.ReadFromJsonAsync<List<UserTag>>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(tags is not null, "Response body could not be deserialized into a list of UserTag.");
        Assert.Empty(tags);
    }

    [Fact]
    [TestDocumentation("Get user tags should return created tags", HttpStatusCode.OK)]
    public async Task UserTags_ReturnsListWithCreatedItem_AfterCreating_WhenAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        TagCreateRequest request = TagTestData.ValidCreateRequest;

        // Act
        await CreateUserTagAsync(client, request);
        HttpResponseMessage response = await client.GetAsync(Routes.Tag.UserTags, _cancelToken);
        List<UserTag>? tags = await response.Content.ReadFromJsonAsync<List<UserTag>>(_cancelToken);
        UserTag? tag = tags?.FirstOrDefault();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(tags is not null, "Response body could not be deserialized into a list of UserTag.");
        Assert.NotEmpty(tags);

        Assert.True(tag is not null, "Expected at least one tag in the list.");
        Assert.Equal(request.Name, tag.Name);
        Assert.Equal(request.Color, tag.Color);
    }

    #endregion

    #region Get User Tag By Id

    [Fact]
    [TestDocumentation("Get user tag by id should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task TagById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        string endpoint = Routes.Tag.Generate.UserTagById(TagTestData.MissingTagId);

        // Act
        HttpResponseMessage response = await client.GetAsync(endpoint, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    [TestDocumentation("Get user tag by id should return the tag when it exists and belongs to the user", HttpStatusCode.OK)]
    public async Task TagById_ReturnsTag_WhenItExists()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        TagCreateRequest createRequest = TagTestData.ValidCreateRequest;

        // Act
        UserTag created = await CreateUserTagAsync(client, createRequest);
        HttpResponseMessage response =
            await client.GetAsync(Routes.Tag.Generate.UserTagById(created.TagId), _cancelToken);

        UserTag? result = await response.Content.ReadFromJsonAsync<UserTag>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result is not null, "Response body could not be deserialized into a UserTag.");
        Assert.Equal(created.TagId, result.TagId);
        Assert.Equal(created.Name, result.Name);
        Assert.Equal(created.Color, result.Color);
    }

    [Fact]
    [TestDocumentation("Get user tag by id should return not found when the tag does not exist", HttpStatusCode.NotFound)]
    public async Task TagById_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Tag.Generate.UserTagById(TagTestData.MissingTagId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    [TestDocumentation("One user should not be able to retrieve another user's tag", HttpStatusCode.Forbidden)]
    public async Task OneUser_CannotAccess_AnotherUsersTag()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        TagCreateRequest request = TagTestData.ValidCreateRequest;

        // Act
        UserTag created = await CreateUserTagAsync(user1, request);
        HttpResponseMessage response =
            await user2.GetAsync(Routes.Tag.Generate.UserTagById(created.TagId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Create User Tag

    [Fact]
    [TestDocumentation("Create user tag should be unauthorized with no token", HttpStatusCode.Unauthorized)]
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
    [TestDocumentation("Create user tag should return created tag when request is valid", HttpStatusCode.Created)]
    [MemberData(nameof(TagTestData.ValidTagCreateRequests), MemberType = typeof(TagTestData))]
    public async Task CreateUserTag_ReturnsTag_WhenValid(TagCreateRequest request)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Tag.CreateUserTag, request, _cancelToken);

        UserTag? created = await response.Content.ReadFromJsonAsync<UserTag>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(created is not null, "Response body could not be deserialized into a UserTag.");
        Assert.NotNull(request);
        Assert.True(created.TagId > 0, $"Expected a positive TagId but got {created.TagId}");
        Assert.Equal(request.Name, created.Name);
        Assert.Equal(request.Color, created.Color);
    }

    #endregion

    #region Update User Tag

    [Fact]
    [TestDocumentation("Update user tag should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task UpdateTag_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        TagUpdateRequest request = TagTestData.ValidUpdateRequest;
        string endpoint = Routes.Tag.Generate.UpdateUserTag(TagTestData.MissingTagId);

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(endpoint, request, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [TestDocumentation("Update user tag should return updated tag when request is valid", HttpStatusCode.OK)]
    [MemberData(nameof(TagTestData.ValidTagUpdateCreateRequests), MemberType = typeof(TagTestData))]
    public async Task UpdateTag_UpdatesTag_WhenValid(TagCreateRequest createRequest, TagUpdateRequest updateRequest)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        UserTag created = await CreateUserTagAsync(client, createRequest);
        HttpResponseMessage updateResponse =
            await client.PatchAsJsonAsync(Routes.Tag.Generate.UpdateUserTag(created.TagId), updateRequest, _cancelToken);

        UserTag? updated = await updateResponse.Content.ReadFromJsonAsync<UserTag>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.True(updated is not null, "Response body could not be deserialized into a UserTag.");
        Assert.NotNull(createRequest);
        Assert.NotNull(updateRequest);
        Assert.Equal(created.TagId, updated.TagId);
        Assert.Equal(updateRequest.Name, updated.Name);
        Assert.Equal(updateRequest.Color, updated.Color);
    }

    [Theory]
    [TestDocumentation("Update user tag should return not found when the tag does not exist", HttpStatusCode.NotFound)]
    [MemberData(nameof(TagTestData.ValidTagUpdateRequests), MemberType = typeof(TagTestData))]
    public async Task UpdateTag_ReturnsNotFound_WhenMissing(TagUpdateRequest updateRequest)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(Routes.Tag.Generate.UpdateUserTag(TagTestData.MissingTagId), updateRequest, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Delete User Tag

    [Fact]
    [TestDocumentation("Delete user tag should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task DeleteTag_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        string endpoint = Routes.Tag.Generate.DeleteUserTag(TagTestData.MissingTagId);

        // Act
        HttpResponseMessage response = await client.DeleteAsync(endpoint, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [TestDocumentation("Delete user tag should remove the tag when it exists and belongs to the user", HttpStatusCode.NoContent)]
    [MemberData(nameof(TagTestData.ValidTagCreateRequests), MemberType = typeof(TagTestData))]
    public async Task DeleteTag_RemovesTag_WhenValid(TagCreateRequest request)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        UserTag created = await CreateUserTagAsync(client, request);
        HttpResponseMessage deleteResponse =
            await client.DeleteAsync(Routes.Tag.Generate.DeleteUserTag(created.TagId), _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync(Routes.Tag.Generate.UserTagById(created.TagId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    [TestDocumentation("Delete user tag should return not found when the tag does not exist", HttpStatusCode.NotFound)]
    public async Task DeleteTag_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.DeleteAsync(Routes.Tag.Generate.DeleteUserTag(TagTestData.MissingTagId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Get Group Tags

    [Fact]
    [TestDocumentation("Get group tags should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task GroupTags_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Tag.Generate.GroupTagsByGroupId(TagTestData.MissingGroupId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    [TestDocumentation("Get group tags should return tags when the group exists and the user has access", HttpStatusCode.OK)]
    public async Task GroupTags_ReturnsList_WhenGroupExistsAndUserHasAccess()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        Group group = await CreateGroupAsync(client);
        GroupTag created = await CreateGroupTagAsync(client, group.GroupId, TagTestData.ValidCreateRequest);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Tag.Generate.GroupTagsByGroupId(group.GroupId), _cancelToken);

        List<GroupTag>? tags = await response.Content.ReadFromJsonAsync<List<GroupTag>>(_cancelToken);
        GroupTag? tag = tags?.FirstOrDefault();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(tags is not null, "Response body could not be deserialized into a list of GroupTag.");
        Assert.NotEmpty(tags);

        Assert.True(tag is not null, "Expected at least one group tag in the list.");
        Assert.Equal(created.TagId, tag.TagId);
        Assert.Equal(created.GroupId, tag.GroupId);
        Assert.Equal(created.Name, tag.Name);
    }

    [Fact]
    [TestDocumentation("Get group tags should return not found when the group does not exist", HttpStatusCode.NotFound)]
    public async Task GroupTags_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Tag.Generate.GroupTagsByGroupId(TagTestData.MissingGroupId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    [TestDocumentation("One user should not be able to read another user's group tags", HttpStatusCode.Forbidden)]
    public async Task GroupTags_DoesNotExpose_AnotherUsersGroupTags()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        Group group = await CreateGroupAsync(user1);
        await CreateGroupTagAsync(user1, group.GroupId, TagTestData.ValidCreateRequest);

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.Tag.Generate.GroupTagsByGroupId(group.GroupId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Get Group Tag By Id

    [Fact]
    [TestDocumentation("Get group tag by id should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task GroupTagById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        string endpoint = Routes.Tag.Generate.GroupTagById(TagTestData.MissingTagId);

        // Act
        HttpResponseMessage response = await client.GetAsync(endpoint, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    [TestDocumentation("Get group tag by id should return the tag when it exists and the user has access", HttpStatusCode.OK)]
    public async Task GroupTagById_ReturnsTag_WhenItExists()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        Group group = await CreateGroupAsync(client);
        GroupTag created = await CreateGroupTagAsync(client, group.GroupId, TagTestData.ValidCreateRequest);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Tag.Generate.GroupTagById(created.TagId), _cancelToken);

        GroupTag? result = await response.Content.ReadFromJsonAsync<GroupTag>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result is not null, "Response body could not be deserialized into a GroupTag.");
        Assert.Equal(created.TagId, result.TagId);
        Assert.Equal(created.GroupId, result.GroupId);
        Assert.Equal(created.Name, result.Name);
        Assert.Equal(created.Color, result.Color);
    }

    [Fact]
    [TestDocumentation("Get group tag by id should return not found when the tag does not exist", HttpStatusCode.NotFound)]
    public async Task GroupTagById_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Tag.Generate.GroupTagById(TagTestData.MissingTagId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    [TestDocumentation("One user should not be able to read another user's group tag", HttpStatusCode.Forbidden)]
    public async Task OneUser_CannotAccess_AnotherUsersGroupTag()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        Group group = await CreateGroupAsync(user1);
        GroupTag created = await CreateGroupTagAsync(user1, group.GroupId, TagTestData.ValidCreateRequest);

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.Tag.Generate.GroupTagById(created.TagId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Create Group Tag

    [Fact]
    [TestDocumentation("Create group tag should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task CreateGroupTag_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Tag.Generate.CreateGroupTag(TagTestData.MissingGroupId), TagTestData.ValidCreateRequest, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [TestDocumentation("Create group tag should return created tag when the group exists and the user has access", HttpStatusCode.Created)]
    [MemberData(nameof(TagTestData.ValidTagCreateRequests), MemberType = typeof(TagTestData))]
    public async Task CreateGroupTag_ReturnsTag_WhenValid(TagCreateRequest request)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);
        Group group = await CreateGroupAsync(client);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Tag.Generate.CreateGroupTag(group.GroupId), request, _cancelToken);

        GroupTag? created = await response.Content.ReadFromJsonAsync<GroupTag>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(created is not null, "Response body could not be deserialized into a GroupTag.");
        Assert.NotNull(request);
        Assert.True(created.TagId > 0, $"Expected a positive TagId but got {created.TagId}");
        Assert.Equal(group.GroupId, created.GroupId);
        Assert.Equal(request.Name, created.Name);
        Assert.Equal(request.Color, created.Color);
    }

    [Fact]
    [TestDocumentation("Create group tag should return not found when the group does not exist", HttpStatusCode.NotFound)]
    public async Task CreateGroupTag_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Tag.Generate.CreateGroupTag(TagTestData.MissingGroupId), TagTestData.ValidCreateRequest, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    [TestDocumentation("One user should not be able to create tags in another user's group", HttpStatusCode.Forbidden)]
    public async Task CreateGroupTag_ReturnsForbidden_WhenNoAccess()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        Group group = await CreateGroupAsync(user1);

        // Act
        HttpResponseMessage response =
            await user2.PostAsJsonAsync(Routes.Tag.Generate.CreateGroupTag(group.GroupId), TagTestData.ValidCreateRequest, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Update Group Tag

    [Fact]
    [TestDocumentation("Update group tag should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task UpdateGroupTag_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        TagUpdateRequest request = TagTestData.ValidUpdateRequest;

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(Routes.Tag.Generate.UpdateGroupTag(TagTestData.MissingTagId), request, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [TestDocumentation("Update group tag should return updated tag when the user has access", HttpStatusCode.OK)]
    [MemberData(nameof(TagTestData.ValidTagUpdateCreateRequests), MemberType = typeof(TagTestData))]
    public async Task UpdateGroupTag_UpdatesTag_WhenValid(TagCreateRequest createRequest, TagUpdateRequest updateRequest)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);
        Group group = await CreateGroupAsync(client);
        GroupTag created = await CreateGroupTagAsync(client, group.GroupId, createRequest);

        // Act
        HttpResponseMessage updateResponse =
            await client.PatchAsJsonAsync(Routes.Tag.Generate.UpdateGroupTag(created.TagId), updateRequest, _cancelToken);

        GroupTag? updated = await updateResponse.Content.ReadFromJsonAsync<GroupTag>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.True(updated is not null, "Response body could not be deserialized into a GroupTag.");
        Assert.NotNull(createRequest);
        Assert.NotNull(updateRequest);
        Assert.Equal(created.TagId, updated.TagId);
        Assert.Equal(created.GroupId, updated.GroupId);
        Assert.Equal(updateRequest.Name, updated.Name);
        Assert.Equal(updateRequest.Color, updated.Color);
    }

    [Theory]
    [TestDocumentation("Update group tag should return not found when the tag does not exist", HttpStatusCode.NotFound)]
    [MemberData(nameof(TagTestData.ValidTagUpdateRequests), MemberType = typeof(TagTestData))]
    public async Task UpdateGroupTag_ReturnsNotFound_WhenMissing(TagUpdateRequest updateRequest)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(Routes.Tag.Generate.UpdateGroupTag(TagTestData.MissingTagId), updateRequest, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    [TestDocumentation("One user should not be able to update another user's group tag", HttpStatusCode.Forbidden)]
    public async Task UpdateGroupTag_ReturnsForbidden_WhenNoAccess()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        Group group = await CreateGroupAsync(user1);
        GroupTag created = await CreateGroupTagAsync(user1, group.GroupId, TagTestData.ValidCreateRequest);

        // Act
        HttpResponseMessage response =
            await user2.PatchAsJsonAsync(Routes.Tag.Generate.UpdateGroupTag(created.TagId), TagTestData.ValidUpdateRequest, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Delete Group Tag

    [Fact]
    [TestDocumentation("Delete group tag should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task DeleteGroupTag_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.DeleteAsync(Routes.Tag.Generate.DeleteGroupTag(TagTestData.MissingTagId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [TestDocumentation("Delete group tag should remove the tag when the user has access", HttpStatusCode.NoContent)]
    [MemberData(nameof(TagTestData.ValidTagCreateRequests), MemberType = typeof(TagTestData))]
    public async Task DeleteGroupTag_RemovesTag_WhenValid(TagCreateRequest request)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        Group group = await CreateGroupAsync(client);
        GroupTag created = await CreateGroupTagAsync(client, group.GroupId, request);

        // Act
        HttpResponseMessage deleteResponse =
            await client.DeleteAsync(Routes.Tag.Generate.DeleteGroupTag(created.TagId), _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync(Routes.Tag.Generate.GroupTagById(created.TagId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    [TestDocumentation("Delete group tag should return not found when the tag does not exist", HttpStatusCode.NotFound)]
    public async Task DeleteGroupTag_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.DeleteAsync(Routes.Tag.Generate.DeleteGroupTag(TagTestData.MissingTagId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    [TestDocumentation("One user should not be able to delete another user's group tag", HttpStatusCode.Forbidden)]
    public async Task DeleteGroupTag_ReturnsForbidden_WhenNoAccess()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient userClient1 = _apiFactory.CreateClient();
        HttpClient userClient2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(userClient1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(userClient2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        Group group = await CreateGroupAsync(userClient1);
        GroupTag created = await CreateGroupTagAsync(userClient1, group.GroupId, TagTestData.ValidCreateRequest);

        // Act
        HttpResponseMessage response =
            await userClient2.DeleteAsync(Routes.Tag.Generate.DeleteGroupTag(created.TagId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    private async Task<UserTag> CreateUserTagAsync(HttpClient client, TagCreateRequest request)
    {
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Tag.CreateUserTag, request, _cancelToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        UserTag? created = await response.Content.ReadFromJsonAsync<UserTag>(_cancelToken);
        Assert.True(created is not null, "Response body could not be deserialized into a UserTag.");

        return created;
    }

    private async Task<Group> CreateGroupAsync(HttpClient client)
    {
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Group.CreateGroup, GroupTestData.ValidCreateRequest, _cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Group? created = await response.Content.ReadFromJsonAsync<Group>(_cancelToken);
        Assert.True(created is not null, "Response body could not be deserialized into a Group.");

        return created;
    }

    private async Task<GroupTag> CreateGroupTagAsync(HttpClient client, int groupId, TagCreateRequest request)
    {
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Tag.Generate.CreateGroupTag(groupId), request, _cancelToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        GroupTag? created = await response.Content.ReadFromJsonAsync<GroupTag>(_cancelToken);
        Assert.True(created is not null, "Response body could not be deserialized into a GroupTag.");

        return created;
    }
}
