using System.Net.Http.Json;

using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Helpers;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Dto.Read;
using MoopelApi.Models.Requests;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Results;

namespace ApiTests.Integration.Generic;

public static class GenericActions
{
    public static class Tag
    {
        public static async Task<UserTag> CreateUserTag(HttpClient client, TagCreateRequest request,
            CancellationToken cancelToken)
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync(Routes.Tag.CreateUserTag, request, cancelToken);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

            UserTag? created = await response.Content.ReadFromJsonAsync<UserTag>(cancelToken);
            Assert.NotNull(created);

            return created;
        }

        public static async Task<Group> CreateGroup(HttpClient client, CancellationToken cancelToken)
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync(Routes.Group.CreateGroup, GroupTestData.ValidCreateRequest, cancelToken);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            Group? created = await response.Content.ReadFromJsonAsync<Group>(cancelToken);
            Assert.NotNull(created);

            return created;
        }

        public static async Task<GroupTag> CreateGroupTag(HttpClient client, int groupId, TagCreateRequest request, CancellationToken cancelToken)
        {
            string endpoint = Routes.Tag.Generate.CreateGroupTag(groupId);
            HttpResponseMessage response = await client.PostAsJsonAsync(endpoint, request, cancelToken);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

            GroupTag? created = await response.Content.ReadFromJsonAsync<GroupTag>(cancelToken);
            Assert.NotNull(created);

            return created;
        }
    }

    public static class Auth
    {
        public static async Task<LoginResult?> Login(HttpClient client, LoginRequest request, CancellationToken cancelToken)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Login, request, cancelToken);

            return await ApiHelper.GetClassFromResponse<LoginResult>(response, cancelToken);
        }

        public static async Task<LoginResult?> GuestLogin(HttpClient client, CancellationToken cancelToken)
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync(Routes.Auth.GuestLogin, cancelToken, cancellationToken: cancelToken);
            return await ApiHelper.GetClassFromResponse<LoginResult>(response, cancelToken);
        }

        public static async Task<UserRead?> Register(HttpClient client, RegistrationRequest request,
            CancellationToken cancelToken)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Register, request, cancelToken);

            return await ApiHelper.GetClassFromResponse<UserRead>(response, cancelToken);
        }

        public static HttpClient AttachBearer(HttpClient client, string? token)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token ?? "");
            return client;
        }

        public static async Task<HttpClient> CreateAuthenticatedClient(HttpClient client, string username,
            string password, CancellationToken cancelToken)
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

        public static async Task<HttpClient> CreateAuthenticatedClient(HttpClient client, CancellationToken cancelToken)
        {
            string userName = AuthTestData.ValidUserName + Guid.NewGuid();
            string password = AuthTestData.ValidPassword + Guid.NewGuid();

            return await CreateAuthenticatedClient(client, userName, password, cancelToken);
        }
    }
}
