using System.Text.Json;

namespace DocumentProcessor.ImageExtraction.Services;

public static class ImageExtractionPromptBuilder
{
    private static readonly JsonSerializerOptions PromptJsonOptions = new() { WriteIndented = true };

    public static string Build(IReadOnlyCollection<string> fieldsToExtract, int inputPageCount)
    {
        var inputImages = Enumerable.Range(1, inputPageCount)
            .Select(imageNumber => $"image_{imageNumber}")
            .ToArray();

        return $"""
            You receive PDF page images.
            The images belong to the input document only.

            Input document images:
            {JsonSerializer.Serialize(inputImages, PromptJsonOptions)}

            Required fields:
            {JsonSerializer.Serialize(fieldsToExtract, PromptJsonOptions)}

            Extract the data from the complete input document images only.
            For invoiceDate, convert unambiguous German dates like "17. August 2025" to "2025-08-17".
            For totalAmount, copy the final payable amount exactly as it appears in the image. Keep European thousand and decimal separators, for example "1.428,00".
            If a value is not clearly readable in the image, return an empty string for that field.
            Do not guess, infer, or use common invoice placeholder values.
            """;
    }
}
