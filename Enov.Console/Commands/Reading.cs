using CommandLine;

namespace Enov.Console.Commands;

[Verb("read", false)]
public class Reading
{
    [Option('i', "ids", Required = true, HelpText = "The ids of the items we wish to retrieve and read, separated by a comma")]
    public string Ids { get; set; }
}