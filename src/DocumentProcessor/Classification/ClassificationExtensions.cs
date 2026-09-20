using DocumentProcessor.Classification.Commands.ClassifyPdfCommand;
using DocumentProcessor.Classification.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentProcessor.Classification;

public static class ClassificationExtensions
{
    public static IServiceCollection AddClassification(this IServiceCollection services)
    {
        services.AddCliCommand<ClassifyPdfCommand>()
            .AddSingleton<ClassifyPdfCommandProcessor>()
            .AddHttpClient<ClassificationService>(
                client =>
                {
                    client.Timeout = TimeSpan.FromMinutes(10);
                }
            );
        return services;
    }
}