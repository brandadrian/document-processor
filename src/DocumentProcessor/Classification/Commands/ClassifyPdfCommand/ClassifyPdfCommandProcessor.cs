using System.Diagnostics;
using System.Text.Json;
using DocumentProcessor.Classification.Commands.ClassifyPdfCommand.Models;
using DocumentProcessor.Classification.Options;
using DocumentProcessor.Classification.Services;
using DocumentProcessor.Shared.Models;
using DocumentProcessor.Shared.Services;
using Microsoft.Extensions.Logging;

namespace DocumentProcessor.Classification.Commands.ClassifyPdfCommand;

public class ClassifyPdfCommandProcessor(
    ILogger<ClassifyPdfCommandProcessor> logger,
    ClassificationService classificationService,
    ClassificationSettings settings,
    TextReaderService textReaderService,
    OutputPathResolver outputPathResolver)
{
    private static readonly JsonSerializerOptions OutputOptions = new() { WriteIndented = true };

    public async Task<bool> Execute(string pdfPath, string? outputPath, CancellationToken cancellationToken)
    {
        bool success = false;

        try
        {
            var stopwatch = Stopwatch.StartNew();
            var text = textReaderService.ReadText(pdfPath, logger, cancellationToken);
            
            logger.LogInformation("Start classification. PdfPath: {PdfPath}", pdfPath);
            
            var result = await classificationService.ClassifyDocument(text, settings.Model, settings.LlmUrl, cancellationToken);
            stopwatch.Stop();
            var classificationResult = new ProcessingResult<DocumentClassification>(
                DateTimeOffset.Now,
                settings.Model,
                stopwatch.ElapsedMilliseconds,
                result,
                text);
            var resolvedOutputPath = outputPathResolver.Resolve(
                pdfPath,
                outputPath,
                "files",
                "classification",
                "results",
                result.DocumentType.ToLowerInvariant());
            if (Path.GetFullPath(pdfPath) == Path.GetFullPath(resolvedOutputPath))
            {
                throw new ArgumentException("The output path must not overwrite the input PDF.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(resolvedOutputPath)!);
            var json = JsonSerializer.Serialize(classificationResult, OutputOptions);
            await File.WriteAllTextAsync(resolvedOutputPath, json + Environment.NewLine, cancellationToken);

            logger.LogInformation(
                "Classification done. PdfPath: {PdfPath}. Type: {DocumentType}. Result: {OutputPath}. Processing time: {ProcessingTimeMs} ms",
                pdfPath,
                result.DocumentType,
                resolvedOutputPath,
                stopwatch.ElapsedMilliseconds);
            
            success = true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error reading PDF or processing request.");
        }

        return success;
    }

}