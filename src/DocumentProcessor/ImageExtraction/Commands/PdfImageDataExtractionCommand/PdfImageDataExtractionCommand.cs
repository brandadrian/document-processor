using System.CommandLine;
using DocumentProcessor.ImageExtraction.Constants;

namespace DocumentProcessor.ImageExtraction.Commands.PdfImageDataExtractionCommand;

public class PdfImageDataExtractionCommand : Command
{
    public PdfImageDataExtractionCommand(PdfImageDataExtractionCommandProcessor processor)
        : base("extract-pdf-image-data", "Extracts document data from a PDF using the image extraction pipeline")
    {
        Aliases.Add("pdf-image-data-extraction");

        var pdfPathArgument = new Argument<string>("pdfPath")
        {
            Description = "Path to the PDF to extract data from"
        };
        var outputOption = new Option<string?>("--output", "-o")
        {
            Description = "Output JSON file (defaults to files/image-extraction/results/<type>/<pdf-filename>_<timestamp>.json)"
        };
        var typeOption = ImageExtractionTypes.CreateOption();

        Add(pdfPathArgument);
        Add(outputOption);
        Add(typeOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            return await processor.Execute(
                parseResult.GetValue(pdfPathArgument)!,
                parseResult.GetValue(outputOption),
                parseResult.GetValue(typeOption)!,
                cancellationToken) ? 0 : 1;
        });
    }
}
