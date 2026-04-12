using System.Net.Http.Json;
using MoopelApi;
using MoopelApi.Helpers;
using MoopelApi.Models.Dto;
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

    public async static Task<UserRead?> Register(HttpClient client, RegistrationRequest request, CancellationToken cancelToken)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Register, request, cancelToken);
        return await ApiHelper.GetClassFromResponse<UserRead>(response, cancelToken);
    }
}
