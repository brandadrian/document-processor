using System.Net.Http.Json;
using System.Text.Json;
using DocumentProcessor.Extraction.Models;
using DocumentProcessor.Shared.Models.Ollama;

namespace DocumentProcessor.Extraction.Services;

public class ExtractionService
{
    private readonly HttpClient _httpClient;

    public ExtractionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExtractedField[]> ExtractData(
        string text,
        IEnumerable<ExtractionExample> examples,
        string[] categories,
        string type,
        string model,
        string ollamaUrl,
        CancellationToken cancellationToken)
    {
        var systemPrompt = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "prompts", type, "extraction_prompt.txt"), cancellationToken);
        
        // Structured output format based on https://docs.ollama.com/capabilities/structured-outputs
        var payload = new OllamaGenerateRequest
        {
            Model = model,
            System = systemPrompt,
            Prompt = ExtractionPromptBuilder.Build(text, examples),
            Stream = false,
            Think = false,
            Format = new OllamaJsonSchema
            {
                Type = "object",
                Properties = categories.ToDictionary(
                    category => category,
                    _ => new OllamaJsonSchema { Type = "string" },
                    StringComparer.Ordinal),
                Required = categories,
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
            throw new InvalidDataException($"Ollama extraction failed: {envelope.Error}");
        }

        if (envelope is null || string.IsNullOrWhiteSpace(envelope.Response))
        {
            throw new InvalidDataException("Ollama returned no extraction response.");
        }

        using var extracted = JsonDocument.Parse(envelope.Response);
        if (extracted.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException("Ollama must return a JSON object of category values.");
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in extracted.RootElement.EnumerateObject())
        {
            if (!categories.Contains(property.Name, StringComparer.Ordinal) ||
                property.Value.ValueKind != JsonValueKind.String ||
                !values.TryAdd(property.Name, property.Value.GetString()!))
            {
                throw new InvalidDataException($"Ollama returned an invalid or duplicate category: {property.Name}");
            }
        }

        if (values.Count != categories.Length)
        {
            throw new InvalidDataException("Ollama omitted required categories. Try a larger model.");
        }

        return categories.Select(category => new ExtractedField(category, values[category])).ToArray();
    }
}