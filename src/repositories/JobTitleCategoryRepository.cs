using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Constants;
using Entities.Interfaces;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Context;

namespace Repositories;

public class JobTitleCategoryRepository : IJobTitleCategoryRepository
{
    private readonly MongoDbContext _context;
    private readonly ICacheService _cacheService;

    public JobTitleCategoryRepository(MongoDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<JobTitleCategory>> GetAllAsync()
    {
        return await _context.JobTitleCategories.AsNoTracking().ToListAsync();
    }

    public async Task<JobTitleCategory?> GetByCategoryAsync(string category)
    {
        var cacheKey = CacheConstants.JobTitleCategoryKey(category);

        // 1. Cache-Aside Check: Check Redis cache first
        var cachedRecord = await _cacheService.GetAsync<JobTitleCategory>(cacheKey);
        if (cachedRecord != null)
        {
            return cachedRecord;
        }

        // 2. Cache Miss: Fetch from MongoDB database
        var record = await _context.JobTitleCategories.FirstOrDefaultAsync(j => j.Category == category);

        // 3. Cache-Aside Populate: Save to Redis if found
        if (record != null)
        {
            await _cacheService.SetAsync(cacheKey, record, TimeSpan.FromMinutes(AppConstants.DefaultCacheExpirationMinutes));
        }

        return record;
    }

    public async Task<JobTitleCategory> SaveAsync(JobTitleCategory jobTitleCategory)
    {
        var existing = await _context.JobTitleCategories.FirstOrDefaultAsync(j => j.Category == jobTitleCategory.Category);

        if (existing != null)
        {
            existing.Priority = jobTitleCategory.Priority;
            existing.Multiplier = jobTitleCategory.Multiplier;
            existing.Keywords = jobTitleCategory.Keywords;
            _context.JobTitleCategories.Update(existing);
        }
        else
        {
            _context.JobTitleCategories.Add(jobTitleCategory);
        }

        await _context.SaveChangesAsync();

        // Update Redis cache after successful persistence
        var cacheKey = CacheConstants.JobTitleCategoryKey(jobTitleCategory.Category);
        await _cacheService.SetAsync(cacheKey, jobTitleCategory, TimeSpan.FromMinutes(AppConstants.DefaultCacheExpirationMinutes));

        return jobTitleCategory;
    }

    public async Task<bool> DeleteAsync(string category)
    {
        var existing = await _context.JobTitleCategories.FirstOrDefaultAsync(j => j.Category == category);
        if (existing == null)
        {
            return false;
        }

        _context.JobTitleCategories.Remove(existing);
        await _context.SaveChangesAsync();

        // Invalidate Redis cache
        var cacheKey = CacheConstants.JobTitleCategoryKey(category);
        await _cacheService.RemoveAsync(cacheKey);

        return true;
    }
}
