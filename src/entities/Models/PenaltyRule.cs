using System;

namespace Entities.Models;

public class PenaltyRule
{
    public string RuleId { get; set; } = string.Empty;
    public int Priority { get; set; }
    public decimal? Effect { get; set; }
    public string[] Trigger { get; set; } = Array.Empty<string>();
}
