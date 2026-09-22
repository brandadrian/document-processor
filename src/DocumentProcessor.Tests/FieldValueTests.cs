using DocumentProcessor.Extraction.Models;
using Xunit;

namespace DocumentProcessor.Tests;

public class FieldValueTests
{
    [Fact]
    public void Constructor_PreservesFieldNameAndText()
    {
        var fieldValue = new FieldValue("InvoiceNumber", "INV-001");

        Assert.Equal("InvoiceNumber", fieldValue.FieldName);
        Assert.Equal("INV-001", fieldValue.Text);
    }
}