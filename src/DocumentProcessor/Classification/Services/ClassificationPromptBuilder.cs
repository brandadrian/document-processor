namespace DocumentProcessor.Classification.Services;

public class ClassificationPromptBuilder
{
    public static async Task<string> BuildSystemPrompt(CancellationToken cancellationToken)
    {
        return await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "prompts", "classification_prompt.txt"), cancellationToken);
    }

    public static string BuildPrompt(string text)
    {
        return $"""
            Classify this document text:
            {text}
            """;
    }
}