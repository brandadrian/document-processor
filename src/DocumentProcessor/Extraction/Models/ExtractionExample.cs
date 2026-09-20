namespace DocumentProcessor.Extraction.Models;

public sealed record ExtractionExample(string Text, ExtractedField[] Fields);
