namespace DocumentProcessor.Classification.Options;

public sealed class ClassificationSettings
{
    public string LlmUrl { get; init; } = "http://localhost:11434/api/generate";

    public string Model { get; init; } = "qwen3:4b";
}
