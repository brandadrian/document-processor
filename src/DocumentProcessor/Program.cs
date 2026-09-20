using DocumentProcessor;
using DocumentProcessor.Classification;
using DocumentProcessor.Classification.Options;
using DocumentProcessor.Extraction;
using DocumentProcessor.Extraction.Options;
using DocumentProcessor.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var services = new ServiceCollection()
    .AddSingleton(new ClassificationSettings
    {
        LlmUrl = configuration["Classification:LlmUrl"] ??
                 throw new InvalidDataException("Missing configuration value: Classification:LlmUrl"),
        Model = configuration["Classification:Model"] ??
                throw new InvalidDataException("Missing configuration value: Classification:Model")
    })
    .AddSingleton(new ExtractionSettings
    {
        LlmUrl = configuration["Extraction:LlmUrl"] ??
                 throw new InvalidDataException("Missing configuration value: Extraction:LlmUrl"),
        Model = configuration["Extraction:Model"] ??
                throw new InvalidDataException("Missing configuration value: Extraction:Model")
    })
    .AddLogging(builder =>
    {
        builder.AddConsole(options =>
        {
            options.FormatterName = ConsoleFormatterNames.Simple;
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });
        builder.Services.Configure<SimpleConsoleFormatterOptions>(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
        });
    })
    .AddClassification()
    .AddExtraction()
    .AddTransient<TextReaderService>()
    .AddSingleton<Cli>();

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DocumentProcessor");
var cli = provider.GetRequiredService<Cli>();
cli.Description = "DocumentProcessor is a command line tool for document processing.";

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    logger.LogInformation("Cancelling...");
    cts.Cancel();
    e.Cancel = true;
};

return await cli.ExecuteAsync(args, cts.Token);