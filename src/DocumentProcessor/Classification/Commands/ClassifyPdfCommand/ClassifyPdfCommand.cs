using System.CommandLine;

namespace DocumentProcessor.Classification.Commands.ClassifyPdfCommand;

public class ClassifyPdfCommand : Command
{
    public ClassifyPdfCommand(ClassifyPdfCommandProcessor processor) : base("classify-pdf", "Classifies a pdf document whether it is an invoice, correspondence or others")
    {
        var pdfPathArgument = new Argument<string>("pdfPath")
        {
            Description = "Path to pdf to classify"
        };
        var outputOption = new Option<string?>("--output", "-o")
        {
            Description = "Output JSON file (defaults to files/classification/results/<document-type>/<pdf-filename>_<timestamp>.json)"
        };

        Add(pdfPathArgument);
        Add(outputOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var pdfPath = parseResult.GetValue(pdfPathArgument);
            var success = await processor.Execute(
                pdfPath!,
                parseResult.GetValue(outputOption),
                cancellationToken);
            return success ? 0 : 1;
        });
    }
}