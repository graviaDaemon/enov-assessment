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
            // Check if any connection exists before trying to make a new connection
            if (_connection.State == ConnectionState.Closed)
                await _connection.OpenAsync();

            // Loop over each item and insert each into a new row in the table
            // and then sum up the number of entries that were successful

            int numberOfRows = 0;
            foreach (Item item in items)
            {
                string cmdText =
                    $"INSERT INTO names (id, name, is_new) VALUES ({item.Id}, '{item.Name}', {item.IsNew})";
                MySqlCommand cmd = new(cmdText, _connection);
                numberOfRows += cmd.ExecuteNonQuery();
                await SingleMetric(JsonConvert.SerializeObject(item), "Create", "POST");
            }

            return items.Count() == numberOfRows ? items : new List<Item>();
        }
        catch (Exception ex)
        {
            await BulkMetric(items.Select(JsonConvert.SerializeObject).ToArray(), "Create", "POST", ex);
            // TODO: Return the result items that were successful excluding everything from the exception to the end of the list.
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
            string cmdText = $"SELECT * FROM names WHERE id in ({string.Join(", ", ids)})";
            MySqlCommand cmd = new(cmdText, _connection);
            await using (MySqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
            {
                List<Item> buffer = new();

                while (await reader.ReadAsync())
                {
                    Item item = new Item();

                    if (int.TryParse(reader["id"].ToString(), out int id))
                    {
                        item.Id = id;
                    }

                    item.Name = reader["name"].ToString();
                    if (bool.TryParse(reader["is_new"].ToString(), out bool isn))
                    {
                        item.IsNew = isn;
                    }

                    if (DateTime.TryParse(reader["created_at"].ToString(), out DateTime cDate))
                    {
                        item.CreatedAt = cDate;
                    }

                    if (DateTime.TryParse(reader["updated_at"].ToString(), out DateTime uDate))
                    {
                        item.UpdatedAt = uDate;
                    }

                    buffer.Add(item);
                }

                // TODO: Create a check that only the items that were "successful" are added to the range
                results.AddRange(buffer);
            }

            await BulkMetric(ids, "Read", "GET");

            return results.ToArray();
        }
        catch (Exception ex)
        {
            await BulkMetric(ids, "Read", "GET", ex);
            // TODO: Return the result items that were successful excluding everything from the exception to the end of the list. 
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

            int affectedRows = 0;
            foreach (Item item in items)
            {
                string cmdText = $"UPDATE names SET name = '{item.Name}', is_new = {false} WHERE id = {item.Id}";
                MySqlCommand cmd = new(cmdText, _connection);
                affectedRows += cmd.ExecuteNonQuery();
                await SingleMetric(JsonConvert.SerializeObject(item), "Update", "PUT");
            }

            return items.Count() == affectedRows ? items : new List<Item>();
        }
        catch (Exception ex)
        {
            await BulkMetric(items.Select(JsonConvert.SerializeObject).ToArray(), "Update", "PUT", ex);
            // TODO: Return the result items that were successful excluding everything from the exception to the end of the list.
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

            int numberOfRows = 0;
            foreach (string id in ids)
            {
                string cmdText = $"DELETE FROM names WHERE id = {id}";
                MySqlCommand cmd = new(cmdText, _connection);
                numberOfRows += cmd.ExecuteNonQuery();
                await SingleMetric(id, "Delete", "DELETE");
            }
            
            return ids.Length == numberOfRows;
        }
        catch (Exception ex)
        {
            await BulkMetric(ids, "Delete", "DELETE", ex);

            return false;
        }
        finally
        {
            if (_connection.State != ConnectionState.Closed)
                await _connection.CloseAsync();
        }
    }

    private async Task<int> BulkMetric(string[] data, string func, string method, Exception ex = null)
    {
        try
        {
            if (_connection.State == ConnectionState.Closed)
                await _connection.OpenAsync();

            int num = (from item in data
                select
                    $"INSERT INTO logging (request, payload, method, exception)VALUES ('{func}', '{string.Join(", ", data)}', '{method}', '{ex?.Message}')"
                into cmdText
                select new MySqlCommand(cmdText, _connection)
                into cmd
                select cmd.ExecuteNonQuery()).Sum();

            return num;
        }
        catch (Exception exc)
        {
            int num = (from item in data
                select
                    $"INSERT INTO logging (request, payload, method, exception)VALUES ('{func}', '{string.Join(", ", data)}', '{method}', '{ex?.Message}')"
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

    private async Task SingleMetric(string data, string func, string method, Exception ex = null)
    {
        try
        {
            if (_connection.State == ConnectionState.Closed)
                await _connection.OpenAsync();

            string cmdText =
                $"INSERT INTO logging (request, payload, method, exception) VALUES ('{func}', '{data}', '{method}', '{ex?.Message}')";
            MySqlCommand cmd = new(cmdText, _connection);
            cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            string cmdText =
                $"INSERT INTO logging (request, payload, method, exception) VALUES ('{func}', '{data}', '{method}', '{ex?.Message}')";
            MySqlCommand cmd = new(cmdText, _connection);
            cmd.ExecuteNonQuery();
        }
        finally
        {
            if (_connection.State != ConnectionState.Closed)
                await _connection.CloseAsync();
        }
    }
}