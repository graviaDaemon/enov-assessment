using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enov.API.Entities;

public interface IDatabaseService
{
    Task<IEnumerable<Item>> CreateAsync(IEnumerable<Item> items);
    Task<IEnumerable<Item>> GetAsync(string[] ids);
    Task<IEnumerable<Item>> UpdateAsync(IEnumerable<Item> items);
    Task<bool> DeleteAsync(string[] ids);
}