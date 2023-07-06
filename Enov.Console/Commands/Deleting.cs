using CommandLine;

namespace Enov.Console.Commands;

[Verb("delete", false)]
public class Deleting
{
    [Option('i', "ids", Required = true, HelpText = "The ids of the items we wish to delete, separated by a comma")]
    public string Ids { get; set; }
}