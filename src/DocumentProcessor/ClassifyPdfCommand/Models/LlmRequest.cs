using System.Text.Json.Serialization;

namespace DocumentProcessor.ClassifyPdfCommand.Models;

public class LlmRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
    
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;
    
    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}