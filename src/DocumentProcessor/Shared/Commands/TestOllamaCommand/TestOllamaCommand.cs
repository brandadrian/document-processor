using System.CommandLine;

namespace DocumentProcessor.Shared.Commands.TestOllamaCommand;

public class TestOllamaCommand : Command
{
    public TestOllamaCommand(TestOllamaCommandProcessor processor) : base("test-ollama", "Sends a basic request to Ollama to verify connectivity")
    {
        var promptOption = new Option<string>("--prompt")
        {
            Description = "Prompt to send to Ollama",
            DefaultValueFactory = _ => "Reply with the single word OK."
        };

        Add(promptOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var success = await processor.Execute(
                parseResult.GetValue(promptOption)!,
                cancellationToken);
            return success ? 0 : 1;
        });
    }
}
