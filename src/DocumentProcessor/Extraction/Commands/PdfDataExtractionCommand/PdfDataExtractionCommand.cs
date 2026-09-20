using System.CommandLine;
using DocumentProcessor.Extraction.Constants;

namespace DocumentProcessor.Extraction.Commands.PdfDataExtractionCommand;

public class PdfDataExtractionCommand : Command
{
    public PdfDataExtractionCommand(PdfDataExtractionCommandProcessor processor) : base("extract-pdf-data", "Extracts document data as FieldName/Text JSON")
    {
        Aliases.Add("pdf-data-extraction");

        var pdfPathArgument = new Argument<string>("pdfPath")
        {
            Description = "Path to the PDF to extract data from"
        };
        var outputOption = new Option<string?>("--output", "-o")
        {
            Description = "Output JSON file (defaults to files/extraction/results/<type>/<pdf-filename>_<timestamp>.json)"
        };
        var typeOption = ExtractionTypes.CreateOption();

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