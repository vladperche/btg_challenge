using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;

namespace Entities.Interfaces;

public interface ICustomerClusterRepository
{
    Task<IEnumerable<CustomerCluster>> GetAllAsync();
    Task<CustomerCluster?> GetByClusterIdAsync(string clusterId);
    Task<CustomerCluster> SaveAsync(CustomerCluster customerCluster);
    Task<bool> DeleteAsync(string clusterId);
}
