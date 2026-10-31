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
        var useTrainingFilesOption = new Option<bool>("--use-training-files")
        {
            Description = "Whether embedded training files should be used as extraction examples",
            DefaultValueFactory = _ => false
        };

        Add(pdfPathArgument);
        Add(outputOption);
        Add(typeOption);
        Add(useTrainingFilesOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            return await processor.Execute(
                parseResult.GetValue(pdfPathArgument)!,
                parseResult.GetValue(outputOption),
                parseResult.GetValue(typeOption)!,
                parseResult.GetValue(useTrainingFilesOption),
                cancellationToken) ? 0 : 1;
        });
    }
}