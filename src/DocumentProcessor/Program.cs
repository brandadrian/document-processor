using DocumentProcessor;
using DocumentProcessor.ClassifyPdfCommand;
using Microsoft.Extensions.DependencyInjection;
 
var services = new ServiceCollection()
    .AddCliCommand<ClassifyPdfCommand>()
    .AddSingleton<Cli>();

var provider = services.BuildServiceProvider();
var cli = provider.GetRequiredService<Cli>();
cli.Name = "DocumentProcessor";
cli.Description = "DocumentProcessor is a command line tool for document processing.";

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("Cancelling...");
    cts.Cancel();
    e.Cancel = true;
};

return await cli.ExecuteAsync(args, cts.Token);