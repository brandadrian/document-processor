using System.Text.Json;
using DocumentProcessor.Extraction.Models;

namespace DocumentProcessor.Extraction.Services;

public static class ExtractionPromptBuilder
{
    private static readonly JsonSerializerOptions PromptJsonOptions = new() { WriteIndented = true };

    public static string Build(string text, IEnumerable<ExtractionExample> examples)
    {
        var extractionExamples = examples.ToArray();
        // Support extraction runs without embedded training examples.
        // In that mode we send only the raw PDF text as task input.
        if (extractionExamples.Length == 0)
        {
            return $"""
                Extract the data from this PDF text:
                {text}
                """;
        }

        var exampleJson = JsonSerializer.Serialize(
            extractionExamples.Select(example => new
            {
                text = example.Text,
                extraction = example.Fields.ToDictionary(field => field.FieldName, field => field.Text, StringComparer.Ordinal)
            }),
            PromptJsonOptions);

        return $"""
            Use these example PDF texts with their corresponding extraction results as guidance for category meanings and value formatting.
            Do not copy example values unless the same values are present in the PDF text to extract.

            Examples:
            {exampleJson}

            Extract the data from this PDF text:
            {text}
            """;
    }
}
