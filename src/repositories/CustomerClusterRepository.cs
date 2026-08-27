using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Constants;
using Entities.Interfaces;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Context;

namespace Repositories;

public class CustomerClusterRepository : ICustomerClusterRepository
{
    private readonly MongoDbContext _context;
    private readonly ICacheService _cacheService;

    public CustomerClusterRepository(MongoDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<CustomerCluster>> GetAllAsync()
    {
        return await _context.CustomerClusters.AsNoTracking().ToListAsync();
    }

    public async Task<CustomerCluster?> GetByClusterIdAsync(string clusterId)
    {
        var cacheKey = CacheConstants.CustomerClusterKey(clusterId);

        // 1. Cache-Aside Check: Check Redis cache first
        var cachedRecord = await _cacheService.GetAsync<CustomerCluster>(cacheKey);
        if (cachedRecord != null)
        {
            return cachedRecord;
        }

        // 2. Cache Miss: Fetch from MongoDB database
        var record = await _context.CustomerClusters.FirstOrDefaultAsync(c => c.ClusterId == clusterId);

        // 3. Cache-Aside Populate: Save to Redis if found
        if (record != null)
        {
            await _cacheService.SetAsync(cacheKey, record, TimeSpan.FromMinutes(AppConstants.DefaultCacheExpirationMinutes));
        }

        return record;
    }

    public async Task<CustomerCluster> SaveAsync(CustomerCluster customerCluster)
    {
        var existing = await _context.CustomerClusters.FirstOrDefaultAsync(c => c.ClusterId == customerCluster.ClusterId);

        if (existing != null)
        {
            existing.Priority = customerCluster.Priority;
            existing.Name = customerCluster.Name;
            existing.Score = customerCluster.Score;
            existing.AgeMin = customerCluster.AgeMin;
            existing.AgeMax = customerCluster.AgeMax;
            existing.BaseLimit = customerCluster.BaseLimit;
            existing.CapLimit = customerCluster.CapLimit;
            existing.DebtConditionMarketDebtCheck = customerCluster.DebtConditionMarketDebtCheck;
            existing.DebtConditionMarketTypes = customerCluster.DebtConditionMarketTypes;
            _context.CustomerClusters.Update(existing);
        }
        else
        {
            _context.CustomerClusters.Add(customerCluster);
        }

        await _context.SaveChangesAsync();

        // Update Redis cache after successful persistence
        var cacheKey = CacheConstants.CustomerClusterKey(customerCluster.ClusterId);
        await _cacheService.SetAsync(cacheKey, customerCluster, TimeSpan.FromMinutes(AppConstants.DefaultCacheExpirationMinutes));

        return customerCluster;
    }

    public async Task<bool> DeleteAsync(string clusterId)
    {
        var existing = await _context.CustomerClusters.FirstOrDefaultAsync(c => c.ClusterId == clusterId);
        if (existing == null)
        {
            return false;
        }

        _context.CustomerClusters.Remove(existing);
        await _context.SaveChangesAsync();

        // Invalidate Redis cache
        var cacheKey = CacheConstants.CustomerClusterKey(clusterId);
        await _cacheService.RemoveAsync(cacheKey);

        return true;
    }
}
