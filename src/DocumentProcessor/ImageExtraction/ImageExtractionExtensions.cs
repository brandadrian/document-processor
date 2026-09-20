using DocumentProcessor.ImageExtraction.Commands.PdfImageDataExtractionCommand;
using DocumentProcessor.ImageExtraction.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentProcessor.ImageExtraction;

public static class ImageExtractionExtensions
{
    public static IServiceCollection AddImageExtraction(this IServiceCollection services)
    {
        services.AddCliCommand<PdfImageDataExtractionCommand>()
            .AddSingleton<PdfImageDataExtractionCommandProcessor>()
            .AddTransient<PdfImageRendererService>()
            .AddHttpClient<ImageExtractionService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(10);
            });

        return services;
    }
}
