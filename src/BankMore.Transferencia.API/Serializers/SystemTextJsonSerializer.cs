using KafkaFlow;
using System.Text;
using System.Text.Json;

namespace BankMore.Transferencia.API.Serializers;

public class SystemTextJsonSerializer : ISerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Task SerializeAsync(object message, Stream output, ISerializerContext context)
    {
        var json = JsonSerializer.Serialize(message, Options);
        var bytes = Encoding.UTF8.GetBytes(json);
        return output.WriteAsync(bytes, 0, bytes.Length);
    }

    public Task<object> DeserializeAsync(Stream input, Type type, ISerializerContext context)
    {
        using var reader = new StreamReader(input);
        var json = reader.ReadToEnd();
        var result = JsonSerializer.Deserialize(json, type, Options);
        return Task.FromResult(result ?? throw new InvalidOperationException("Deserialization failed"));
    }
}

