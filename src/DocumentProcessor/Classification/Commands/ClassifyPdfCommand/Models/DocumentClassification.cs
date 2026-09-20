using System.Text.Json.Serialization;
using DocumentProcessor.Classification.Constants;

namespace DocumentProcessor.Classification.Commands.ClassifyPdfCommand.Models;

public class DocumentClassification
{
    public const string DocumentTypeJsonName = "document_type";
    public const string ConfidenceJsonName = "confidence";
    public const string ReasoningJsonName = "reasoning";

    [JsonPropertyName(DocumentTypeJsonName)]
    public string DocumentType { get; set; } = DocumentTypes.Other;
    
    [JsonPropertyName(ConfidenceJsonName)]
    public double Confidence { get; set; }
    
    [JsonPropertyName(ReasoningJsonName)]
    public string Reasoning { get; set; } = string.Empty;
    
    [JsonPropertyName("successful")]
    public bool Successful { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
