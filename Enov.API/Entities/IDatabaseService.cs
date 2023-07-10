using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enov.API.Entities;

/// <summary>
/// The service which connects to the database and handles all the CRUD interactions.
/// TODO: Add a "tablename" parameter to the connection instead of hardcoding it.
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// The Create method (POST) for the database.
    /// This method can handle multiple entries at the same time.
    /// </summary>
    /// <param name="items">The items to create into the table. This is an Array of 1 or more items</param>
    /// <returns>An array of Items that were created in the database</returns>
    Task<IEnumerable<Item>> CreateAsync(IEnumerable<Item> items);
    
    /// <summary>
    /// The Get method (GET) for the database.
    /// This method can handle multiple IDs to search and fetch.
    /// </summary>
    /// <param name="ids">An array of 1 ore more ID's to fetch from the database.</param>
    /// <returns>An array of items that were fetched from the table.</returns>
    Task<IEnumerable<Item>> GetAsync(string[] ids);
    
    /// <summary>
    /// The Update method (PUT) for the database.
    /// This method can handle multiple entries at the same time.
    /// </summary>
    /// <param name="items">An array of items to update. Where the ID's of these items exists.</param>
    /// <returns>An array of items that were updated in the database</returns>
    Task<IEnumerable<Item>> UpdateAsync(IEnumerable<Item> items);
    
    /// <summary>
    /// The Delete method (DELETE) for the database.
    /// This method can handle multiple entries at the same time
    /// </summary>
    /// <param name="ids">An array of 1 or more ID's to delete from the database</param>
    /// <returns>True if all items were deleted. False if any were not. Check the metrics for more information</returns>
    Task<bool> DeleteAsync(string[] ids);
}