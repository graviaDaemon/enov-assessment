using System.Threading.Tasks;
using Enov.API.Entities;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

[assembly: FunctionsStartup(typeof(Enov.API.Startup))]

namespace Enov.API;

/// <summary>
/// The startup for the Azure Function which handles the connection to the database. Taking secrets from the
/// Environmental Variables where required.
/// NOTE: At localhost this seems to create issues, so for now we hardcoded.
/// IMPORTANT: Update hardcoded secrets to require Environmental Variables!
/// </summary>
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        IConfiguration configuration = builder.GetContext().Configuration;
        builder.Services.AddSingleton<IDatabaseService>((s) =>
            InitializeDatabaseInstanceAsync(configuration).GetAwaiter().GetResult());
    }

    private static async Task<DatabaseService> InitializeDatabaseInstanceAsync(IConfiguration configuration)
    {
        const string connectionString = "server=localhost;port=3306;database=items;user=root;";
        // IMPORTANT: Update hardcoded secrets to require Environmental Variables!
        // TODO: Use underlying line instead of the const above in production. 
        // configuration.GetSection("ConnectionString").Value;
        MySqlConnection connection = new(connectionString);
        DatabaseService dbService = new(connection);
        return dbService;
    }
}