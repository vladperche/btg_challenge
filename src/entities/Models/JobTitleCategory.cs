using System;

namespace Entities.Models;

public class JobTitleCategory
{
    public string Category { get; set; } = string.Empty;
    public int Priority { get; set; }
    public decimal Multiplier { get; set; }
    public string[] Keywords { get; set; } = Array.Empty<string>();
}
