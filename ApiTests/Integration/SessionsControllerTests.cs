using System.Net;
using System.Net.Http.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Dto;
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

    #region Get My Sessions

    [Fact]
    public async Task MySessions_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Session.MySessions, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MySessions_ReturnsList_WhenAuthenticated(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Session.MySessions, _cancelToken);
        List<Session>? sessions = await response.Content.ReadFromJsonAsync<List<Session>>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(sessions);
        Assert.NotEmpty(sessions);
    }

    #endregion

    #region Get Session By Id

    [Fact]
    public async Task SessionById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Session.Generate.SessionById(1), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task SessionById_ReturnsOwnedSession_WhenItExists(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage mySessionsResponse =
            await client.GetAsync(Routes.Session.MySessions, _cancelToken);

        List<Session>? sessions =
            await mySessionsResponse.Content.ReadFromJsonAsync<List<Session>>(_cancelToken);

        Assert.NotNull(sessions);
        Assert.NotEmpty(sessions);

        Session firstSession = sessions.First();

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Session.Generate.SessionById(firstSession.SessionId), _cancelToken);

        Session? result =
            await response.Content.ReadFromJsonAsync<Session>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(firstSession.SessionId, result.SessionId);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task SessionById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Session.Generate.SessionById(9999), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersSession()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        HttpResponseMessage user1SessionsResponse =
            await user1.GetAsync(Routes.Session.MySessions, _cancelToken);

        List<Session>? user1Sessions =
            await user1SessionsResponse.Content.ReadFromJsonAsync<List<Session>>(_cancelToken);

        Assert.NotNull(user1Sessions);
        Assert.NotEmpty(user1Sessions);

        Session session = user1Sessions.First();

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.Session.Generate.SessionById(session.SessionId), _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    #endregion

    #region Revoke Session

    [Fact]
    public async Task RevokeSession_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PostAsync(Routes.Session.Generate.RevokeSession(1), null, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task RevokeSession_RemovesOwnedSession_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient primaryClient = _apiFactory.CreateClient();
        HttpClient secondClient = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(primaryClient, username, password, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(secondClient, username + "x", password, _cancelToken);

        // Create another session for the same user on another client
        LoginRequest loginRequest = new()
        {
            Username = username,
            Password = password
        };

        LoginResult? secondLogin =
            await GenericActions.Auth.Login(secondClient, loginRequest, _cancelToken);

        Assert.NotNull(secondLogin);
        Assert.NotNull(secondLogin.Token);
        GenericActions.Auth.AttachBearer(secondClient, secondLogin.Token);

        HttpResponseMessage sessionsResponse =
            await primaryClient.GetAsync(Routes.Session.MySessions, _cancelToken);

        List<Session>? sessions =
            await sessionsResponse.Content.ReadFromJsonAsync<List<Session>>(_cancelToken);

        Assert.NotNull(sessions);
        Assert.True(sessions.Count >= 1);

        Session sessionToRevoke = sessions.Last();

        // Act
        HttpResponseMessage revokeResponse =
            await primaryClient.PostAsync(Routes.Session.Generate.RevokeSession(sessionToRevoke.SessionId), null, _cancelToken);

        Assert.True(
            revokeResponse.StatusCode == HttpStatusCode.OK ||
            revokeResponse.StatusCode == HttpStatusCode.NoContent);

        HttpResponseMessage getResponse =
            await primaryClient.GetAsync(Routes.Session.Generate.SessionById(sessionToRevoke.SessionId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task RevokeSession_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PostAsync(Routes.Session.Generate.RevokeSession(9999), null, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotRevoke_AnotherUsersSession()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        HttpResponseMessage user1SessionsResponse =
            await user1.GetAsync(Routes.Session.MySessions, _cancelToken);

        List<Session>? user1Sessions =
            await user1SessionsResponse.Content.ReadFromJsonAsync<List<Session>>(_cancelToken);

        Assert.NotNull(user1Sessions);
        Assert.NotEmpty(user1Sessions);

        Session session = user1Sessions.First();

        // Act
        HttpResponseMessage response =
            await user2.PostAsync(Routes.Session.Generate.RevokeSession(session.SessionId), null, _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    #endregion

    #region Revoke All Sessions

    [Fact]
    public async Task RevokeAll_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PostAsync(Routes.Session.RevokeAll, null, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task RevokeAll_RemovesOtherSessions_AndKeepsRequestValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient client1 = _apiFactory.CreateClient();
        HttpClient client2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client1, username, password, _cancelToken);

        LoginRequest loginRequest = new()
        {
            Username = username,
            Password = password
        };

        LoginResult? loginResult =
            await GenericActions.Auth.Login(client2, loginRequest, _cancelToken);

        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);
        GenericActions.Auth.AttachBearer(client2, loginResult.Token);

        HttpResponseMessage beforeResponse =
            await client1.GetAsync(Routes.Session.MySessions, _cancelToken);

        List<Session>? beforeSessions =
            await beforeResponse.Content.ReadFromJsonAsync<List<Session>>(_cancelToken);

        Assert.NotNull(beforeSessions);
        Assert.True(beforeSessions.Count >= 2);

        // Act
        HttpResponseMessage revokeAllResponse =
            await client1.PostAsync(Routes.Session.RevokeAll, null, _cancelToken);

        Assert.True(
            revokeAllResponse.StatusCode == HttpStatusCode.OK ||
            revokeAllResponse.StatusCode == HttpStatusCode.NoContent);

        HttpResponseMessage afterResponse =
            await client1.GetAsync(Routes.Session.MySessions, _cancelToken);

        HttpResponseMessage oldOtherSessionRequest =
            await client2.GetAsync(Routes.Session.MySessions, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, afterResponse.StatusCode);
        Assert.True(
            oldOtherSessionRequest.StatusCode == HttpStatusCode.Unauthorized ||
            oldOtherSessionRequest.StatusCode == HttpStatusCode.Forbidden);
    }

    #endregion
}