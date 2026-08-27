using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;

namespace Entities.Interfaces;

public interface IPenaltyRuleService
{
    Task<IEnumerable<PenaltyRule>> GetAllAsync();
    Task<PenaltyRule?> GetByRuleIdAsync(string ruleId);
    Task<PenaltyRule> SaveAsync(PenaltyRule penaltyRule);
    Task<bool> DeleteAsync(string ruleId);
}
