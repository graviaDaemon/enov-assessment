using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using Enov.API.Entities;
using Enov.Console.Commands;
using MySqlConnector;
using Newtonsoft.Json;

namespace Enov.Console;

public static class Program
{
    private static MySqlConnection _connection;

    private static bool _viewing = true;
    
    public static async Task Main(string[] args)
    {
        bool security = false;
        while (!security)
        {
            System.Console.WriteLine("Please insert the database connectionstring");
            string? connectionString = System.Console.ReadLine();

            try
            {
                _connection = new MySqlConnection(connectionString);
                await _connection.OpenAsync();
                System.Console.WriteLine("Success! Welcome to the interface.");
                security = true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Invalid connection string: {ex.Message}");
            }
        }
        
        while (_viewing)
        {
            string? input = System.Console.ReadLine();
            string pattern = @"([\""].+?[\""])|([^ ]+)";

            // Tokenize the input based on the pattern
            MatchCollection matches = Regex.Matches(input, pattern);

            // Extract the matched values into an array of strings
            string[] arguments = matches.Cast<Match>().Select(match => match.Value.Trim('\"')).ToArray();
            
            await Parser.Default.ParseArguments<Creating, Reading, Updating, Deleting, Exiting>(arguments)
                .MapResult(
                    async (Creating options) => CreateAsync(options),
                    async (Reading options) => ReadAsync(options),
                    async (Updating options) => UpdateAsync(options),
                    async (Deleting options) => DeleteAsync(options),
                    async (Exiting options) => HandleExitAsync(options),
                    async errs => HandleErrorsAsync(errs)
                );
        }
    }

    /// <summary>
    /// Allows for the creation of a single item. The Azure function allows for multiple,
    /// so future updates might change this to allow for multiple as well
    /// </summary>
    /// <param name="options"></param>
    private static async Task CreateAsync(Creating options)
    {
        // We build the options into the Item object, which we can then parse into a JSON string
        Item item = new Item()
        {
            Id = options.Id,
            Name = options.Name,
            IsNew = options.IsNew
        };

        string jsonItem = JsonConvert.SerializeObject(item);
        string requestObject = "{ \"items\": [ " + jsonItem + " ] }";
        using HttpClient client = new();
        HttpResponseMessage response = await client.PostAsync("http://localhost:7071/api/Create", new StringContent(requestObject));
        if (!response.IsSuccessStatusCode)
        {
            System.Console.WriteLine("There was an issue with creating that item.");
            return;
        }

        string result = await response.Content.ReadAsStringAsync();
        System.Console.WriteLine("Creating result: \n" + result);
    }

    private static async Task ReadAsync(Reading options)
    {
        // first we make sure that all spaces are removed from the string
        // for example 1, 2, 3, 4 would break, so we make it 1,2,3,4 instead
        string repaired = string.Join(",", options.Ids.Split(",").Select(item => item.Trim()).ToArray());
        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync("http://localhost:7071/api/Read?ids=" + repaired);
        if (!response.IsSuccessStatusCode)
        {
            System.Console.WriteLine("There was an issue reading the items");
            return;
        }
        string result = await response.Content.ReadAsStringAsync();
        System.Console.WriteLine($"Reading result: \n{result}");
    }

    /// <summary>
    ///  Just like with create, we acn ony update 1 item at a time
    /// </summary>
    /// <param name="options"></param>
    private static async Task UpdateAsync(Updating options)
    {
        // We build the options into the Item object, which we can then parse into a JSON string
        Item item = new Item()
        {
            Id = options.Id,
            Name = options.Name,
            IsNew = options.IsNew
        };

        string jsonItem = JsonConvert.SerializeObject(item);
        string requestObject = "{ \"items\": [ " + jsonItem + " ] }";
        using HttpClient client = new();
        HttpResponseMessage response = await client.PutAsync("http://localhost:7071/api/Update", new StringContent(requestObject));
        if (!response.IsSuccessStatusCode)
        {
            System.Console.WriteLine("There was an issue with creating that item.");
            return;
        }

        string result = await response.Content.ReadAsStringAsync();
        System.Console.WriteLine("Creating result: \n" + result);
    }

    private static async Task DeleteAsync(Deleting options)
    {
        // first we make sure that all spaces are removed from the string
        // for example 1, 2, 3, 4 would break, so we make it 1,2,3,4 instead
        string repaired = string.Join(",", options.Ids.Split(",").Select(item => item.Trim()).ToArray());
        using HttpClient client = new();
        HttpResponseMessage response = await client.DeleteAsync("http://localhost:7071/api/Delete?ids=" + repaired);
        if (!response.IsSuccessStatusCode)
        {
            System.Console.WriteLine("There was an issue deleting the items");
            return;
        }
        string result = await response.Content.ReadAsStringAsync();
        System.Console.WriteLine($"Reading result: \n{result}");
    }

    private static async Task HandleExitAsync(Exiting options)
    {
        System.Console.WriteLine("Exiting program...");
        await _connection.CloseAsync(); 
        _viewing = false;
    }

    private static async Task HandleErrorsAsync(IEnumerable<Error> errs)
    {
        foreach (Error err in errs)
        {
            System.Console.WriteLine(err);
        }
    }
}