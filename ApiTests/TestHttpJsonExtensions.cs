using System.Net.Http.Json;

namespace ApiTests;

public static class TestHttpJsonExtensions
{
    public static Task<TValue?> ReadFromJsonAsync<TValue>(
        this HttpContent content,
        CancellationToken cancellationToken = default)
    {
        return HttpContentJsonExtensions.ReadFromJsonAsync<TValue>(
            content,
            MoopelApiFactory.SharedJsonSerializerOptions,
            cancellationToken);
    }

    public static Task<HttpResponseMessage> PostAsJsonAsync<TValue>(
        this HttpClient client,
        string? requestUri,
        TValue value,
        CancellationToken cancellationToken = default)
    {
        return HttpClientJsonExtensions.PostAsJsonAsync(
            client,
            requestUri,
            value,
            MoopelApiFactory.SharedJsonSerializerOptions,
            cancellationToken);
    }

    public static Task<HttpResponseMessage> PutAsJsonAsync<TValue>(
        this HttpClient client,
        string? requestUri,
        TValue value,
        CancellationToken cancellationToken = default)
    {
        return HttpClientJsonExtensions.PutAsJsonAsync(
            client,
            requestUri,
            value,
            MoopelApiFactory.SharedJsonSerializerOptions,
            cancellationToken);
    }

    public static Task<HttpResponseMessage> PatchAsJsonAsync<TValue>(
        this HttpClient client,
        string? requestUri,
        TValue value,
        CancellationToken cancellationToken = default)
    {
        return HttpClientJsonExtensions.PatchAsJsonAsync(
            client,
            requestUri,
            value,
            MoopelApiFactory.SharedJsonSerializerOptions,
            cancellationToken);
    }
}
