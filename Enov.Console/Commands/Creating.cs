using CommandLine;

namespace Enov.Console.Commands;

[Verb("create", false)]
public class Creating
{
    [Option('i', "id", Required = true, HelpText = "The id of the item we wish to create")]
    public int Id { get; set; }
    
    [Option('n', "name", Required = true, HelpText = "The name of the item we wish to create")]
    public string Name { get; set; }
    
    [Option('t', "is-new", Required = false, Default = true, HelpText = "Whether or not the item is a new item")]
    public bool IsNew { get; set; }
}