using System.Net.Http.Json;
using System.Text.Json;
using DocumentProcessor.Extraction.Models;
using DocumentProcessor.Shared.Models.Ollama;

namespace DocumentProcessor.ImageExtraction.Services;

public class ImageExtractionService
{
    private readonly HttpClient _httpClient;

    public ImageExtractionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExtractedField[]> ExtractData(
        IReadOnlyList<string> inputImagesBase64,
        string[] fieldsToExtract,
        string type,
        string model,
        string ollamaUrl,
        CancellationToken cancellationToken)
    {
        var systemPrompt = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "prompts", "image-extraction", type, "extraction_prompt.txt"),
            cancellationToken);
        
        var payload = new OllamaGenerateRequest
        {
            Model = model,
            System = systemPrompt,
            Prompt = ImageExtractionPromptBuilder.Build(fieldsToExtract, inputImagesBase64.Count),
            Images = inputImagesBase64.ToArray(),
            Stream = false,
            Think = false,
            Format = new OllamaJsonSchema
            {
                Type = "object",
                Properties = fieldsToExtract.ToDictionary(
                    field => field,
                    _ => new OllamaJsonSchema { Type = "string" },
                    StringComparer.Ordinal),
                Required = fieldsToExtract,
                AdditionalProperties = false
            },
            Options = new OllamaGenerateOptions
            {
                Temperature = 0,
                ContextLength = 32768,
                MaxPredictedTokens = 8192
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(ollamaUrl, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken);
        if (!string.IsNullOrWhiteSpace(envelope?.Error))
        {
            throw new InvalidDataException($"Ollama image extraction failed: {envelope.Error}");
        }

        if (envelope is null || string.IsNullOrWhiteSpace(envelope.Response))
        {
            throw new InvalidDataException("Ollama returned no image extraction response.");
        }

        using var extracted = JsonDocument.Parse(envelope.Response);
        if (extracted.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException("Ollama must return a JSON object of field values.");
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in extracted.RootElement.EnumerateObject())
        {
            if (!fieldsToExtract.Contains(property.Name, StringComparer.Ordinal) ||
                property.Value.ValueKind != JsonValueKind.String ||
                !values.TryAdd(property.Name, property.Value.GetString()!))
            {
                throw new InvalidDataException($"Ollama returned an invalid or duplicate field: {property.Name}");
            }
        }

        if (values.Count != fieldsToExtract.Length)
        {
            throw new InvalidDataException("Ollama omitted required fields. Try a larger model.");
        }

        return fieldsToExtract.Select(field => new ExtractedField(field, values[field])).ToArray();
    }
}
