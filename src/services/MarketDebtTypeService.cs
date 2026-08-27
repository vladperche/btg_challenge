using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Domains;
using Entities.Interfaces;
using Entities.Models;

namespace Services;

public class MarketDebtTypeService : IMarketDebtTypeService
{
    private readonly IMarketDebtTypeRepository _repository;

    public MarketDebtTypeService(IMarketDebtTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<MarketDebtType>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<MarketDebtType?> GetByValueAsync(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Value identifier cannot be empty.");
        }

        return await _repository.GetByValueAsync(value);
    }

    public async Task<MarketDebtType> SaveAsync(MarketDebtType marketDebtType)
    {
        if (marketDebtType == null)
        {
            throw new DomainException("Market debt type data cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(marketDebtType.Value))
        {
            throw new DomainException("Value identifier is required and cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(marketDebtType.Meaning))
        {
            throw new DomainException("Meaning description is required and cannot be empty.");
        }

        return await _repository.SaveAsync(marketDebtType);
    }

    public async Task<bool> DeleteAsync(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Value identifier cannot be empty.");
        }

        return await _repository.DeleteAsync(value);
    }
}
