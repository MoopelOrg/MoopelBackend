using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter());

            try
            {
                return await response.Content.ReadFromJsonAsync<T>(options, cancelToken);
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
