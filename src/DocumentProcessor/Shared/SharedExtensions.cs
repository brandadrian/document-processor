using DocumentProcessor.Shared.Commands.TestOllamaCommand;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentProcessor.Shared;

public static class SharedExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services)
    {
        services.AddCliCommand<TestOllamaCommand>()
            .AddSingleton<TestOllamaCommandProcessor>()
            .AddHttpClient<TestOllamaCommandProcessor>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(1);
            });

        return services;
    }
}
