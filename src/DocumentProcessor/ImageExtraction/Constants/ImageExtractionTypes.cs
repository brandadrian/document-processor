using System.CommandLine;

namespace DocumentProcessor.ImageExtraction.Constants;

internal static class ImageExtractionTypes
{
    public const string InvoiceExample = "invoice_example";

    public static readonly string[] Available = [InvoiceExample];

    public static Option<string> CreateOption()
    {
        var option = new Option<string>("--type")
        {
            Description = $"Image extraction type. Available values: {string.Join(", ", Available)}",
            DefaultValueFactory = _ => InvoiceExample
        };
        option.AcceptOnlyFromAmong(Available);
        return option;
    }
}
