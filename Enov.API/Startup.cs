using System.Threading.Tasks;
using Enov.API.Entities;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

[assembly: FunctionsStartup(typeof(Enov.API.Startup))]

namespace Enov.API;

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
        string connectionString = "server=localhost;port=3306;database=items;user=root;";
            // configuration.GetSection("ConnectionString").Value;
        MySqlConnection connection = new(connectionString);
        DatabaseService dbService = new(connection);
        return dbService;
    }
}