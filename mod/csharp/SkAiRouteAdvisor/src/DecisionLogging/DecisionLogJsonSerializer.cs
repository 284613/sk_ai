using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkAiRouteAdvisor.DecisionLogging;

internal sealed class DecisionLogJsonSerializer
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = false,
    };

    public string Serialize(DecisionLogEntry entry)
    {
        return JsonSerializer.Serialize(entry, _options);
    }

    public string SerializeIndented(DecisionLogEntry entry)
    {
        var options = new JsonSerializerOptions(_options)
        {
            WriteIndented = true,
        };
        return JsonSerializer.Serialize(entry, options);
    }
}
