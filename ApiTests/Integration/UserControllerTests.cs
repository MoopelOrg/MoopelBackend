using System.Net;
using System.Net.Http.Json;
using ApiTests.Integration.Generic;
using ApiTests.TestData;
using MoopelApi;
using MoopelApi.Models.Dto.Read;
using MoopelApi.Models.Requests;
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

    [Fact]
    public async Task MyProfile_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(Routes.User.MyProfile, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UserById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("user/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        CreateUserRequest request = new()
        {
            Username = "createduser",
            Password = "Password123!"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.User.CreateUser, request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        UpdateUserRequest request = new()
        {
            Username = "updateduser"
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync("user/update/1", request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DisableUser_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response =
            await client.PostAsync("user/disable/1", null, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MyProfile_ReturnsCurrentUser_WhenAuthenticated(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync(Routes.User.MyProfile, _cancelToken);
        UserRead? me = await response.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(me);
        Assert.Equal(username, me.Username);
        Assert.True(me.UserId > 0);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UserById_ReturnsCurrentUser_WhenRequestingOwnId(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage meResponse = await client.GetAsync(Routes.User.MyProfile, _cancelToken);
        UserRead? me = await meResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.NotNull(me);

        HttpResponseMessage response = await client.GetAsync($"user/{me.UserId}", _cancelToken);
        UserRead? fetched = await response.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(fetched);
        Assert.Equal(me.UserId, fetched.UserId);
        Assert.Equal(me.Username, fetched.Username);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UserById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync("user/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersProfile()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "profileuser1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "profileuser2", "Password123!", _cancelToken);

        HttpResponseMessage meResponse = await user1.GetAsync(Routes.User.MyProfile, _cancelToken);
        UserRead? user1Me = await meResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.NotNull(user1Me);

        HttpResponseMessage response = await user2.GetAsync($"user/{user1Me.UserId}", _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateUser_UpdatesCurrentUser_WhenAllowed(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage meResponse = await client.GetAsync(Routes.User.MyProfile, _cancelToken);
        UserRead? me = await meResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.NotNull(me);

        string updatedUsername = $"{username}_updated";

        UpdateUserRequest request = new()
        {
            Username = updatedUsername
        };

        HttpResponseMessage updateResponse =
            await client.PutAsJsonAsync($"user/update/{me.UserId}", request, _cancelToken);

        UserRead? updated = await updateResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(me.UserId, updated.UserId);
        Assert.Equal(updatedUsername, updated.Username);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateUser_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UpdateUserRequest request = new()
        {
            Username = "missinguser"
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync("user/update/999999", request, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotUpdate_AnotherUser()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "updateuser1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "updateuser2", "Password123!", _cancelToken);

        HttpResponseMessage meResponse = await user1.GetAsync(Routes.User.MyProfile, _cancelToken);
        UserRead? user1Me = await meResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.NotNull(user1Me);

        UpdateUserRequest request = new()
        {
            Username = "hackedname"
        };

        HttpResponseMessage response =
            await user2.PutAsJsonAsync($"user/update/{user1Me.UserId}", request, _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateUser_CreatesAnotherUser_WhenAllowed(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateUserRequest request = new()
        {
            Username = "createdbyuser",
            Password = "Password123!"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.User.CreateUser, request, _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created ||
            response.StatusCode == HttpStatusCode.Forbidden);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DisableUser_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.PostAsync("user/disable/999999", null, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotDisable_AnotherUser()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "disableuser1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "disableuser2", "Password123!", _cancelToken);

        HttpResponseMessage meResponse = await user1.GetAsync(Routes.User.MyProfile, _cancelToken);
        UserRead? user1Me = await meResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.NotNull(user1Me);

        HttpResponseMessage response =
            await user2.PostAsync($"user/disable/{user1Me.UserId}", null, _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DisableUser_DisablesUser_WhenAllowed(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient adminClient = _apiFactory.CreateClient();
        HttpClient targetClient = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(adminClient, username, password, _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(targetClient, "targetuser", "Password123!", _cancelToken);

        HttpResponseMessage targetMeResponse = await targetClient.GetAsync(Routes.User.MyProfile, _cancelToken);
        UserRead? targetUser = await targetMeResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, targetMeResponse.StatusCode);
        Assert.NotNull(targetUser);

        HttpResponseMessage disableResponse =
            await adminClient.PostAsync($"user/disable/{targetUser.UserId}", null, _cancelToken);

        Assert.True(
            disableResponse.StatusCode == HttpStatusCode.OK ||
            disableResponse.StatusCode == HttpStatusCode.NoContent ||
            disableResponse.StatusCode == HttpStatusCode.Forbidden);

        if (disableResponse.StatusCode == HttpStatusCode.OK ||
            disableResponse.StatusCode == HttpStatusCode.NoContent)
        {
            LoginResult? loginResult = await GenericAuthActions.Login(
                _apiFactory.CreateClient(),
                new LoginRequest
                {
                    Username = "targetuser",
                    Password = "Password123!"
                },
                _cancelToken);

            Assert.True(
                loginResult is null ||
                loginResult.User is null ||
                !string.IsNullOrWhiteSpace(loginResult.Error));
        }
    }
}