using System;

namespace Entities.Models;

public class CustomerClassificationRequest
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public int? Score { get; set; }
    public bool HasMarketDebt { get; set; }
    public string[] MarketDebtTypes { get; set; } = Array.Empty<string>();
    public LocationInfo Location { get; set; } = new();
    public string JobTitle { get; set; } = string.Empty;
}
