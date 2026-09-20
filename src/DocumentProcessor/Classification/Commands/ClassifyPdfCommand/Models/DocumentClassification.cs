namespace DocumentProcessor.Classification.Commands.ClassifyPdfCommand.Models;

public sealed record DocumentClassification(string DocumentType, double Confidence, string Reasoning);
