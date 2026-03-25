using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentProcessor.ClassifyPdfCommand;

public class ClassifyPdfCommand : Command
{
    public ClassifyPdfCommand() : base("classify-pdf", "Classifies a pdf document whether it is an invoice, correspondence or others")
    {
        var pdfPathArgument = new Argument<string>("pdfPath")
        {
            Description = "Path to pdf to classify"
        };

        Add(pdfPathArgument);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var pdfPath = parseResult.GetValue(pdfPathArgument);
            var processor = new ClassifyPdfCommandProcessor(pdfPath!);
            var success = await processor.Execute(cancellationToken);
            return success ? 0 : 1;
        });
    }
}