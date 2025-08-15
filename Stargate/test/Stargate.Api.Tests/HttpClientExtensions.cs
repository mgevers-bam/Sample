using Newtonsoft.Json;
using System.Text;

namespace Stargate.Api.Tests;

public static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, string url, object? body = null, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        if (body != null)
        {
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        }

        return SendAsync(httpClient, request, cancellationToken);
    }

    public static Task<HttpResponseMessage> PostAsync(this HttpClient httpClient, string url, object body, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
        };

        return SendAsync(httpClient, request, cancellationToken);
    }

    public static Task<HttpResponseMessage> PutAsync(this HttpClient httpClient, string url, object body, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
        };

        return SendAsync(httpClient, request, cancellationToken);
    }

    public static Task<HttpResponseMessage> DeleteAsync(this HttpClient httpClient, string url, CancellationToken cancellationToken = default)
    {
        return SendAsync(httpClient, new HttpRequestMessage(HttpMethod.Delete, url), cancellationToken);
    }

    public static Task<HttpResponseMessage> DeleteAsync(this HttpClient httpClient, string url, object body, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
        };

        return SendAsync(httpClient, request, cancellationToken);
    }

    public static async Task<T> ConvertContent<T>(this HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(json)!;
    }

    private static async Task<HttpResponseMessage> SendAsync(HttpClient httpClient, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
        }
    }
}
