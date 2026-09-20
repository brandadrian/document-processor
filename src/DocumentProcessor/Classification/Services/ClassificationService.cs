using System.Net.Http.Json;
using System.Text.Json;
using DocumentProcessor.Classification.Commands.ClassifyPdfCommand.Models;
using DocumentProcessor.Classification.Constants;
using DocumentProcessor.Shared.Models.Ollama;

namespace DocumentProcessor.Classification.Services;

public class ClassificationService
{
    private readonly HttpClient _httpClient;

    public ClassificationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<DocumentClassification> ClassifyDocument(string text, string model, string llmUrl, CancellationToken cancellationToken)
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
                    [nameof(DocumentClassification.DocumentType)] = new()
                    {
                        Type = "string",
                        Enum = DocumentTypes.Available
                    },
                    [nameof(DocumentClassification.Confidence)] = new()
                    {
                        Type = "number",
                        Minimum = 0,
                        Maximum = 1
                    },
                    [nameof(DocumentClassification.Reasoning)] = new() { Type = "string" }
                },
                Required =
                [
                    nameof(DocumentClassification.DocumentType),
                    nameof(DocumentClassification.Confidence),
                    nameof(DocumentClassification.Reasoning)
                ],
                AdditionalProperties = false
            },
            Options = new OllamaGenerateOptions { Temperature = 0 }
        };
            
        var response = await _httpClient.PostAsJsonAsync(llmUrl, requestPayload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidDataException($"Ollama classification failed with HTTP status {response.StatusCode}.");
        }
            
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

        return result;
    }
}