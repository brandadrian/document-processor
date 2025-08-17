using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DocumentProcessor.ClassifyPdfCommand.Helpers;
using DocumentProcessor.ClassifyPdfCommand.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace DocumentProcessor.ClassifyPdfCommand;

internal class ClassifyPdfCommandProcessor
{
    private readonly string _pdfPath;
    private readonly string _llmUrl;

    public ClassifyPdfCommandProcessor(string pdfPath)
    {
        _pdfPath = pdfPath;
        _llmUrl = "http://localhost:11434/api/generate";
    }

    public async Task<bool> Execute(CancellationToken cancellationToken)
    {
        bool success = false;

        try
        {
            var text = ReadTextFromPdf();
            
            Console.WriteLine($"Start classification. PdfPath: {_pdfPath}");
            
            var result = await ClassifyDocument(cancellationToken, text);

            Console.WriteLine($"Classification done. PdfPath: {_pdfPath}. Type: {result.DocumentType}");
            
            success = true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error reading PDF or processing request: " + e.Message);
        }

        return success;
    }

    private async Task<DocumentClassification> ClassifyDocument(CancellationToken cancellationToken, string text)
    {
        var responseMessage = string.Empty;
        
        try
        {
            var requestPayload = new LlmRequest
            {
                Model = "mistral",
                Prompt = LlmPromts.GetDefaultPromt(text),
                Stream = false
            };
                
            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(_llmUrl, httpContent, cancellationToken);
            responseMessage = response.StatusCode.ToString();
                
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var ollamaResponse = JsonSerializer.Deserialize<LlmResponse>(responseContent);
                var result = JsonSerializer.Deserialize<DocumentClassification>(ollamaResponse.Response);
                result.Successful = true;
                result.Message = "success";
            
                return result;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error during classification: " + e.Message);
            responseMessage = e.Message;
        }
        
        return new DocumentClassification() 
        {
            Confidence = 0.0,
            DocumentType = "OTHER",
            Reasoning = "Failed to classify document!",
            Successful = false,
            Message = responseMessage
        };
    }

    private string ReadTextFromPdf()
    {
        Console.WriteLine($"Start text extraction. PdfPath: {_pdfPath}");
        
        var text = string.Empty;
        
        using (PdfDocument document = PdfDocument.Open(_pdfPath))
        {
            foreach (Page page in document.GetPages())
            {
                var currentText = ContentOrderTextExtractor.GetText(page);
                text = text + "\n" + currentText; 
            }
        }

        Console.WriteLine($"Text extraction done. PdfPath: {_pdfPath}");
        
        return text;
    }
}