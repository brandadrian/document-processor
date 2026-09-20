using System.CommandLine;

namespace DocumentProcessor.Extraction.Constants;

internal static class ExtractionTypes
{
    public const string Default = "lehrvertrag";
    public const string Invoice = "invoice";
    public const string InvoiceExample = "invoice_example";

    public static readonly string[] Available = [Invoice, InvoiceExample, Default];

    public static Option<string> CreateOption()
    {
        var option = new Option<string>("--type")
        {
            Description = $"Extraction type. Available values: {string.Join(", ", Available)}",
            DefaultValueFactory = _ => Default
        };
        option.AcceptOnlyFromAmong(Available);
        return option;
    }
}
