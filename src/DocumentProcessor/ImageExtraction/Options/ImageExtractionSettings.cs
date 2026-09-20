namespace DocumentProcessor.ImageExtraction.Options;

public sealed class ImageExtractionSettings
{
    public required string LlmUrl { get; init; }
    
    public required string Model { get; init; }
}
