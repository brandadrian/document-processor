using System.Diagnostics;
using DocumentProcessor.Classification.Options;
using DocumentProcessor.Classification.Services;
using DocumentProcessor.Shared;
using DocumentProcessor.Shared.Services;
using Microsoft.Extensions.Logging;

namespace DocumentProcessor.Classification.Commands.ClassifyPdfCommand;

public class ClassifyPdfCommandProcessor(
    ILogger<ClassifyPdfCommandProcessor> logger,
    ClassificationService classificationService,
    ClassificationSettings settings,
    TextReaderService textReaderService)
{
    public async Task<bool> Execute(string pdfPath, CancellationToken cancellationToken)
    {
        bool success = false;

        try
        {
            var stopwatch = Stopwatch.StartNew();
            var text = textReaderService.ReadText(pdfPath, logger, cancellationToken);
            
            logger.LogInformation("Start classification. PdfPath: {PdfPath}", pdfPath);
            
            var result = await classificationService.ClassifyDocument(text, settings.Model, settings.LlmUrl, cancellationToken);
            stopwatch.Stop();

            logger.LogInformation(
                "Classification done. PdfPath: {PdfPath}. Type: {DocumentType}. Processing time: {ProcessingTimeMs} ms",
                pdfPath,
                result.DocumentType,
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