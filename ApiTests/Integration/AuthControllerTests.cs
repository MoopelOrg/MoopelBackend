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
        UserRead? user = await GenericAuthActions.Register(client, registerRequest, _cancelToken);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(username, user.Username);
        Assert.True(user.UserId > 0);
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
        UserRead? user = await GenericAuthActions.Register(client, registerRequest, _cancelToken);

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
        UserRead? firstRegistrationUser = await GenericAuthActions.Register(client, registerRequest, _cancelToken);
        UserRead? secondRegistrationUser = await GenericAuthActions.Register(client, registerRequest, _cancelToken);

        // Assert
        Assert.NotNull(firstRegistrationUser);
        Assert.Equal(username, firstRegistrationUser.Username);
        Assert.Null(secondRegistrationUser);
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
        UserRead? firstUser = await GenericAuthActions.Register(client, registerRequest, _cancelToken);
        UserRead? secondUser = await GenericAuthActions.Register(client, registerRequest, _cancelToken);
        LoginResult? loginResult = await GenericAuthActions.Login(client, loginRequest, _cancelToken);

        // Assert
        Assert.NotNull(firstUser);
        Assert.Null(secondUser);

        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.User);
        Assert.NotNull(loginResult.Token);
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
        UserRead? user = await GenericAuthActions.Register(client, registerRequest, _cancelToken);
        LoginResult? loginResult = await GenericAuthActions.Login(client, loginRequest, _cancelToken);

        // Assert
        Assert.NotNull(user);
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.User);
        Assert.NotNull(loginResult.Token);
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
        LoginResult? loginResult = await GenericAuthActions.GuestLogin(client, _cancelToken);

        // Assert
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.User);
        Assert.NotNull(loginResult.Token);
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

        await GenericAuthActions.Register(client, registerRequest, _cancelToken);

        // Act
        HttpResponseMessage loginResponse =
            await client.PostAsJsonAsync(Routes.Auth.Login, invalidLoginRequest, _cancelToken);

        LoginResult? loginResult =
            await loginResponse.Content.ReadFromJsonAsync<LoginResult>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
        Assert.NotNull(loginResult);
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
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
        Assert.NotNull(loginResult);
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

        await GenericAuthActions.Register(
            client,
            new RegistrationRequest { Username = username, Password = password },
            _cancelToken);

        // Act
        LoginResult? loginResult = await GenericAuthActions.Login(
            client,
            new LoginRequest { Username = username, Password = password },
            _cancelToken);

        // Assert
        Assert.NotNull(loginResult);
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
        UserRead? user = await GenericAuthActions.Register(client, registerRequest, _cancelToken);
        LoginResult? loginResult = await GenericAuthActions.Login(client, loginRequest, _cancelToken);

        Assert.NotNull(loginResult);
        GenericAuthActions.AttachBearer(client, loginResult.Token);

        HttpResponseMessage meResponse = await client.GetAsync(Routes.Auth.Me, _cancelToken);
        UserRead? meResult = await meResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.NotNull(user);
        Assert.NotNull(loginResult.User);
        Assert.NotNull(meResult);

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
        Assert.Equal(HttpStatusCode.Unauthorized, meResponse.StatusCode);
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
        Assert.Equal(HttpStatusCode.Unauthorized, meResponse.StatusCode);
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
        UserRead? registeredUser = await GenericAuthActions.Register(
            client,
            new RegistrationRequest { Username = username, Password = password },
            _cancelToken);

        LoginResult? loginResult = await GenericAuthActions.Login(
            client,
            new LoginRequest { Username = username, Password = password },
            _cancelToken);

        Assert.NotNull(loginResult);
        GenericAuthActions.AttachBearer(client, loginResult.Token);

        UserRead? me = await client.GetFromJsonAsync<UserRead>(Routes.Auth.Me, _cancelToken);

        // Assert
        Assert.NotNull(registeredUser);
        Assert.NotNull(loginResult.User);
        Assert.NotNull(me);

        Assert.Equal(registeredUser.UserId, loginResult.User.UserId);
        Assert.Equal(registeredUser.UserId, me.UserId);

        Assert.Equal(registeredUser.Username, loginResult.User.Username);
        Assert.Equal(registeredUser.Username, me.Username);
    }

    #endregion
}