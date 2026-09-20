using System.Text.Json.Serialization;

namespace DocumentProcessor.Shared.Models.Ollama;

public sealed class OllamaGenerateOptions
{
    [JsonPropertyName("temperature")]
    public double? Temperature { get; init; }

    [JsonPropertyName("num_ctx")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ContextLength { get; init; }

    [JsonPropertyName("num_predict")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxPredictedTokens { get; init; }
}
