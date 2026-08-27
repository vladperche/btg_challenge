using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Constants;
using Entities.Interfaces;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Context;

namespace Repositories;

public class PenaltyRuleRepository : IPenaltyRuleRepository
{
    private readonly MongoDbContext _context;
    private readonly ICacheService _cacheService;

    public PenaltyRuleRepository(MongoDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<PenaltyRule>> GetAllAsync()
    {
        return await _context.PenaltyRules.AsNoTracking().ToListAsync();
    }

    public async Task<PenaltyRule?> GetByRuleIdAsync(string ruleId)
    {
        var cacheKey = CacheConstants.PenaltyRuleKey(ruleId);

        // 1. Cache-Aside Check: Check Redis cache first
        var cachedRecord = await _cacheService.GetAsync<PenaltyRule>(cacheKey);
        if (cachedRecord != null)
        {
            return cachedRecord;
        }

        // 2. Cache Miss: Fetch from MongoDB database
        var record = await _context.PenaltyRules.FirstOrDefaultAsync(p => p.RuleId == ruleId);

        // 3. Cache-Aside Populate: Save to Redis if found
        if (record != null)
        {
            await _cacheService.SetAsync(cacheKey, record, TimeSpan.FromMinutes(AppConstants.DefaultCacheExpirationMinutes));
        }

        return record;
    }

    public async Task<PenaltyRule> SaveAsync(PenaltyRule penaltyRule)
    {
        var existing = await _context.PenaltyRules.FirstOrDefaultAsync(p => p.RuleId == penaltyRule.RuleId);

        if (existing != null)
        {
            existing.Priority = penaltyRule.Priority;
            existing.Effect = penaltyRule.Effect;
            existing.Trigger = penaltyRule.Trigger;
            _context.PenaltyRules.Update(existing);
        }
        else
        {
            _context.PenaltyRules.Add(penaltyRule);
        }

        await _context.SaveChangesAsync();

        // Update Redis cache after successful persistence
        var cacheKey = CacheConstants.PenaltyRuleKey(penaltyRule.RuleId);
        await _cacheService.SetAsync(cacheKey, penaltyRule, TimeSpan.FromMinutes(AppConstants.DefaultCacheExpirationMinutes));

        return penaltyRule;
    }

    public async Task<bool> DeleteAsync(string ruleId)
    {
        var existing = await _context.PenaltyRules.FirstOrDefaultAsync(p => p.RuleId == ruleId);
        if (existing == null)
        {
            return false;
        }

        _context.PenaltyRules.Remove(existing);
        await _context.SaveChangesAsync();

        // Invalidate Redis cache
        var cacheKey = CacheConstants.PenaltyRuleKey(ruleId);
        await _cacheService.RemoveAsync(cacheKey);

        return true;
    }
}
