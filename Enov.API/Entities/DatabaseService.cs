using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using Newtonsoft.Json;

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
        try
        {
            if (_connection.State == ConnectionState.Closed)
                await _connection.OpenAsync();

            int numberOfRows =
                (from data in items
                    select "INSERT INTO names (id, name, is_new) "
                           + "VALUES  (" + data.Id + ", '" + data.Name + "', " + data.IsNew + ")"
                    into cmdText
                    select new MySqlCommand(cmdText, _connection)
                    into cmd
                    select cmd.ExecuteNonQuery()).Sum();

            await Metric(items.Select(JsonConvert.SerializeObject).ToArray(), "Create", "POST");

            return items.Count() == numberOfRows ? items : new List<Item>();
        }
        catch (Exception ex)
        {
            await Metric(items.Select(JsonConvert.SerializeObject).ToArray(), "Create", "POST", ex);

            return Array.Empty<Item>();
        }
        finally
        {
            if (_connection.State != ConnectionState.Closed)
                await _connection.CloseAsync();
        }

        
    }

    public async Task<IEnumerable<Item>> GetAsync(string[] ids)
    {
        try
        {
            if (_connection.State == ConnectionState.Closed)
                await _connection.OpenAsync();
                
            List<Item> results = new();
            string cmdText = "SELECT * FROM names WHERE id in (" + string.Join(", ", ids) + ")";
            MySqlCommand cmd = new(cmdText, _connection);
            await using (MySqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
            {
                List<Item> buffer = new();
            
                while (await reader.ReadAsync())
                {
                    Item item = new Item();

                    if (int.TryParse(reader["id"].ToString(), out int id)) { item.Id = id; }
                    item.Name = reader["name"].ToString();
                    if (bool.TryParse(reader["is_new"].ToString(), out bool isn)) { item.IsNew = isn; }
                    if (DateTime.TryParse(reader["created_at"].ToString(), out DateTime cDate)) { item.CreatedAt = cDate; }
                    if (DateTime.TryParse(reader["updated_at"].ToString(), out DateTime uDate)) { item.UpdatedAt = uDate; }

                    buffer.Add(item);
                }
                results.AddRange(buffer);
            }

            await Metric(ids, "Read", "GET");
            
            return results.ToArray();
        }
        catch (Exception ex)
        {
            await Metric(ids, "Read", "GET", ex);
            return Array.Empty<Item>();
        }
        finally
        {
            if (_connection.State != ConnectionState.Closed)
                await _connection.CloseAsync();
        }
    }

    public async Task<IEnumerable<Item>> UpdateAsync(IEnumerable<Item> items)
    {
        try
        {
            if (_connection.State == ConnectionState.Closed)
                await _connection.OpenAsync();
            
            int affectedRows = (from data in items
                select "UPDATE names SET name = '" + data.Name + "', is_new = " + false + " WHERE id = " + data.Id
                into cmdText
                select new MySqlCommand(cmdText, _connection)
                into cmd
                select cmd.ExecuteNonQuery()).Sum();

            await Metric(items.Select(JsonConvert.SerializeObject).ToArray(), "Update", "PUT");
            
            return items.Count() == affectedRows ? items : new List<Item>();
        }
        catch (Exception ex)
        {
            await Metric(items.Select(JsonConvert.SerializeObject).ToArray(), "Update", "PUT", ex);
            
            return Array.Empty<Item>();
        }
        finally
        {
            if (_connection.State != ConnectionState.Closed)
                await _connection.CloseAsync();
        }
    }

    public async Task<bool> DeleteAsync(string[] ids)
    {
        try
        {
            if (_connection.State == ConnectionState.Closed)
                await _connection.OpenAsync();
            
            int numberOfRows =
                (from id in ids
                    select "DELETE from names WHERE id = " + id
                    into cmdText
                    select new MySqlCommand(cmdText, _connection)
                    into cmd
                    select cmd.ExecuteNonQuery()).Sum();
            
            await Metric(ids, "Delete", "DELETE");
            
            return ids.Length == numberOfRows;
        }
        catch (Exception ex)
        {
            await Metric(ids, "Delete", "DELETE", ex);
            
            return false;
        }
        finally
        {
            if (_connection.State != ConnectionState.Closed)
                await _connection.CloseAsync();
        }
    }

    private async Task<int> Metric(string[] data, string func, string method, Exception ex = null)
    {
        try
        {
            if (_connection.State == ConnectionState.Closed)
                await _connection.OpenAsync();

            int num = (from item in data
                select "INSERT INTO logging (request, payload, method, exception)" +
                       "VALUES ('" + func + "', '" + string.Join(", ", data) + "', '" + method + "', '" + ex?.Message +
                       "')"
                into cmdText
                select new MySqlCommand(cmdText, _connection)
                into cmd
                select cmd.ExecuteNonQuery()).Sum();

            return num;
        }
        catch (Exception exc)
        {
            int num = (from item in data
                select "INSERT INTO logging (request, payload, method, exception)" +
                       "VALUES ('" + func + "', '" + string.Join(", ", data) + "', '" + method + "', '" + ex?.Message +
                       "')"
                into cmdText
                select new MySqlCommand(cmdText, _connection)
                into cmd
                select cmd.ExecuteNonQuery()).Sum();

            return num;
        }
        finally
        {
            if (_connection.State != ConnectionState.Closed)
                await _connection.CloseAsync();
        }
    }
}