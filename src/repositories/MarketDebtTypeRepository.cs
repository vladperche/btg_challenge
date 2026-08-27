using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Constants;
using Entities.Interfaces;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Context;

namespace Repositories;

public class MarketDebtTypeRepository : IMarketDebtTypeRepository
{
    private readonly MongoDbContext _context;
    private readonly ICacheService _cacheService;

    public MarketDebtTypeRepository(MongoDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<MarketDebtType>> GetAllAsync()
    {
        return await _context.MarketDebtTypes.AsNoTracking().ToListAsync();
    }

    public async Task<MarketDebtType?> GetByValueAsync(string value)
    {
        var cacheKey = CacheConstants.MarketDebtTypeKey(value);

        // 1. Cache-Aside Check: Check Redis cache first
        var cachedRecord = await _cacheService.GetAsync<MarketDebtType>(cacheKey);
        if (cachedRecord != null)
        {
            return cachedRecord;
        }

        // 2. Cache Miss: Fetch from MongoDB database
        var record = await _context.MarketDebtTypes.FirstOrDefaultAsync(m => m.Value == value);

        // 3. Cache-Aside Populate: Save to Redis if found
        if (record != null)
        {
            await _cacheService.SetAsync(cacheKey, record, TimeSpan.FromMinutes(AppConstants.DefaultCacheExpirationMinutes));
        }

        return record;
    }

    public async Task<MarketDebtType> SaveAsync(MarketDebtType marketDebtType)
    {
        var existing = await _context.MarketDebtTypes.FirstOrDefaultAsync(m => m.Value == marketDebtType.Value);

        if (existing != null)
        {
            // Update existing record
            existing.Meaning = marketDebtType.Meaning;
            _context.MarketDebtTypes.Update(existing);
        }
        else
        {
            // Insert new record
            _context.MarketDebtTypes.Add(marketDebtType);
        }

        await _context.SaveChangesAsync();

        // Update Redis cache after successful persistence
        var cacheKey = CacheConstants.MarketDebtTypeKey(marketDebtType.Value);
        await _cacheService.SetAsync(cacheKey, marketDebtType, TimeSpan.FromMinutes(AppConstants.DefaultCacheExpirationMinutes));

        return marketDebtType;
    }

    public async Task<bool> DeleteAsync(string value)
    {
        var existing = await _context.MarketDebtTypes.FirstOrDefaultAsync(m => m.Value == value);
        if (existing == null)
        {
            return false;
        }

        _context.MarketDebtTypes.Remove(existing);
        await _context.SaveChangesAsync();

        // Invalidate Redis cache
        var cacheKey = CacheConstants.MarketDebtTypeKey(value);
        await _cacheService.RemoveAsync(cacheKey);

        return true;
    }
}
