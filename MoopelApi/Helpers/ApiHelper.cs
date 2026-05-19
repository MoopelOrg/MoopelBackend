using System.Net;
using System.Net.Http.Headers;

namespace MoopelApi.Helpers;

public static class ApiHelper
{
    public async static Task<T?> GetClassFromResponse<T>(HttpResponseMessage response, CancellationToken cancelToken)
        where T : class
    {
        if (response is null)
            return null;

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return await response.Content.ReadFromJsonAsync<T>(cancelToken);
        }

        return null;
    }

    public static HttpClient AttachTokenToClient(HttpClient client, string? token)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token ?? "");
        return client;
    }
}
