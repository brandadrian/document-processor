namespace DocumentProcessor.Shared.Models;

public sealed record ProcessingResult<T>(
    DateTimeOffset Date,
    string Model,
    long ProcessingTimeMs,
    T Data,
    string RawOcr);
