using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Dto.Read;
using MoopelApi.Models.Requests;
using MoopelApi.Models.Results;

namespace ApiTests.Integration;

public class AuthControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public AuthControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    #region Register

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task Register_ReturnsUser_WhenCredentialsAreValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        RegistrationRequest registerRequest = new()
        {
            Username = username,
            Password = password
        };

        // Act
        UserRead? user = await GenericActions.Auth.Register(client, registerRequest, _cancelToken);

        // Assert
        Assert.True(user is not null, $"Registration failed for username '{username}'.");
        Assert.Equal(username, user.Username);
        Assert.True(user.UserId > 0, $"Expected a positive UserId but got {user.UserId}");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.InValidUsers), MemberType = typeof(AuthTestData))]
    public async Task Register_Fails_WhenRegisteringWithInvalidCredentials(string username, string password, string reason)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        RegistrationRequest registerRequest = new()
        {
            Username = username,
            Password = password
        };

        // Act
        UserRead? user = await GenericActions.Auth.Register(client, registerRequest, _cancelToken);

        // Assert
        Assert.True(user is null, reason);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task Register_Fails_WhenUsernameAlreadyExists(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        RegistrationRequest registerRequest = new()
        {
            Username = username,
            Password = password
        };

        // Act
        UserRead? firstRegistrationUser = await GenericActions.Auth.Register(client, registerRequest, _cancelToken);
        UserRead? secondRegistrationUser = await GenericActions.Auth.Register(client, registerRequest, _cancelToken);

        // Assert
        Assert.True(firstRegistrationUser is not null, $"First registration failed for username '{username}'.");
        Assert.Equal(username, firstRegistrationUser.Username);
        Assert.True(secondRegistrationUser is null, $"Expected duplicate registration for '{username}' to fail but it succeeded.");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DuplicateRegistration_DoesNotBreakOriginalLogin(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

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

        // Act
        UserRead? firstUser = await GenericActions.Auth.Register(client, registerRequest, _cancelToken);
        UserRead? secondUser = await GenericActions.Auth.Register(client, registerRequest, _cancelToken);
        LoginResult? loginResult = await GenericActions.Auth.Login(client, loginRequest, _cancelToken);

        // Assert
        Assert.True(firstUser is not null, $"First registration failed for username '{username}'.");
        Assert.True(secondUser is null, $"Expected duplicate registration for '{username}' to fail but it succeeded.");

        Assert.True(loginResult is not null, $"Login failed for username '{username}' after duplicate registration attempt.");
        Assert.True(loginResult.User is not null, $"Login result had no user for '{username}'.");
        Assert.True(loginResult.Token is not null, $"Login result had no token for '{username}'.");
        Assert.Equal(username, loginResult.User.Username);
    }

    #endregion

    #region Login

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task Login_ReturnsToken_AfterRegisteringAndLoggingInWithValidCredentials(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

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

        // Act
        UserRead? user = await GenericActions.Auth.Register(client, registerRequest, _cancelToken);
        LoginResult? loginResult = await GenericActions.Auth.Login(client, loginRequest, _cancelToken);

        // Assert
        Assert.True(user is not null, $"Registration failed for username '{username}'.");
        Assert.True(loginResult is not null, $"Login failed for username '{username}'.");
        Assert.True(loginResult.User is not null, $"Login result had no user for '{username}'.");
        Assert.True(loginResult.Token is not null, $"Login result had no token for '{username}'.");
        Assert.Null(loginResult.Error);

        Assert.Equal(username, user.Username);
        Assert.Equal(username, loginResult.User.Username);

        Assert.False(string.IsNullOrWhiteSpace(loginResult.Token));
        Assert.Equal(3, loginResult.Token.Split('.').Length);
        Assert.StartsWith("ey", loginResult.Token);
    }

    [Fact]
    public async Task GuestLogin_ReturnsToken_WithNoCredentials()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        LoginResult? loginResult = await GenericActions.Auth.GuestLogin(client, _cancelToken);

        // Assert
        Assert.True(loginResult is not null, "Guest login failed.");
        Assert.True(loginResult.User is not null, "Guest login result had no user.");
        Assert.True(loginResult.Token is not null, "Guest login result had no token.");
        Assert.Null(loginResult.Error);

        Assert.StartsWith("Guest_", loginResult.User.Username);

        Assert.False(string.IsNullOrWhiteSpace(loginResult.Token));
        Assert.Equal(3, loginResult.Token.Split('.').Length);
        Assert.StartsWith("ey", loginResult.Token);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task Login_Fails_WhenPasswordIsWrong(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        RegistrationRequest registerRequest = new()
        {
            Username = username,
            Password = password
        };

        LoginRequest invalidLoginRequest = new()
        {
            Username = username,
            Password = password + "_wrong"
        };

        await GenericActions.Auth.Register(client, registerRequest, _cancelToken);

        // Act
        HttpResponseMessage loginResponse =
            await client.PostAsJsonAsync(Routes.Auth.Login, invalidLoginRequest, _cancelToken);

        LoginResult? loginResult =
            await loginResponse.Content.ReadFromJsonAsync<LoginResult>(_cancelToken);

        // Assert
        Assert.True(
            loginResponse.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected {HttpStatusCode.Unauthorized} but got {loginResponse.StatusCode} for username '{username}' with wrong password.");
        Assert.True(loginResult is not null, "Login response could not be deserialized into a LoginResult.");
        Assert.Null(loginResult.User);
        Assert.True(string.IsNullOrWhiteSpace(loginResult.Token));
        Assert.False(string.IsNullOrWhiteSpace(loginResult.Error));
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task Login_Fails_WhenUserDoesNotExist(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        LoginRequest loginRequest = new()
        {
            Username = username,
            Password = password
        };

        // Act
        HttpResponseMessage loginResponse =
            await client.PostAsJsonAsync(Routes.Auth.Login, loginRequest, _cancelToken);

        LoginResult? loginResult =
            await loginResponse.Content.ReadFromJsonAsync<LoginResult>(_cancelToken);

        // Assert
        Assert.True(
            loginResponse.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected {HttpStatusCode.Unauthorized} but got {loginResponse.StatusCode} for non-existent username '{username}'.");
        Assert.True(loginResult is not null, "Login response could not be deserialized into a LoginResult.");
        Assert.Null(loginResult.User);
        Assert.True(string.IsNullOrWhiteSpace(loginResult.Token));
        Assert.False(string.IsNullOrWhiteSpace(loginResult.Error));
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task Login_ReturnsDifferentTokenShapeThanEmptyString(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.Register(
            client,
            new RegistrationRequest { Username = username, Password = password },
            _cancelToken);

        // Act
        LoginResult? loginResult = await GenericActions.Auth.Login(
            client,
            new LoginRequest { Username = username, Password = password },
            _cancelToken);

        // Assert
        Assert.True(loginResult is not null, $"Login failed for username '{username}'.");
        Assert.False(string.IsNullOrWhiteSpace(loginResult.Token));
        Assert.NotEqual(string.Empty, loginResult.Token);
        Assert.Contains(".", loginResult.Token);
    }

    #endregion

    #region Me

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task Me_ReturnsMyInfo_AfterRegisteringAndLoggingInWithValidCredentials(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

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

        // Act
        UserRead? user = await GenericActions.Auth.Register(client, registerRequest, _cancelToken);
        LoginResult? loginResult = await GenericActions.Auth.Login(client, loginRequest, _cancelToken);

        Assert.NotNull(loginResult);
        GenericActions.Auth.AttachBearer(client, loginResult.Token);

        HttpResponseMessage meResponse = await client.GetAsync(Routes.Auth.Me, _cancelToken);
        UserRead? meResult = await meResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        // Assert
        Assert.True(
            meResponse.StatusCode == HttpStatusCode.OK,
            $"Expected {HttpStatusCode.OK} but got {meResponse.StatusCode} for username '{username}'.");
        Assert.True(user is not null, $"Registration failed for username '{username}'.");
        Assert.True(loginResult.User is not null, $"Login result had no user for '{username}'.");
        Assert.True(meResult is not null, $"/me response could not be deserialized for username '{username}'.");

        Assert.Equal(username, user.Username);
        Assert.Equal(username, loginResult.User.Username);
        Assert.Equal(username, meResult.Username);
        Assert.Equal(user.UserId, meResult.UserId);
    }

    [Fact]
    public async Task Me_Fails_WhenNoBearerTokenIsProvided()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage meResponse = await client.GetAsync(Routes.Auth.Me, _cancelToken);

        // Assert
        Assert.True(
            meResponse.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected {HttpStatusCode.Unauthorized} but got {meResponse.StatusCode} when no bearer token was provided.");
    }

    [Fact]
    public async Task Me_Fails_WhenBearerTokenIsInvalid()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "not-a-real-token");

        // Act
        HttpResponseMessage meResponse = await client.GetAsync(Routes.Auth.Me, _cancelToken);

        // Assert
        Assert.True(
            meResponse.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected {HttpStatusCode.Unauthorized} but got {meResponse.StatusCode} when an invalid bearer token was provided.");
    }

    #endregion

    #region Register -> Login -> Me flow

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task Register_Then_Login_Then_Me_IsAConsistentFlow(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        UserRead? registeredUser = await GenericActions.Auth.Register(
            client,
            new RegistrationRequest { Username = username, Password = password },
            _cancelToken);

        LoginResult? loginResult = await GenericActions.Auth.Login(
            client,
            new LoginRequest { Username = username, Password = password },
            _cancelToken);

        Assert.NotNull(loginResult);
        GenericActions.Auth.AttachBearer(client, loginResult.Token);

        UserRead? me = await client.GetFromJsonAsync<UserRead>(Routes.Auth.Me, _cancelToken);

        // Assert
        Assert.True(registeredUser is not null, $"Registration failed for username '{username}'.");
        Assert.True(loginResult.User is not null, $"Login result had no user for '{username}'.");
        Assert.True(me is not null, $"/me response could not be deserialized for username '{username}'.");

        Assert.Equal(registeredUser.UserId, loginResult.User.UserId);
        Assert.Equal(registeredUser.UserId, me.UserId);

        Assert.Equal(registeredUser.Username, loginResult.User.Username);
        Assert.Equal(registeredUser.Username, me.Username);
    }

    #endregion
}