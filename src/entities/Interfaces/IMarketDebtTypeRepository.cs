using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;

namespace Entities.Interfaces;

public interface IMarketDebtTypeRepository
{
    Task<IEnumerable<MarketDebtType>> GetAllAsync();
    Task<MarketDebtType?> GetByValueAsync(string value);
    Task<MarketDebtType> SaveAsync(MarketDebtType marketDebtType);
    Task<bool> DeleteAsync(string value);
}
