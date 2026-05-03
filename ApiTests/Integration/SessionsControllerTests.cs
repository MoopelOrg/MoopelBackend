using System.Net;
using System.Net.Http.Json;
using ApiTests.Integration.Generic;
using ApiTests.TestData;
using MoopelApi;
using MoopelApi.Models.Requests;
using MoopelApi.Models.Results;

namespace ApiTests.Integration;

public class SessionsControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public SessionsControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    [Fact]
    public async Task MySessions_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(Routes.Session.MySessions, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SessionById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("session/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RevokeSession_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response =
            await client.PostAsync("session/revoke/1", null, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RevokeAll_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response =
            await client.PostAsync(Routes.Session.RevokeAll, null, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MySessions_ReturnsList_WhenAuthenticated(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync(Routes.Session.MySessions, _cancelToken);
        List<SessionRead>? sessions = await response.Content.ReadFromJsonAsync<List<SessionRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(sessions);
        Assert.NotEmpty(sessions);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task SessionById_ReturnsOwnedSession_WhenItExists(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage mySessionsResponse =
            await client.GetAsync(Routes.Session.MySessions, _cancelToken);

        List<SessionRead>? sessions =
            await mySessionsResponse.Content.ReadFromJsonAsync<List<SessionRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, mySessionsResponse.StatusCode);
        Assert.NotNull(sessions);
        Assert.NotEmpty(sessions);

        SessionRead firstSession = sessions.First();

        HttpResponseMessage response =
            await client.GetAsync($"session/{firstSession.SessionId}", _cancelToken);

        SessionRead? result =
            await response.Content.ReadFromJsonAsync<SessionRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(firstSession.SessionId, result.SessionId);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task SessionById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync("session/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersSession()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "sessionuser1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "sessionuser2", "Password123!", _cancelToken);

        HttpResponseMessage user1SessionsResponse =
            await user1.GetAsync(Routes.Session.MySessions, _cancelToken);

        List<SessionRead>? user1Sessions =
            await user1SessionsResponse.Content.ReadFromJsonAsync<List<SessionRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, user1SessionsResponse.StatusCode);
        Assert.NotNull(user1Sessions);
        Assert.NotEmpty(user1Sessions);

        SessionRead session = user1Sessions.First();

        HttpResponseMessage response =
            await user2.GetAsync($"session/{session.SessionId}", _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task RevokeSession_RemovesOwnedSession_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient primaryClient = _apiFactory.CreateClient();
        HttpClient secondClient = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(primaryClient, username, password, _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(secondClient, username + "x", password, _cancelToken);

        // Create another session for the same user on another client
        RegistrationRequest registerRequest = new()
        {
            Username = username,
            Password = password
        };

        LoginRequest loginRequest = new()
        {
            Username = username,
            Password = password
        };

        LoginResult? secondLogin =
            await GenericAuthActions.Login(secondClient, loginRequest, _cancelToken);

        Assert.NotNull(secondLogin);
        Assert.NotNull(secondLogin.Token);
        GenericAuthActions.AttachBearer(secondClient, secondLogin.Token);

        HttpResponseMessage sessionsResponse =
            await primaryClient.GetAsync(Routes.Session.MySessions, _cancelToken);

        List<SessionRead>? sessions =
            await sessionsResponse.Content.ReadFromJsonAsync<List<SessionRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, sessionsResponse.StatusCode);
        Assert.NotNull(sessions);
        Assert.True(sessions.Count >= 1);

        SessionRead sessionToRevoke = sessions.Last();

        HttpResponseMessage revokeResponse =
            await primaryClient.PostAsync($"session/revoke/{sessionToRevoke.SessionId}", null, _cancelToken);

        Assert.True(
            revokeResponse.StatusCode == HttpStatusCode.OK ||
            revokeResponse.StatusCode == HttpStatusCode.NoContent);

        HttpResponseMessage getResponse =
            await primaryClient.GetAsync($"session/{sessionToRevoke.SessionId}", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task RevokeSession_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.PostAsync("session/revoke/999999", null, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotRevoke_AnotherUsersSession()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "revokeuser1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "revokeuser2", "Password123!", _cancelToken);

        HttpResponseMessage user1SessionsResponse =
            await user1.GetAsync(Routes.Session.MySessions, _cancelToken);

        List<SessionRead>? user1Sessions =
            await user1SessionsResponse.Content.ReadFromJsonAsync<List<SessionRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, user1SessionsResponse.StatusCode);
        Assert.NotNull(user1Sessions);
        Assert.NotEmpty(user1Sessions);

        SessionRead session = user1Sessions.First();

        HttpResponseMessage response =
            await user2.PostAsync($"session/revoke/{session.SessionId}", null, _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task RevokeAll_RemovesOtherSessions_AndKeepsRequestValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient client1 = _apiFactory.CreateClient();
        HttpClient client2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client1, username, password, _cancelToken);

        RegistrationRequest registerRequest = new()
        {
            Username = username,
            Password = password
        };

        LoginRequest loginRequest = new()
        {
            Username = username,
            Password = password
        };

        LoginResult? loginResult =
            await GenericAuthActions.Login(client2, loginRequest, _cancelToken);

        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);
        GenericAuthActions.AttachBearer(client2, loginResult.Token);

        HttpResponseMessage beforeResponse =
            await client1.GetAsync(Routes.Session.MySessions, _cancelToken);

        List<SessionRead>? beforeSessions =
            await beforeResponse.Content.ReadFromJsonAsync<List<SessionRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, beforeResponse.StatusCode);
        Assert.NotNull(beforeSessions);
        Assert.True(beforeSessions.Count >= 2);

        HttpResponseMessage revokeAllResponse =
            await client1.PostAsync(Routes.Session.RevokeAll, null, _cancelToken);

        Assert.True(
            revokeAllResponse.StatusCode == HttpStatusCode.OK ||
            revokeAllResponse.StatusCode == HttpStatusCode.NoContent);

        HttpResponseMessage afterResponse =
            await client1.GetAsync(Routes.Session.MySessions, _cancelToken);

        Assert.Equal(HttpStatusCode.OK, afterResponse.StatusCode);

        HttpResponseMessage oldOtherSessionRequest =
            await client2.GetAsync(Routes.Session.MySessions, _cancelToken);

        Assert.True(
            oldOtherSessionRequest.StatusCode == HttpStatusCode.Unauthorized ||
            oldOtherSessionRequest.StatusCode == HttpStatusCode.Forbidden);
    }
}