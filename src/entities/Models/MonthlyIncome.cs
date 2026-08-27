namespace Entities.Models;

public class MonthlyIncome
{
    public string Category { get; set; } = string.Empty;
    public string ClusterId { get; set; } = string.Empty;
    public decimal Income { get; set; }
}
