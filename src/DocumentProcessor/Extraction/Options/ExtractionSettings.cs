namespace DocumentProcessor.Extraction.Options;

public sealed class ExtractionSettings
{
    public string LlmUrl { get; init; } = "http://localhost:11434/api/generate";

    public string Model { get; init; } = "qwen3:4b";
}
