using System.Diagnostics;
using System.Text.Json;
using DocumentProcessor.Extraction.Models;
using DocumentProcessor.Extraction.Options;
using DocumentProcessor.Extraction.Services;
using DocumentProcessor.Shared.Models;
using DocumentProcessor.Shared.Services;
using Microsoft.Extensions.Logging;

namespace DocumentProcessor.Extraction.Commands.PdfDataExtractionCommand;

public class PdfDataExtractionCommandProcessor(
    ExtractionService extractionService,
    ILogger<PdfDataExtractionCommandProcessor> logger,
    ExtractionSettings settings,
    TextReaderService textReaderService,
    OutputPathResolver outputPathResolver)
{
    private static readonly JsonSerializerOptions OutputOptions = new() { WriteIndented = true };
    
    public async Task<bool> Execute(
        string pdfPath,
        string? outputPath,
        string type,
        bool useTrainingFiles,
        CancellationToken cancellationToken)
    {
        try
        {
            if (outputPath is not null && Path.GetFullPath(pdfPath) == Path.GetFullPath(outputPath))
            {
                throw new ArgumentException("The output path must not overwrite the input PDF.");
            }

            var stopwatch = Stopwatch.StartNew();
            var examples = useTrainingFiles ? LoadExamples(type, cancellationToken) : [];
            var categories = ResolveCategories(type, examples);
            var text = textReaderService.ReadText(pdfPath, logger, cancellationToken);
            var fields = await extractionService.ExtractData(text, examples, categories, type, settings.Model, settings.LlmUrl, cancellationToken);
            stopwatch.Stop();
            var result = new ProcessingResult<ExtractedField[]>(
                DateTimeOffset.Now,
                settings.Model,
                stopwatch.ElapsedMilliseconds,
                fields,
                text);
            var json = JsonSerializer.Serialize(result, OutputOptions);
            var resolvedOutputPath = outputPathResolver.Resolve(
                pdfPath,
                outputPath,
                "files",
                "extraction",
                "results",
                type);
            if (Path.GetFullPath(pdfPath) == Path.GetFullPath(resolvedOutputPath))
            {
                throw new ArgumentException("The output path must not overwrite the input PDF.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(resolvedOutputPath)!);
            await File.WriteAllTextAsync(resolvedOutputPath, json + Environment.NewLine, cancellationToken);
            logger.LogInformation(
                "Extracted {FieldCount} fields to {OutputPath} in {ProcessingTimeMs} ms",
                fields.Length,
                resolvedOutputPath,
                stopwatch.ElapsedMilliseconds);

            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("PDF data extraction cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "PDF data extraction failed.");
        }

        return false;
    }

    private static string[] ResolveCategories(string type, IReadOnlyCollection<ExtractionExample> examples)
    {
        if (examples.Count > 0)
        {
            return examples
                .SelectMany(example => example.Fields)
                .Select(field => field.FieldName)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }

        var categoriesFromEmbeddedSchema = LoadCategoriesFromEmbeddedExpectedFiles(type);
        if (categoriesFromEmbeddedSchema.Length > 0)
        {
            return categoriesFromEmbeddedSchema;
        }

        throw new InvalidDataException(
            $"No categories available for type '{type}'. Add embedded *_expected.json files for this type.");
    }

    private static string[] LoadCategoriesFromEmbeddedExpectedFiles(string type)
    {
        var assembly = typeof(PdfDataExtractionCommandProcessor).Assembly;
        var resourcePrefix = $"ExtractionSamples/{type}/";
        var categorySet = new HashSet<string>(StringComparer.Ordinal);

        foreach (var name in assembly.GetManifestResourceNames()
                     .Where(name => NormalizeResourcePath(name).StartsWith(resourcePrefix, StringComparison.Ordinal) &&
                                    NormalizeResourcePath(name).EndsWith("_expected.json", StringComparison.Ordinal))
                     .OrderBy(name => name, StringComparer.Ordinal))
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            var fields = JsonSerializer.Deserialize<ExtractedField[]>(stream);
            if (fields is null || fields.Length == 0 ||
                fields.Any(field => field is null || string.IsNullOrWhiteSpace(field.FieldName) || field.Text is null))
            {
                throw new InvalidDataException($"Invalid extraction example: {name}");
            }

            foreach (var fieldName in fields.Select(field => field.FieldName))
            {
                categorySet.Add(fieldName);
            }
        }

        return categorySet.ToArray();
    }

    private static List<ExtractionExample> LoadExamples(string type, CancellationToken cancellationToken)
    {
        var assembly = typeof(PdfDataExtractionCommandProcessor).Assembly;
        var examples = new List<ExtractionExample>();
        var resourcePrefix = $"ExtractionSamples/{type}/";

        foreach (var name in assembly.GetManifestResourceNames()
                     .Where(name => NormalizeResourcePath(name).StartsWith(resourcePrefix, StringComparison.Ordinal) &&
                                    NormalizeResourcePath(name).EndsWith("_expected.json", StringComparison.Ordinal))
                     .OrderBy(name => name, StringComparer.Ordinal))
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            var fields = JsonSerializer.Deserialize<ExtractedField[]>(stream);
            if (fields is null || fields.Length == 0 ||
                fields.Any(field => field is null || string.IsNullOrWhiteSpace(field.FieldName) || field.Text is null) ||
                fields.Select(field => field.FieldName).Distinct(StringComparer.Ordinal).Count() != fields.Length)
            {
                throw new InvalidDataException($"Invalid extraction example: {name}");
            }

            var pdfResourceName = name[..^"_expected.json".Length] + ".pdf";
            using var pdfStream = assembly.GetManifestResourceStream(pdfResourceName) ??
                                  throw new InvalidDataException($"Missing extraction example PDF: {pdfResourceName}");
            var text = TextReaderService.ReadText(pdfStream, cancellationToken);
            examples.Add(new ExtractionExample(text, fields));
        }

        if (examples.Count == 0)
        {
            throw new InvalidDataException($"No extraction examples are embedded for type '{type}'.");
        }

        return examples;
    }

    private static string NormalizeResourcePath(string resourceName)
    {
        return resourceName.Replace('\\', '/');
    }
}