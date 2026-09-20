using System.Text;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace DocumentProcessor.Shared.Services;

public class TextReaderService
{
    public string ReadText(string pdfPath, ILogger logger, CancellationToken cancellationToken)
    {
        logger.LogInformation("Start text extraction. PdfPath: {PdfPath}", pdfPath);

        using var document = PdfDocument.Open(pdfPath);
        var text = ReadText(document, cancellationToken);

        logger.LogInformation("Text extraction done. PdfPath: {PdfPath}", pdfPath);
        return text;
    }

    public static string ReadText(Stream pdfStream, CancellationToken cancellationToken)
    {
        using var document = PdfDocument.Open(pdfStream);
        return ReadText(document, cancellationToken);
    }

    private static string ReadText(PdfDocument document, CancellationToken cancellationToken)
    {
        var text = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();
            text.AppendLine(ContentOrderTextExtractor.GetText(page));
        }

        if (text.Length == 0)
        {
            throw new InvalidDataException("The PDF contains no readable text.");
        }

        return text.ToString();
    }
}