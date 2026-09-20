namespace DocumentProcessor.Classification.Constants;

public static class DocumentTypes
{
    public const string Invoice = "INVOICE";
    public const string Correspondence = "CORRESPONDENCE";
    public const string Other = "OTHER";

    public static readonly string[] Available = [Invoice, Correspondence, Other];
}
