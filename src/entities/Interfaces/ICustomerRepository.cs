using System.Threading.Tasks;
using Entities.Models;

namespace Entities.Interfaces;

public interface ICustomerRepository
{
    Task<Customer> SaveAsync(Customer customer);
}
