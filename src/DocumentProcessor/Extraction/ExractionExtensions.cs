using DocumentProcessor.Extraction.Commands.PdfDataExtractionCommand;
using DocumentProcessor.Extraction.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentProcessor.Extraction;

public static class ExractionExtensions
{
    public static IServiceCollection AddExtraction(this IServiceCollection services)
    {
        services.AddCliCommand<PdfDataExtractionCommand>()
            .AddSingleton<PdfDataExtractionCommandProcessor>()
            .AddHttpClient<ExtractionService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(10);
            });
        return services;
    }
}