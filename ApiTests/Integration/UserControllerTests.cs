using System.Net;
using System.Text.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Attributes;

using MoopelObjects.Dto.Read;
using MoopelObjects.Requests;
using MoopelObjects.Requests.Update;
using MoopelObjects.Results;

using static ApiTests.TestHelper;

namespace ApiTests.Integration;

public class UserControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public UserControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    #region Get User By Id

    [Fact]
    [TestDocumentation("Get user by id should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task UserById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.User.Generate.UserById(UserTestData.MissingUserId), _cancelToken);

        // Assert

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UserById_ReturnsCurrentUser_WhenRequestingOwnId(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage meResponse = await client.GetAsync(Routes.User.Generate.UserByName(username), _cancelToken);

        // Act
        string meResponseBody = await meResponse.Content.ReadAsStringAsync(_cancelToken);
        UserRead? me = JsonSerializer.Deserialize<UserRead>(meResponseBody, _apiFactory.JsonSerializerOptions);
        HttpResponseMessage response =
            await client.GetAsync(Routes.User.Generate.UserById(me?.UserId ?? 0), _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            condition: meResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, meResponse));
        Assert.True(me is not null, $"Response body could not be deserialized into a UserRead. Body: {meResponseBody}");
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, response));
        UserRead? fetched = JsonSerializer.Deserialize<UserRead>(responseBody, _apiFactory.JsonSerializerOptions);
        Assert.True(fetched is not null, $"Response body could not be deserialized into a UserRead. Body: {responseBody}");
        Assert.Equal(me!.UserId, fetched!.UserId);
        Assert.Equal(me.Username, fetched.Username);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UserById_ReturnsForbidden_WhenMissing_AndRoleIsNotAdmin(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.User.Generate.UserById(UserTestData.MissingUserId), _cancelToken);

        // Assert

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Forbidden,
            userMessage: await TMsg(HttpStatusCode.Forbidden, response));
    }

    [Fact]
    public async Task UserById_ReturnsNotFound_WhenMissing_AndRoleIsAdmin()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAdminAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.User.Generate.UserById(UserTestData.MissingUserId), _cancelToken);

        // Assert

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersProfileById()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        HttpResponseMessage user1MeResponse = await user1.GetAsync(Routes.User.Generate.UserByName(AuthTestData.ValidUserName), _cancelToken);

        // Act
        string user1MeResponseBody = await user1MeResponse.Content.ReadAsStringAsync(_cancelToken);
        UserRead? user1Me = JsonSerializer.Deserialize<UserRead>(user1MeResponseBody, _apiFactory.JsonSerializerOptions);
        HttpResponseMessage response =
            await user2.GetAsync(Routes.User.Generate.UserById(user1Me?.UserId ?? 0), _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            condition: user1MeResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, user1MeResponse));
        Assert.True(user1Me is not null, $"Response body could not be deserialized into a UserRead. Body: {user1MeResponseBody}");
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    #endregion

    #region Get User By Name

    [Fact]
    [TestDocumentation("Get user by name should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task UserByName_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.User.Generate.UserByName(AuthTestData.ValidUserName), _cancelToken);

        // Assert

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UserByName_ReturnsCurrentUser_WhenRequestingOwnUsername(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.User.Generate.UserByName(username), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, response));
        UserRead? fetched = JsonSerializer.Deserialize<UserRead>(responseBody, _apiFactory.JsonSerializerOptions);
        Assert.True(fetched is not null, $"Response body could not be deserialized into a UserRead. Body: {responseBody}");
        Assert.Equal(username, fetched!.Username);
        Assert.True(fetched.UserId > 0);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UserByName_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.User.Generate.UserByName(UserTestData.MissingUsername), _cancelToken);

        // Assert

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersProfileByName()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.User.Generate.UserByName(AuthTestData.ValidUserName), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    #endregion

    #region Update User

    [Fact]
    [TestDocumentation("Update user should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task UpdateUser_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        string updateRoute = Routes.User.Generate.UpdateUser(UserTestData.MissingUserId);

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(
                updateRoute,
                new UserUpdateRequest(),
                _cancelToken);

        // Assert

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateUser_UpdatesCurrentUser_WhenAllowed(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        UserUpdateRequest updateRequest = new()
        {
            Email = "Email@email.com"
        };

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);
        HttpResponseMessage meResponse = await client.GetAsync(Routes.User.Generate.UserByName(username), _cancelToken);
        string meResponseBody = await meResponse.Content.ReadAsStringAsync(_cancelToken);

        // Act
        UserRead? me = JsonSerializer.Deserialize<UserRead>(meResponseBody, _apiFactory.JsonSerializerOptions);
        string updateRoute = Routes.User.Generate.UpdateUser(me?.UserId ?? 0);
        HttpResponseMessage updateResponse =
            await client.PatchAsJsonAsync(updateRoute, updateRequest, _cancelToken);
        string updateResponseBody = await updateResponse.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            condition: meResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, meResponse));
        Assert.True(me is not null, $"Response body could not be deserialized into a UserRead. Body: {meResponseBody}");
        Assert.True(
            condition: updateResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, updateResponse));
        UserRead? updated = JsonSerializer.Deserialize<UserRead>(updateResponseBody, _apiFactory.JsonSerializerOptions);
        Assert.True(updated is not null, $"Response body could not be deserialized into a UserRead. Body: {updateResponseBody}");
        Assert.Equal(me!.UserId, updated!.UserId);
        Assert.Equal(me.Username, updated.Username);
        Assert.Equal(updated.Email, updateRequest.Email);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateUser_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        string updateRoute = Routes.User.Generate.UpdateUser(UserTestData.MissingUserId);

        // Act
        HttpResponseMessage response = await client.PatchAsJsonAsync(updateRoute, new UserUpdateRequest(), _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Forbidden,
            userMessage: await TMsg(HttpStatusCode.Forbidden, response)
        );
    }

    [Fact]
    public async Task OneUser_CannotUpdate_AnotherUser()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        HttpResponseMessage user1MeResponse = await user1.GetAsync(Routes.User.Generate.UserByName(AuthTestData.ValidUserName), _cancelToken);

        // Act
        string user1MeResponseBody = await user1MeResponse.Content.ReadAsStringAsync(_cancelToken);
        UserRead? user1Me = JsonSerializer.Deserialize<UserRead>(user1MeResponseBody, _apiFactory.JsonSerializerOptions);
        string updateRoute = Routes.User.Generate.UpdateUser(user1Me?.UserId ?? 0);
        HttpResponseMessage response =
            await user2.PatchAsJsonAsync(updateRoute, new UserUpdateRequest(), _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            condition: user1MeResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, user1MeResponse));
        Assert.True(user1Me is not null, $"Response body could not be deserialized into a UserRead. Body: {user1MeResponseBody}");
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    #endregion

    #region Disable User

    [Fact]
    [TestDocumentation("Disable user should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task DisableUser_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(Routes.User.Generate.DisableUser(UserTestData.MissingUserId), new { }, _cancelToken);

        // Assert

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DisableUser_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(Routes.User.Generate.DisableUser(UserTestData.MissingUserId), new { }, _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        // Assert

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Fact]
    public async Task OneUser_CannotDisable_AnotherUser()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        HttpResponseMessage user1MeResponse = await user1.GetAsync(Routes.User.Generate.UserByName(AuthTestData.ValidUserName), _cancelToken);

        // Act
        string user1MeResponseBody = await user1MeResponse.Content.ReadAsStringAsync(_cancelToken);
        UserRead? user1Me = JsonSerializer.Deserialize<UserRead>(user1MeResponseBody, _apiFactory.JsonSerializerOptions);
        HttpResponseMessage response =
            await user2.PatchAsJsonAsync(Routes.User.Generate.DisableUser(user1Me?.UserId ?? 0), new { }, _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            condition: user1MeResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, user1MeResponse));
        Assert.True(user1Me is not null, $"Response body could not be deserialized into a UserRead. Body: {user1MeResponseBody}");
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task Admin_CanDisable_AnotherUser(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient adminClient = _apiFactory.CreateClient();
        HttpClient targetClient = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAdminAuthenticatedClient(adminClient, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(targetClient, username, password, _cancelToken);

        // Act

        // Get the target user
        HttpResponseMessage targetMeResponse = await targetClient.GetAsync(Routes.Auth.Me, _cancelToken);
        string targetMeResponseBody = await targetMeResponse.Content.ReadAsStringAsync(_cancelToken);
        UserRead? targetUser = JsonSerializer.Deserialize<UserRead>(targetMeResponseBody, _apiFactory.JsonSerializerOptions);

        // try disabling user
        string disableRoute = Routes.User.Generate.DisableUser(targetUser?.UserId ?? 0);
        HttpResponseMessage disableResponse = await adminClient.PatchAsJsonAsync(disableRoute, new { }, _cancelToken);

        LoginResult? loginResult = await GenericActions.Auth.Login(
            _apiFactory.CreateClient(),
            new LoginRequest
            {
                Username = AuthTestData.ValidUserName2,
                Password = AuthTestData.ValidPassword2
            },
            _cancelToken);

        // Assert
        Assert.True(
            condition: targetMeResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, targetMeResponse));
        Assert.True(targetUser is not null, $"Response body could not be deserialized into a UserRead. Body: {targetMeResponseBody}");

        Assert.True(
            condition: disableResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, disableResponse));

        Assert.True(
            loginResult is null ||
            loginResult.User is null ||
            !string.IsNullOrWhiteSpace(loginResult.Error));
    }

    #endregion
}
