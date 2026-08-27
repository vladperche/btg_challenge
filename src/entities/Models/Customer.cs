using System;

namespace Entities.Models;

public class Customer
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public int? Score { get; set; }
    public bool HasMarketDebt { get; set; }
    public string[] MarketDebtTypes { get; set; } = Array.Empty<string>();
    public LocationInfo Location { get; set; } = new();
    public string JobTitle { get; set; } = string.Empty;

    // Enriched Fields
    public string JobCategory { get; set; } = string.Empty;
    public decimal JobMultiplier { get; set; }
    public string CustomerCluster { get; set; } = string.Empty;
    public string ClusterName { get; set; } = string.Empty;
    public decimal BaseLimit { get; set; }
    public decimal CapLimit { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal PenaltyFactor { get; set; }
    public decimal ApprovedLimit { get; set; }
}
