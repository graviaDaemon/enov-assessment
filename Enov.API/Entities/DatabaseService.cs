using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;

namespace Enov.API.Entities;

public class DatabaseService : IDatabaseService
{
    private readonly MySqlConnection _connection;

    public DatabaseService(MySqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<Item>> CreateAsync(IEnumerable<Item> items)
    {
        _connection.Open();

        int numberOfRows = 
            (from data in items select "INSERT INTO names (id, name, is_new) " 
                                       + "VALUES  (" + data.Id + ", '" + data.Name + "', " + data.IsNew + ")" 
                into cmdText select new MySqlCommand(cmdText, _connection) 
                into cmd select cmd.ExecuteNonQuery()).Sum();

        await _connection.CloseAsync();
        return items.Count() == numberOfRows ? items : new List<Item>();
    }

    public async Task<IEnumerable<Item>> GetAsync(string[] ids)
    {
        List<Item> results = new();
        _connection.Open();

        string cmdText = "SELECT * FROM names WHERE id in (" + string.Join(", ", ids) + ")";
        MySqlCommand cmd = new(cmdText, _connection);
        MySqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (reader.Read())
        {
            Item item = new Item();
            if (int.TryParse(reader["id"].ToString(), out int id)) { item.Id = id; }
            item.Name = reader["name"].ToString();
            if (bool.TryParse(reader["is_new"].ToString(), out bool isn)) { item.IsNew = isn; }
            if (DateTime.TryParse(reader["created_at"].ToString(), out DateTime cDate)) { item.CreatedAt = cDate; }
            if (DateTime.TryParse(reader["updated_at"].ToString(), out DateTime uDate)) { item.UpdatedAt = uDate; }
            
            results.Add(item);
        }
        await _connection.CloseAsync();
        return results.ToArray();
    }

    public async Task<IEnumerable<Item>> UpdateAsync(IEnumerable<Item> items)
    {
        _connection.Open();
        int affectedRows = (from data in items
            select "UPDATE names SET name = '" + data.Name + "', is_new = " + false + " WHERE id = " + data.Id
            into cmdText
            select new MySqlCommand(cmdText, _connection)
            into cmd
            select cmd.ExecuteNonQuery()).Sum();

        await _connection.CloseAsync();
        return items.Count() == affectedRows ? items : new List<Item>();
    }

    public async Task<bool> DeleteAsync(string[] ids)
    {
        _connection.Open();
        int numberOfRows =
            (from id in ids
                select "DELETE from names WHERE id = " + id
                into cmdText
                select new MySqlCommand(cmdText, _connection)
                into cmd
                select cmd.ExecuteNonQuery()).Sum();

        await _connection.CloseAsync();
        return ids.Length == numberOfRows;
    }
}