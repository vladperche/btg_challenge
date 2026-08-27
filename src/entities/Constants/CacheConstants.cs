namespace Entities.Constants;

public static class CacheConstants
{
    public const string MarketDebtTypePrefix = "marketdebttype:";
    public static string MarketDebtTypeKey(string value) => $"{MarketDebtTypePrefix}{value}";

    public const string JobTitleCategoryPrefix = "jobtitlecategory:";
    public static string JobTitleCategoryKey(string category) => $"{JobTitleCategoryPrefix}{category}";

    public const string CustomerClusterPrefix = "customercluster:";
    public static string CustomerClusterKey(string clusterId) => $"{CustomerClusterPrefix}{clusterId}";

    public const string MonthlyIncomePrefix = "monthlyincome:";
    public static string MonthlyIncomeKey(string category, string clusterId) => $"{MonthlyIncomePrefix}{category}:{clusterId}";

    public const string PenaltyRulePrefix = "penaltyrule:";
    public static string PenaltyRuleKey(string ruleId) => $"{PenaltyRulePrefix}{ruleId}";
}
