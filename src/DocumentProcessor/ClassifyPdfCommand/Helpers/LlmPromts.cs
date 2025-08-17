namespace DocumentProcessor.ClassifyPdfCommand.Helpers;

public static class LlmPromts
{
    public static string GetDefaultPromt(string text)
    {
        var prompt = $@"You are a document classifier. Analyze the following document text and classify it as exactly one of these types: INVOICE, CORRESPONDENCE, or OTHER.
                        You MUST respond with ONLY a valid JSON object in this EXACT format (no additional text before or after):
                        {{
                            ""document_type"": ""INVOICE"",
                            ""confidence"": 0.9,
                            ""reasoning"": ""The document contains pricing, totals, and payment details typical of an invoice""
                        }}

                        The document_type must be exactly one of: INVOICE, CORRESPONDENCE, or OTHER
                        The confidence must be a number between 0.0 and 1.0
                        The reasoning should be a brief explanation

                        Document text to analyze:
                        {text}

                        JSON response:";
        
        return prompt;
    }
}