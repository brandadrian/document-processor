using System.Text.Json.Serialization;

namespace DocumentProcessor.ClassifyPdfCommand.Models;

public class DocumentClassification
{
    [JsonPropertyName("document_type")]
    public string DocumentType { get; set; } = "OTHER";
    
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
    
    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;
    
    [JsonPropertyName("successful")]
    public bool Successful { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
