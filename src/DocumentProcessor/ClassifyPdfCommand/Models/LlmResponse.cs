using System.Text.Json.Serialization;

namespace DocumentProcessor.ClassifyPdfCommand.Models;

public class LlmResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;
}