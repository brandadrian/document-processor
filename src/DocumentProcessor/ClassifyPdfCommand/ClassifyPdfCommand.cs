using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace DocumentProcessor.ClassifyPdfCommand;

public class ClassifyPdfCommand : CommandLineApplication
{
    private readonly CommandArgument<string> _pdfPath;

    public ClassifyPdfCommand()
    {
        Name = "classify-pdf";
        Description = "Classifies a pdf document wether it is an invoice, correspondence or others";

        _pdfPath = Argument<string>(
            "pdfPath",
            "Path to pdf to classify",
            cfg => cfg.IsRequired(),
            true
        );

        OnExecuteAsync(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        ClassifyPdfCommandProcessor processor = new(_pdfPath.Value!);
        return await processor.Execute(cancellationToken)
            ? 0
            : 1;
    }
}