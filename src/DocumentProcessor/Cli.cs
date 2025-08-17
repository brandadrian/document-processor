using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;

namespace DocumentProcessor;

public class Cli : CommandLineApplication
{
    public Cli(IEnumerable<CommandLineApplication> commands)
    {
        HelpOption("-? | -h | --help", true);

        foreach (var cmd in commands)
        {
            AddSubcommand(cmd);
        }
    }
}