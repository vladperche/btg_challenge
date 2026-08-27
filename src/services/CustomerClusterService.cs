using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Domains;
using Entities.Interfaces;
using Entities.Models;

namespace Services;

public class CustomerClusterService : ICustomerClusterService
{
    private readonly ICustomerClusterRepository _clusterRepository;
    private readonly IMarketDebtTypeRepository _marketDebtTypeRepository;

    public CustomerClusterService(
        ICustomerClusterRepository clusterRepository,
        IMarketDebtTypeRepository marketDebtTypeRepository)
    {
        _clusterRepository = clusterRepository;
        _marketDebtTypeRepository = marketDebtTypeRepository;
    }

    public async Task<IEnumerable<CustomerCluster>> GetAllAsync()
    {
        return await _clusterRepository.GetAllAsync();
    }

    public async Task<CustomerCluster?> GetByClusterIdAsync(string clusterId)
    {
        if (string.IsNullOrWhiteSpace(clusterId))
        {
            throw new DomainException("Cluster ID identifier cannot be empty.");
        }

        return await _clusterRepository.GetByClusterIdAsync(clusterId);
    }

    public async Task<CustomerCluster> SaveAsync(CustomerCluster customerCluster)
    {
        if (customerCluster == null)
        {
            throw new DomainException("Customer cluster data cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(customerCluster.ClusterId))
        {
            throw new DomainException("Cluster ID identifier is required and cannot be empty.");
        }

        if (customerCluster.Priority <= 0)
        {
            throw new DomainException("Priority must be a positive integer greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(customerCluster.Name))
        {
            throw new DomainException("Name description is required and cannot be empty.");
        }

        if (customerCluster.Score < 0)
        {
            throw new DomainException("Score must be a decimal greater than or equal to zero.");
        }

        if (customerCluster.AgeMin < 0)
        {
            throw new DomainException("AgeMin must be an integer greater than or equal to zero.");
        }

        if (customerCluster.AgeMax <= 0)
        {
            throw new DomainException("AgeMax must be a positive integer greater than zero.");
        }

        if (customerCluster.AgeMax < customerCluster.AgeMin)
        {
            throw new DomainException("AgeMax must be greater than or equal to AgeMin.");
        }

        if (!customerCluster.BaseLimit.HasValue || customerCluster.BaseLimit.Value < 0)
        {
            throw new DomainException("BaseLimit is required and must be a decimal greater than or equal to zero.");
        }

        if (!customerCluster.CapLimit.HasValue || customerCluster.CapLimit.Value < 0)
        {
            throw new DomainException("CapLimit is required and must be a decimal greater than or equal to zero.");
        }

        if (customerCluster.CapLimit.Value < customerCluster.BaseLimit.Value)
        {
            throw new DomainException("CapLimit must be greater than or equal to BaseLimit.");
        }

        // DebtConditionMarketTypes handling: if null, default to empty array []
        if (customerCluster.DebtConditionMarketTypes == null)
        {
            customerCluster.DebtConditionMarketTypes = Array.Empty<string>();
        }
        else if (customerCluster.DebtConditionMarketTypes.Length > 0)
        {
            // Validate that provided items are non-empty and check existence in Market Debt Types
            foreach (var debtType in customerCluster.DebtConditionMarketTypes)
            {
                if (string.IsNullOrWhiteSpace(debtType))
                {
                    throw new DomainException("DebtConditionMarketTypes elements must not be empty or null.");
                }

                var existingType = await _marketDebtTypeRepository.GetByValueAsync(debtType);
                if (existingType == null)
                {
                    throw new DomainException($"Market Debt Type '{debtType}' does not exist.");
                }
            }
        }

        return await _clusterRepository.SaveAsync(customerCluster);
    }

    public async Task<bool> DeleteAsync(string clusterId)
    {
        if (string.IsNullOrWhiteSpace(clusterId))
        {
            throw new DomainException("Cluster ID identifier cannot be empty.");
        }

        return await _clusterRepository.DeleteAsync(clusterId);
    }
}
