using BankMore.Transferencia.Domain.Interfaces;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace BankMore.Transferencia.Infrastructure.Services;

public class HttpClientService : IHttpClientService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpClientService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HttpResponseMessage> GetAsync(string requestUri, string? token = null)
    {
        var httpClient = _httpClientFactory.CreateClient("ContaCorrenteApi");
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        
        return await httpClient.SendAsync(request);
    }

    public async Task<HttpResponseMessage> PostAsync(string requestUri, object content, string? token = null)
    {
        var httpClient = _httpClientFactory.CreateClient("ContaCorrenteApi");
        var json = JsonSerializer.Serialize(content);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = httpContent
        };
        
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        
        return await httpClient.SendAsync(request);
    }
}

