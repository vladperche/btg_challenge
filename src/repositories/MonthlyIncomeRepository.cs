using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Constants;
using Entities.Interfaces;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Context;

namespace Repositories;

public class MonthlyIncomeRepository : IMonthlyIncomeRepository
{
    private readonly MongoDbContext _context;
    private readonly ICacheService _cacheService;

    public MonthlyIncomeRepository(MongoDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<MonthlyIncome>> GetAllAsync()
    {
        return await _context.MonthlyIncomes.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<MonthlyIncome>> GetByCategoryAsync(string category)
    {
        return await _context.MonthlyIncomes.AsNoTracking()
            .Where(m => m.Category == category)
            .ToListAsync();
    }

    public async Task<MonthlyIncome?> GetByCompositeKeyAsync(string category, string clusterId)
    {
        var cacheKey = CacheConstants.MonthlyIncomeKey(category, clusterId);

        // 1. Cache-Aside Check: Check Redis cache first
        var cachedRecord = await _cacheService.GetAsync<MonthlyIncome>(cacheKey);
        if (cachedRecord != null)
        {
            return cachedRecord;
        }

        // 2. Cache Miss: Fetch from MongoDB database
        var record = await _context.MonthlyIncomes.FirstOrDefaultAsync(m => m.Category == category && m.ClusterId == clusterId);

        // 3. Cache-Aside Populate: Save to Redis if found
        if (record != null)
        {
            await _cacheService.SetAsync(cacheKey, record, TimeSpan.FromMinutes(AppConstants.DefaultCacheExpirationMinutes));
        }

        return record;
    }

    public async Task<MonthlyIncome> SaveAsync(MonthlyIncome monthlyIncome)
    {
        var existing = await _context.MonthlyIncomes.FirstOrDefaultAsync(m => m.Category == monthlyIncome.Category && m.ClusterId == monthlyIncome.ClusterId);

        if (existing != null)
        {
            existing.Income = monthlyIncome.Income;
            _context.MonthlyIncomes.Update(existing);
        }
        else
        {
            _context.MonthlyIncomes.Add(monthlyIncome);
        }

        await _context.SaveChangesAsync();

        // Update Redis cache after successful persistence
        var cacheKey = CacheConstants.MonthlyIncomeKey(monthlyIncome.Category, monthlyIncome.ClusterId);
        await _cacheService.SetAsync(cacheKey, monthlyIncome, TimeSpan.FromMinutes(AppConstants.DefaultCacheExpirationMinutes));

        return monthlyIncome;
    }

    public async Task<bool> DeleteAsync(string category, string clusterId)
    {
        var existing = await _context.MonthlyIncomes.FirstOrDefaultAsync(m => m.Category == category && m.ClusterId == clusterId);
        if (existing == null)
        {
            return false;
        }

        _context.MonthlyIncomes.Remove(existing);
        await _context.SaveChangesAsync();

        // Invalidate Redis cache
        var cacheKey = CacheConstants.MonthlyIncomeKey(category, clusterId);
        await _cacheService.RemoveAsync(cacheKey);

        return true;
    }
}
