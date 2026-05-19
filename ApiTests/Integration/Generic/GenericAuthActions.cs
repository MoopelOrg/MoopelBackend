using System.Net.Http.Json;

using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Helpers;
using MoopelApi.Models.Dto.Read;
using MoopelApi.Models.Requests;
using MoopelApi.Models.Results;

namespace ApiTests.Integration.Generic;

public static class GenericAuthActions
{
    public async static Task<LoginResult?> Login(HttpClient client, LoginRequest request, CancellationToken cancelToken)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Login, request, cancelToken);
        return await ApiHelper.GetClassFromResponse<LoginResult>(response, cancelToken);
    }

    public async static Task<LoginResult?> GuestLogin(HttpClient client, CancellationToken cancelToken)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.GuestLogin, cancelToken);
        return await ApiHelper.GetClassFromResponse<LoginResult>(response, cancelToken);
    }

    public async static Task<UserRead?> Register(HttpClient client, RegistrationRequest request, CancellationToken cancelToken)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Register, request, cancelToken);
        return await ApiHelper.GetClassFromResponse<UserRead>(response, cancelToken);
    }

    public static HttpClient AttachBearer(HttpClient client, string? token)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token ?? "");
        return client;
    }

    public async static Task<HttpClient> CreateAuthenticatedClient(HttpClient client, string username, string password, CancellationToken cancelToken)
    {
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

        UserRead? user = await Register(client, registerRequest, cancelToken);
        Assert.NotNull(user);

        LoginResult? loginResult = await Login(client, loginRequest, cancelToken);
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);

        AttachBearer(client, loginResult.Token);

        return client;
    }

    public async static Task<HttpClient> CreateAuthenticatedClient(HttpClient client, CancellationToken cancelToken)
    {
        string userName = AuthTestData.ValidUserName + Guid.NewGuid();
        string password = AuthTestData.ValidPassword + Guid.NewGuid();

        return await CreateAuthenticatedClient(client, userName, password, cancelToken);
    }
}
