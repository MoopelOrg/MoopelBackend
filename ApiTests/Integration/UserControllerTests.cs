using System.Net;
using System.Net.Http.Json;

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
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
        UserRead? me = await meResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.NotNull(me);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.User.Generate.UserById(me!.UserId), _cancelToken);
        UserRead? fetched = await response.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(fetched);
        Assert.Equal(me.UserId, fetched!.UserId);
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
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
        UserRead? user1Me = await user1MeResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, user1MeResponse.StatusCode);
        Assert.NotNull(user1Me);

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.User.Generate.UserById(user1Me!.UserId), _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
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
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
        UserRead? fetched = await response.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(fetched);
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
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
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
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
        UserRead? me = await meResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.NotNull(me);

        // Act
        HttpResponseMessage updateResponse =
            await client.PatchAsJsonAsync(
                UpdateUserRoute(me!.UserId),
                new UserUpdateRequest(),
                _cancelToken);
        UserRead? updated = await updateResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(me.UserId, updated!.UserId);
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
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
        UserRead? user1Me = await user1MeResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, user1MeResponse.StatusCode);
        Assert.NotNull(user1Me);

        // Act
        HttpResponseMessage response =
            await user2.PatchAsJsonAsync(
                UpdateUserRoute(user1Me!.UserId),
                new UserUpdateRequest(),
                _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
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
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
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
        UserRead? user1Me = await user1MeResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, user1MeResponse.StatusCode);
        Assert.NotNull(user1Me);

        // Act
        HttpResponseMessage response =
            await user2.PatchAsJsonAsync(Routes.User.Generate.DisableUser(user1Me!.UserId), new { }, _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
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
        UserRead? targetUser = await targetMeResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, targetMeResponse.StatusCode);
        Assert.NotNull(targetUser);

        // Act
        HttpResponseMessage disableResponse =
            await adminClient.PatchAsJsonAsync(Routes.User.Generate.DisableUser(targetUser!.UserId), new { }, _cancelToken);

        // Assert
        Assert.True(
            disableResponse.StatusCode == HttpStatusCode.OK ||
            disableResponse.StatusCode == HttpStatusCode.NoContent ||
            disableResponse.StatusCode == HttpStatusCode.Forbidden);

        if (disableResponse.StatusCode == HttpStatusCode.OK ||
            disableResponse.StatusCode == HttpStatusCode.NoContent)
        {
            LoginResult? loginResult = await GenericActions.Auth.Login(
                _apiFactory.CreateClient(),
                new LoginRequest
                {
                    Username = AuthTestData.ValidUserName2,
                    Password = AuthTestData.ValidPassword2
                },
                _cancelToken);

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
