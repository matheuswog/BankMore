namespace BankMore.Transferencia.Domain.Interfaces;

public interface IHttpClientService
{
    Task<HttpResponseMessage> GetAsync(string requestUri, string? token = null);
    Task<HttpResponseMessage> PostAsync(string requestUri, object content, string? token = null);
}

