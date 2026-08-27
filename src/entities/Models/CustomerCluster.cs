using System;

namespace Entities.Models;

public class CustomerCluster
{
    public string ClusterId { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public int AgeMin { get; set; }
    public int AgeMax { get; set; }
    public decimal? BaseLimit { get; set; }
    public decimal? CapLimit { get; set; }
    public bool DebtConditionMarketDebtCheck { get; set; } = false;
    public string[] DebtConditionMarketTypes { get; set; } = Array.Empty<string>();
}
