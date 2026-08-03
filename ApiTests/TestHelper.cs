using System.Net;

namespace ApiTests;

public static class TestHelper
{
    /// <summary>
    /// Get Test Error Message
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public static async Task<string> TMsg(HttpStatusCode expected, HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);

        string responseBody = await response.Content.ReadAsStringAsync() ?? "";

        return
            $"Expected status {expected}, " +
            $"actual {response.StatusCode}, " +
            $"endpoint {response.RequestMessage?.RequestUri}, " +
            $"raw body: {responseBody}";
    }
}
