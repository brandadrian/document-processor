using System.Text.Json.Serialization;

namespace DocumentProcessor.Shared.Models.Ollama;

public sealed class OllamaGenerateResponse
{
    [JsonPropertyName("response")]
    public string Response { get; init; } = string.Empty;

    [JsonPropertyName("error")]
    public string? Error { get; init; }
}
