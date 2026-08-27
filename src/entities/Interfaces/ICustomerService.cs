using System.Threading.Tasks;
using Entities.Models;

namespace Entities.Interfaces;

public interface ICustomerService
{
    Task<Customer> ProcessAndSaveAsync(CustomerClassificationRequest request);
}
