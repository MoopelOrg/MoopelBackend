using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MoopelApi.Helpers;

public static class ApiHelper
{


    public async static Task<T?> GetClassFromResponse<T>(
        HttpResponseMessage response,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancelToken)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(jsonSerializerOptions);

        if (response is null)
            return null;

        if (response.StatusCode == HttpStatusCode.OK)
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<T>(jsonSerializerOptions, cancelToken);
            }
            catch (Exception ex)
            {
                string content = await response.Content.ReadAsStringAsync(cancelToken);

                Console.WriteLine($"{content} had parse error: {ex.Message}");
            }
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
