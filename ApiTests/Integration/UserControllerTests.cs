using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Attributes;
using MoopelApi.Models.Dto.Read;
using MoopelApi.Models.Requests;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Models.Results;

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
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,

            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
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
        UserRead? me = JsonSerializer.Deserialize<UserRead>(meResponseBody);
        HttpResponseMessage response =
            await client.GetAsync(Routes.User.Generate.UserById(me?.UserId ?? 0), _cancelToken);

        // Assert
        Assert.True(meResponse.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {meResponse.StatusCode}, endpoint {meResponse.RequestMessage?.RequestUri}, raw body: {meResponseBody}");
        Assert.True(me is not null, $"Response body could not be deserialized into a UserRead. Body: {meResponseBody}");
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        UserRead? fetched = JsonSerializer.Deserialize<UserRead>(responseBody);
        Assert.True(fetched is not null, $"Response body could not be deserialized into a UserRead. Body: {responseBody}");
        Assert.Equal(me!.UserId, fetched!.UserId);
        Assert.Equal(me.Username, fetched.Username);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UserById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.User.Generate.UserById(UserTestData.MissingUserId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
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
        UserRead? user1Me = JsonSerializer.Deserialize<UserRead>(user1MeResponseBody);
        HttpResponseMessage response =
            await user2.GetAsync(Routes.User.Generate.UserById(user1Me?.UserId ?? 0), _cancelToken);

        // Assert
        Assert.True(user1MeResponse.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {user1MeResponse.StatusCode}, endpoint {user1MeResponse.RequestMessage?.RequestUri}, raw body: {user1MeResponseBody}");
        Assert.True(user1Me is not null, $"Response body could not be deserialized into a UserRead. Body: {user1MeResponseBody}");
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
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
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,

            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
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
        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        UserRead? fetched = JsonSerializer.Deserialize<UserRead>(responseBody);
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
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
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

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(
                UpdateUserRoute(UserTestData.MissingUserId),
                new UserUpdateRequest(),
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,

            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateUser_UpdatesCurrentUser_WhenAllowed(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage meResponse = await client.GetAsync(Routes.User.Generate.UserByName(username), _cancelToken);

        // Act
        string meResponseBody = await meResponse.Content.ReadAsStringAsync(_cancelToken);
        UserRead? me = JsonSerializer.Deserialize<UserRead>(meResponseBody);
        HttpResponseMessage updateResponse =
            await client.PatchAsJsonAsync(
                UpdateUserRoute(me?.UserId ?? 0),
                new UserUpdateRequest(),
                _cancelToken);

        // Assert
        Assert.True(meResponse.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {meResponse.StatusCode}, endpoint {meResponse.RequestMessage?.RequestUri}, raw body: {meResponseBody}");
        Assert.True(me is not null, $"Response body could not be deserialized into a UserRead. Body: {meResponseBody}");
        string updateResponseBody = await updateResponse.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(updateResponse.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {updateResponse.StatusCode}, endpoint {updateResponse.RequestMessage?.RequestUri}, raw body: {updateResponseBody}");
        UserRead? updated = JsonSerializer.Deserialize<UserRead>(updateResponseBody);
        Assert.True(updated is not null, $"Response body could not be deserialized into a UserRead. Body: {updateResponseBody}");
        Assert.Equal(me!.UserId, updated!.UserId);
        Assert.Equal(me.Username, updated.Username);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateUser_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(
                UpdateUserRoute(UserTestData.MissingUserId),
                new UserUpdateRequest(),
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
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
        UserRead? user1Me = JsonSerializer.Deserialize<UserRead>(user1MeResponseBody);
        HttpResponseMessage response =
            await user2.PatchAsJsonAsync(
                UpdateUserRoute(user1Me?.UserId ?? 0),
                new UserUpdateRequest(),
                _cancelToken);

        // Assert
        Assert.True(user1MeResponse.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {user1MeResponse.StatusCode}, endpoint {user1MeResponse.RequestMessage?.RequestUri}, raw body: {user1MeResponseBody}");
        Assert.True(user1Me is not null, $"Response body could not be deserialized into a UserRead. Body: {user1MeResponseBody}");
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
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
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,

            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
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

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

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
        UserRead? user1Me = JsonSerializer.Deserialize<UserRead>(user1MeResponseBody);
        HttpResponseMessage response =
            await user2.PatchAsJsonAsync(Routes.User.Generate.DisableUser(user1Me?.UserId ?? 0), new { }, _cancelToken);

        // Assert
        Assert.True(user1MeResponse.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {user1MeResponse.StatusCode}, endpoint {user1MeResponse.RequestMessage?.RequestUri}, raw body: {user1MeResponseBody}");
        Assert.True(user1Me is not null, $"Response body could not be deserialized into a UserRead. Body: {user1MeResponseBody}");
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DisableUser_DisablesUser_WhenAllowed(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient adminClient = _apiFactory.CreateClient();
        HttpClient targetClient = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(adminClient, username, password, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(targetClient, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        HttpResponseMessage targetMeResponse = await targetClient.GetAsync(Routes.User.Generate.UserByName(AuthTestData.ValidUserName2), _cancelToken);

        // Act
        string targetMeResponseBody = await targetMeResponse.Content.ReadAsStringAsync(_cancelToken);
        UserRead? targetUser = JsonSerializer.Deserialize<UserRead>(targetMeResponseBody);
        HttpResponseMessage disableResponse =
            await adminClient.PatchAsJsonAsync(Routes.User.Generate.DisableUser(targetUser?.UserId ?? 0), new { }, _cancelToken);
        LoginResult? loginResult = await GenericActions.Auth.Login(
            _apiFactory.CreateClient(),
            new LoginRequest
            {
                Username = AuthTestData.ValidUserName2,
                Password = AuthTestData.ValidPassword2
            },
            _cancelToken);

        // Assert
        Assert.True(targetMeResponse.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {targetMeResponse.StatusCode}, endpoint {targetMeResponse.RequestMessage?.RequestUri}, raw body: {targetMeResponseBody}");
        Assert.True(targetUser is not null, $"Response body could not be deserialized into a UserRead. Body: {targetMeResponseBody}");
        string disableResponseBody = await disableResponse.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            disableResponse.StatusCode == HttpStatusCode.OK ||
            disableResponse.StatusCode == HttpStatusCode.NoContent ||
            disableResponse.StatusCode == HttpStatusCode.Forbidden,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.NoContent} or {HttpStatusCode.Forbidden}, actual {disableResponse.StatusCode}, endpoint {disableResponse.RequestMessage?.RequestUri}, raw body: {disableResponseBody}");

        if (disableResponse.StatusCode == HttpStatusCode.OK ||
            disableResponse.StatusCode == HttpStatusCode.NoContent)
        {
            Assert.True(
                loginResult is null ||
                loginResult.User is null ||
                !string.IsNullOrWhiteSpace(loginResult.Error));
        }
    }

    #endregion

    private static string UpdateUserRoute(int userId)
    {
        return Routes.User.UpdateUser.Replace("{userId:int}", userId.ToString());
    }
}
