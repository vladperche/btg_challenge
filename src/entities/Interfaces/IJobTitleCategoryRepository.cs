using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;

namespace Entities.Interfaces;

public interface IJobTitleCategoryRepository
{
    Task<IEnumerable<JobTitleCategory>> GetAllAsync();
    Task<JobTitleCategory?> GetByCategoryAsync(string category);
    Task<JobTitleCategory> SaveAsync(JobTitleCategory jobTitleCategory);
    Task<bool> DeleteAsync(string category);
}
