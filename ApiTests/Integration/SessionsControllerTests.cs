using System.Net;
using System.Text.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests;
using MoopelApi.Models.Results;
using static ApiTests.TestHelper;

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

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
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

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, response));
        List<Session>? sessions = JsonSerializer.Deserialize<List<Session>>(responseBody, _apiFactory.JsonSerializerOptions);
        Assert.True(sessions is not null, $"Response body could not be deserialized into a List<Session>. Body: {responseBody}");
        Assert.NotEmpty(sessions!);
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

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
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

        // Act
        string mySessionsResponseBody = await mySessionsResponse.Content.ReadAsStringAsync(_cancelToken);
        List<Session>? sessions = JsonSerializer.Deserialize<List<Session>>(mySessionsResponseBody, _apiFactory.JsonSerializerOptions);
        Session? firstSession = sessions?.FirstOrDefault();
        HttpResponseMessage response =
            await client.GetAsync(Routes.Session.Generate.SessionById(firstSession?.SessionId ?? 0), _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            condition: mySessionsResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, mySessionsResponse));
        Assert.True(sessions is not null, $"Response body could not be deserialized into a List<Session>. Body: {mySessionsResponseBody}");
        Assert.NotEmpty(sessions!);
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, response));
        Session? result = JsonSerializer.Deserialize<Session>(responseBody, _apiFactory.JsonSerializerOptions);
        Assert.True(result is not null, $"Response body could not be deserialized into a Session. Body: {responseBody}");
        Assert.Equal(firstSession!.SessionId, result!.SessionId);
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

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
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

        // Act
        string user1SessionsResponseBody = await user1SessionsResponse.Content.ReadAsStringAsync(_cancelToken);
        List<Session>? user1Sessions = JsonSerializer.Deserialize<List<Session>>(user1SessionsResponseBody, _apiFactory.JsonSerializerOptions);
        Session? session = user1Sessions?.FirstOrDefault();
        HttpResponseMessage response =
            await user2.GetAsync(Routes.Session.Generate.SessionById(session?.SessionId ?? 0), _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            condition: user1SessionsResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, user1SessionsResponse));
        Assert.True(user1Sessions is not null, $"Response body could not be deserialized into a List<Session>. Body: {user1SessionsResponseBody}");
        Assert.NotEmpty(user1Sessions!);
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
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

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
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

        GenericActions.Auth.AttachBearer(secondClient, secondLogin?.Token);

        HttpResponseMessage sessionsResponse =
            await primaryClient.GetAsync(Routes.Session.MySessions, _cancelToken);
        string sessionsResponseBody = await sessionsResponse.Content.ReadAsStringAsync(_cancelToken);

        // Act
        List<Session>? sessions = JsonSerializer.Deserialize<List<Session>>(sessionsResponseBody, _apiFactory.JsonSerializerOptions);
        Session? sessionToRevoke = sessions?.LastOrDefault();
        HttpResponseMessage revokeResponse =
            await primaryClient.PostAsync(Routes.Session.Generate.RevokeSession(sessionToRevoke?.SessionId ?? 0), null, _cancelToken);
        string revokeResponseBody = await revokeResponse.Content.ReadAsStringAsync(_cancelToken);
        HttpResponseMessage getResponse =
            await primaryClient.GetAsync(Routes.Session.Generate.SessionById(sessionToRevoke?.SessionId ?? 0), _cancelToken);

        // Assert
        Assert.True(
            condition: sessionsResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, sessionsResponse));
        Assert.True(sessions is not null, $"Response body could not be deserialized into a List<Session>. Body: {sessionsResponseBody}");
        Assert.NotEmpty(sessions!);
        Assert.True(
            revokeResponse.StatusCode == HttpStatusCode.OK ||
            revokeResponse.StatusCode == HttpStatusCode.NoContent,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.NoContent}, actual {revokeResponse.StatusCode}, endpoint {revokeResponse.RequestMessage?.RequestUri}, raw body: {revokeResponseBody}");
        Assert.True(
            condition: getResponse.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, getResponse));
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

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
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

        // Act
        string user1SessionsResponseBody = await user1SessionsResponse.Content.ReadAsStringAsync(_cancelToken);
        List<Session>? user1Sessions = JsonSerializer.Deserialize<List<Session>>(user1SessionsResponseBody, _apiFactory.JsonSerializerOptions);
        Session? session = user1Sessions?.FirstOrDefault();
        HttpResponseMessage response =
            await user2.PostAsync(Routes.Session.Generate.RevokeSession(session?.SessionId ?? 0), null, _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            condition: user1SessionsResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, user1SessionsResponse));
        Assert.True(user1Sessions is not null, $"Response body could not be deserialized into a List<Session>. Body: {user1SessionsResponseBody}");
        Assert.NotEmpty(user1Sessions!);
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected status {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
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

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
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

        GenericActions.Auth.AttachBearer(client2, loginResult?.Token);

        HttpResponseMessage beforeResponse =
            await client1.GetAsync(Routes.Session.MySessions, _cancelToken);
        string beforeResponseBody = await beforeResponse.Content.ReadAsStringAsync(_cancelToken);

        // Act
        List<Session>? beforeSessions = JsonSerializer.Deserialize<List<Session>>(beforeResponseBody, _apiFactory.JsonSerializerOptions);
        HttpResponseMessage revokeAllResponse =
            await client1.PostAsync(Routes.Session.RevokeAll, null, _cancelToken);
        string revokeAllResponseBody = await revokeAllResponse.Content.ReadAsStringAsync(_cancelToken);
        HttpResponseMessage afterResponse =
            await client1.GetAsync(Routes.Session.MySessions, _cancelToken);

        HttpResponseMessage oldOtherSessionRequest =
            await client2.GetAsync(Routes.Session.MySessions, _cancelToken);
        string oldOtherSessionRequestBody = await oldOtherSessionRequest.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            condition: beforeResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, beforeResponse));
        Assert.True(beforeSessions is not null, $"Response body could not be deserialized into a List<Session>. Body: {beforeResponseBody}");
        Assert.True(beforeSessions!.Count >= 2);
        Assert.True(
            revokeAllResponse.StatusCode == HttpStatusCode.OK ||
            revokeAllResponse.StatusCode == HttpStatusCode.NoContent,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.NoContent}, actual {revokeAllResponse.StatusCode}, endpoint {revokeAllResponse.RequestMessage?.RequestUri}, raw body: {revokeAllResponseBody}");
        Assert.True(
            condition: afterResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, afterResponse));
        Assert.True(
            oldOtherSessionRequest.StatusCode == HttpStatusCode.Unauthorized ||
            oldOtherSessionRequest.StatusCode == HttpStatusCode.Forbidden,
            $"Expected status {HttpStatusCode.Unauthorized} or {HttpStatusCode.Forbidden}, actual {oldOtherSessionRequest.StatusCode}, endpoint {oldOtherSessionRequest.RequestMessage?.RequestUri}, raw body: {oldOtherSessionRequestBody}");
    }

    #endregion
}