using System.Threading.Tasks;
using Entities.Interfaces;
using Entities.Models;
using Repositories.Context;

namespace Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly MongoDbContext _context;

    public CustomerRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> SaveAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }
}
