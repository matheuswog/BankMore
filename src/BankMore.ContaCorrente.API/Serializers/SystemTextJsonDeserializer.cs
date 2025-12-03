using KafkaFlow;
using System.Text;
using System.Text.Json;

namespace BankMore.ContaCorrente.API.Serializers;

public class SystemTextJsonDeserializer : IDeserializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Task<object> DeserializeAsync(Stream input, Type type, ISerializerContext context)
    {
        using var reader = new StreamReader(input);
        var json = reader.ReadToEnd();
        var result = JsonSerializer.Deserialize(json, type, Options);
        return Task.FromResult(result ?? throw new InvalidOperationException("Deserialization failed"));
    }
}

