using System.Diagnostics;
using System.Text.Json;
using DocumentProcessor.Extraction.Models;
using DocumentProcessor.ImageExtraction.Options;
using DocumentProcessor.ImageExtraction.Services;
using DocumentProcessor.Shared.Models;
using DocumentProcessor.Shared.Services;
using Microsoft.Extensions.Logging;

namespace DocumentProcessor.ImageExtraction.Commands.PdfImageDataExtractionCommand;

public class PdfImageDataExtractionCommandProcessor(
    ImageExtractionService imageExtractionService,
    ILogger<PdfImageDataExtractionCommandProcessor> logger,
    ImageExtractionSettings settings,
    PdfImageRendererService pdfImageRendererService,
    OutputPathResolver outputPathResolver)
{
    private static readonly JsonSerializerOptions OutputOptions = new() { WriteIndented = true };
    
    public async Task<bool> Execute(string pdfPath, string? outputPath, string type, CancellationToken cancellationToken)
    {
        try
        {
            if (outputPath is not null && Path.GetFullPath(pdfPath) == Path.GetFullPath(outputPath))
            {
                throw new ArgumentException("The output path must not overwrite the input PDF.");
            }

            var stopwatch = Stopwatch.StartNew();
            var exampleFields = LoadExampleFields(type);
            var fieldsToExtract = exampleFields
                .Select(field => field.FieldName).Distinct(StringComparer.Ordinal).ToArray();
            var imagesBase64 = await pdfImageRendererService.RenderPagesToBase64Png(pdfPath, cancellationToken);
            var fields = await imageExtractionService.ExtractData(
                imagesBase64,
                fieldsToExtract,
                type,
                settings.Model,
                settings.LlmUrl,
                cancellationToken);
            stopwatch.Stop();
            var result = new ProcessingResult<ExtractedField[]>(
                DateTimeOffset.Now,
                settings.Model,
                stopwatch.ElapsedMilliseconds,
                fields,
                string.Empty);
            var json = JsonSerializer.Serialize(result, OutputOptions);
            var resolvedOutputPath = outputPathResolver.Resolve(
                pdfPath,
                outputPath,
                "files",
                "image-extraction",
                "results",
                type);
            if (Path.GetFullPath(pdfPath) == Path.GetFullPath(resolvedOutputPath))
            {
                throw new ArgumentException("The output path must not overwrite the input PDF.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(resolvedOutputPath)!);
            await File.WriteAllTextAsync(resolvedOutputPath, json + Environment.NewLine, cancellationToken);
            logger.LogInformation(
                "Image extraction wrote {FieldCount} fields to {OutputPath} in {ProcessingTimeMs} ms",
                fields.Length,
                resolvedOutputPath,
                stopwatch.ElapsedMilliseconds);

            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("PDF image extraction cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "PDF image extraction failed.");
        }

        return false;
    }

    private static List<ExtractedField> LoadExampleFields(string type)
    {
        var assembly = typeof(PdfImageDataExtractionCommandProcessor).Assembly;
        var fields = new List<ExtractedField>();

        foreach (var name in assembly.GetManifestResourceNames()
                     .Where(name => name.StartsWith($"ImageExtractionSamples/{type}/", StringComparison.Ordinal) &&
                                    name.EndsWith("_expected.json", StringComparison.Ordinal))
                     .OrderBy(name => name, StringComparer.Ordinal))
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            var exampleFields = JsonSerializer.Deserialize<ExtractedField[]>(stream);
            if (exampleFields is null || exampleFields.Length == 0 ||
                exampleFields.Any(field => field is null || string.IsNullOrWhiteSpace(field.FieldName) || field.Text is null) ||
                exampleFields.Select(field => field.FieldName).Distinct(StringComparer.Ordinal).Count() != exampleFields.Length)
            {
                throw new InvalidDataException($"Invalid image extraction example: {name}");
            }

            fields.AddRange(exampleFields);
        }

        if (fields.Count == 0)
        {
            throw new InvalidDataException($"No image extraction fields are embedded for type '{type}'.");
        }

        return fields;
    }
}
