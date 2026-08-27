using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Domains;
using Entities.Interfaces;
using Entities.Models;

namespace Services;

public class PenaltyRuleService : IPenaltyRuleService
{
    private readonly IPenaltyRuleRepository _ruleRepository;
    private readonly IMarketDebtTypeRepository _marketDebtTypeRepository;

    public PenaltyRuleService(
        IPenaltyRuleRepository ruleRepository,
        IMarketDebtTypeRepository marketDebtTypeRepository)
    {
        _ruleRepository = ruleRepository;
        _marketDebtTypeRepository = marketDebtTypeRepository;
    }

    public async Task<IEnumerable<PenaltyRule>> GetAllAsync()
    {
        return await _ruleRepository.GetAllAsync();
    }

    public async Task<PenaltyRule?> GetByRuleIdAsync(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId))
        {
            throw new DomainException("Rule ID identifier cannot be empty.");
        }

        return await _ruleRepository.GetByRuleIdAsync(ruleId);
    }

    public async Task<PenaltyRule> SaveAsync(PenaltyRule penaltyRule)
    {
        if (penaltyRule == null)
        {
            throw new DomainException("Penalty rule data cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(penaltyRule.RuleId))
        {
            throw new DomainException("Rule ID identifier is required and cannot be empty.");
        }

        if (penaltyRule.Priority < 0)
        {
            throw new DomainException("Priority must be an integer greater than or equal to zero.");
        }

        if (!penaltyRule.Effect.HasValue || penaltyRule.Effect.Value <= 0)
        {
            throw new DomainException("Effect is required and must be a decimal greater than zero.");
        }

        // Trigger handling: if null, default to empty array []
        if (penaltyRule.Trigger == null)
        {
            penaltyRule.Trigger = Array.Empty<string>();
        }
        else if (penaltyRule.Trigger.Length > 0)
        {
            // Validate that provided items are non-empty and check existence in Market Debt Types
            foreach (var debtType in penaltyRule.Trigger)
            {
                if (string.IsNullOrWhiteSpace(debtType))
                {
                    throw new DomainException("Trigger elements must not be empty or null.");
                }

                var existingType = await _marketDebtTypeRepository.GetByValueAsync(debtType);
                if (existingType == null)
                {
                    throw new DomainException($"Market Debt Type '{debtType}' does not exist.");
                }
            }
        }

        return await _ruleRepository.SaveAsync(penaltyRule);
    }

    public async Task<bool> DeleteAsync(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId))
        {
            throw new DomainException("Rule ID identifier cannot be empty.");
        }

        return await _ruleRepository.DeleteAsync(ruleId);
    }
}
