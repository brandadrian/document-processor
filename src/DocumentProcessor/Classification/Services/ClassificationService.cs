using System.Net.Http.Json;
using System.Text.Json;
using DocumentProcessor.Classification.Commands.ClassifyPdfCommand.Models;
using DocumentProcessor.Classification.Constants;
using DocumentProcessor.Shared.Models.Ollama;
using Microsoft.Extensions.Logging;

namespace DocumentProcessor.Classification.Services;

public class ClassificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClassificationService> _logger;

    public ClassificationService(HttpClient httpClient, ILogger<ClassificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<DocumentClassification> ClassifyDocument(string text, string model, string llmUrl, CancellationToken cancellationToken)
    {
        var responseMessage = string.Empty;
        
        try
        {
            var requestPayload = new OllamaGenerateRequest
            {
                Model = model,
                System = await ClassificationPromptBuilder.BuildSystemPrompt(cancellationToken),
                Prompt = ClassificationPromptBuilder.BuildPrompt(text),
                Stream = false,
                Think = false,
                Format = new OllamaJsonSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OllamaJsonSchema>
                    {
                        [DocumentClassification.DocumentTypeJsonName] = new()
                        {
                            Type = "string",
                            Enum = DocumentTypes.Available
                        },
                        [DocumentClassification.ConfidenceJsonName] = new()
                        {
                            Type = "number",
                            Minimum = 0,
                            Maximum = 1
                        },
                        [DocumentClassification.ReasoningJsonName] = new() { Type = "string" }
                    },
                    Required =
                    [
                        DocumentClassification.DocumentTypeJsonName,
                        DocumentClassification.ConfidenceJsonName,
                        DocumentClassification.ReasoningJsonName
                    ],
                    AdditionalProperties = false
                },
                Options = new OllamaGenerateOptions { Temperature = 0 }
            };
                
            var response = await _httpClient.PostAsJsonAsync(llmUrl, requestPayload, cancellationToken);
            responseMessage = response.StatusCode.ToString();
                
            if (response.IsSuccessStatusCode)
            {
                var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken);
                if (!string.IsNullOrWhiteSpace(ollamaResponse?.Error))
                {
                    throw new InvalidDataException($"Ollama classification failed: {ollamaResponse.Error}");
                }

                if (ollamaResponse is null || string.IsNullOrWhiteSpace(ollamaResponse.Response))
                {
                    throw new InvalidDataException("Ollama returned no classification response.");
                }

                var result = JsonSerializer.Deserialize<DocumentClassification>(ollamaResponse.Response) ??
                             throw new InvalidDataException("Ollama returned invalid classification JSON.");
                if (!DocumentTypes.Available.Contains(result.DocumentType, StringComparer.Ordinal) ||
                    result.Confidence is < 0 or > 1 ||
                    string.IsNullOrWhiteSpace(result.Reasoning))
                {
                    throw new InvalidDataException("Ollama returned an invalid classification shape.");
                }

                result.Successful = true;
                result.Message = "success";
            
                return result;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during classification.");
            responseMessage = e.Message;
        }
        
        return new DocumentClassification() 
        {
            Confidence = 0.0,
            DocumentType = DocumentTypes.Other,
            Reasoning = "Failed to classify document!",
            Successful = false,
            Message = responseMessage
        };
    }
}