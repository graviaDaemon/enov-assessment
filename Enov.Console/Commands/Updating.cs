using CommandLine;

namespace Enov.Console.Commands;

[Verb("update", false)]
public class Updating
{
    [Option('i', "id", Required = true, HelpText = "The id of the item we wish to update")]
    public int Id { get; set; }
    
    [Option('n', "name", Required = true, HelpText = "The name of the item we wish to update")]
    public string Name { get; set; }
    
    [Option('t', "is-new", Required = false, Default = false, HelpText = "Whether or not the item we wish to update is a new item")]
    public bool IsNew { get; set; }
}