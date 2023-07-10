using System;
using Newtonsoft.Json;

namespace Enov.API.Entities;

/// <summary>
/// The data model for the database
/// </summary>
public class Item
{
    /// <summary>
    /// The ID property. Same as ID AUTOINCREMENT PRIMARYKEY in database
    /// </summary>
    [JsonProperty("id")] public int Id { get; set; }
    
    /// <summary>
    /// The Name property. Readable data, can be updated, cannot be null
    /// </summary>
    [JsonProperty("name")] public string Name { get; set; }
    
    /// <summary>
    /// The property telling if this item is a new entry, or has been updated once or more.
    /// Cannot be NULL. Defaults to false if no entry was given
    /// </summary>
    [JsonProperty("is_new")] public bool IsNew { get; set; }
    
    /// <summary>
    /// The Created at property, TIMESTAMP in database
    /// </summary>
    [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// The updated at property, TIMESTAMP, on UPDATE CURRENT_TIMESTAMP() in database
    /// </summary>
    [JsonProperty("updated_at")] public DateTime UpdatedAt { get; set; }
}