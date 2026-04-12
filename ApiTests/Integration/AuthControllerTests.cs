using System.Net;
using System.Net.Http.Json;
using ApiTests.Integration.Generic;
using ApiTests.TestData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MoopelApi;
using MoopelApi.Helpers;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests;
using MoopelApi.Models.Results;
using Org.BouncyCastle.Asn1.Ocsp;
using static MoopelApi.ConstantValues;

namespace ApiTests.Integration;

public class AuthControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly JwtSettings _jwtSettings;
    private readonly CancellationToken _cancelToken;

    public AuthControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;

        using var scope = _apiFactory.Services.CreateScope();
        var tokenSettings = scope.ServiceProvider.GetRequiredService<IOptions<JwtSettings>>();
        _jwtSettings = tokenSettings.Value;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task Login_ReturnsToken_AfterRegisteringAndLoggingInWithValidCredentials(string username, string password)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.ResetDatabaseAsync();
        RegistrationRequest registerRequest = new() { Username = username, Password = password };
        LoginRequest loginRequest = new() { Username = username, Password = password };

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

        Assert.StartsWith("ey", loginResult.Token);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.InValidUsers), MemberType = typeof(AuthTestData))]
    public async Task Register_Fails_WhenRegisteringWithInvalidCredentials(string username, string password, string reason)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        RegistrationRequest registerRequest = new() { Username = username, Password = password };

        // Act
        UserRead? user = await GenericAuthActions.Register(client, registerRequest, _cancelToken);

        // Assert
        Assert.True(user is null, reason);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task Register_Fails_WhenRegisteringValidUserWithSameName(string username, string password)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.ResetDatabaseAsync();
        RegistrationRequest registerRequest = new() { Username = username, Password = password };

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
    public async Task Me_ReturnsMyInfo_AfterRegisteringAndLoggingInWithValidCredentials(string username, string password)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.ResetDatabaseAsync();
        RegistrationRequest registerRequest = new() { Username = username, Password = password };
        LoginRequest loginRequest = new() { Username = username, Password = password };

        // Act
        UserRead? user = await GenericAuthActions.Register(client, registerRequest, _cancelToken);
        LoginResult? loginResult = await GenericAuthActions.Login(client, loginRequest, _cancelToken);
        HttpResponseMessage meResponse = await client.GetAsync(Routes.Auth.Me, _cancelToken);
        UserRead? meResult = await meResponse.Content.ReadFromJsonAsync<UserRead>(_cancelToken);

        // Assert
        Assert.NotNull(user);
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.User);
        Assert.NotNull(meResult);
        Assert.Equal(username, user.Username);
        Assert.Equal(username, loginResult.User.Username);
        Assert.Equal(username, meResult.Username);
    }
}
