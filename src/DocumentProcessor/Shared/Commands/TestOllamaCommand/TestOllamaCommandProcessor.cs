using System.Net.Http.Json;
using DocumentProcessor.Classification.Options;
using DocumentProcessor.Shared.Models.Ollama;
using Microsoft.Extensions.Logging;

namespace DocumentProcessor.Shared.Commands.TestOllamaCommand;

public class TestOllamaCommandProcessor(
    HttpClient httpClient,
    ClassificationSettings settings,
    ILogger<TestOllamaCommandProcessor> logger)
{
    public async Task<bool> Execute(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            var requestPayload = new OllamaGenerateRequest
            {
                Model = settings.Model,
                Prompt = prompt,
                Stream = false,
                Think = false
            };

            using var response = await httpClient.PostAsJsonAsync(settings.LlmUrl, requestPayload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidDataException($"Ollama test failed with HTTP status {response.StatusCode}.");
            }

            var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken);
            if (!string.IsNullOrWhiteSpace(ollamaResponse?.Error))
            {
                throw new InvalidDataException($"Ollama test failed: {ollamaResponse.Error}");
            }

            if (ollamaResponse is null || string.IsNullOrWhiteSpace(ollamaResponse.Response))
            {
                throw new InvalidDataException("Ollama returned no response.");
            }

            logger.LogInformation("Ollama test succeeded. Model: {Model}. Response: {Response}", settings.Model, ollamaResponse.Response.Trim());
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Ollama test cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Ollama test failed.");
        }

        return false;
    }
}
