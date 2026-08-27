using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;

namespace Entities.Interfaces;

public interface IMonthlyIncomeService
{
    Task<IEnumerable<MonthlyIncome>> GetAllAsync();
    Task<IEnumerable<MonthlyIncome>> GetByCategoryAsync(string category);
    Task<MonthlyIncome?> GetByCompositeKeyAsync(string category, string clusterId);
    Task<MonthlyIncome> SaveAsync(MonthlyIncome monthlyIncome);
    Task<bool> DeleteAsync(string category, string clusterId);
}
