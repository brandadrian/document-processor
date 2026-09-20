namespace DocumentProcessor.Shared.Models;

public sealed record ProcessingResult<T>(DateTimeOffset Date, T Data, string RawOcr);
