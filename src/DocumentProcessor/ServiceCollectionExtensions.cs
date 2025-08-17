using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentProcessor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCliCommand<T>(this IServiceCollection services)
        where T : CommandLineApplication
    {
        return services.AddSingleton<CommandLineApplication, T>();
    }
}