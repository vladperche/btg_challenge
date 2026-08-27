using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Domains;
using Entities.Interfaces;
using Entities.Models;

namespace Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IJobTitleCategoryRepository _jobTitleCategoryRepository;
    private readonly ICustomerClusterRepository _customerClusterRepository;
    private readonly IMonthlyIncomeRepository _monthlyIncomeRepository;
    private readonly IPenaltyRuleRepository _penaltyRuleRepository;
    private readonly IMarketDebtTypeRepository _marketDebtTypeRepository;

    private static readonly HashSet<string> ValidRegions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Norte", "Nordeste", "Centro-Oeste", "Sudeste", "Sul"
    };

    public CustomerService(
        ICustomerRepository customerRepository,
        IJobTitleCategoryRepository jobTitleCategoryRepository,
        ICustomerClusterRepository customerClusterRepository,
        IMonthlyIncomeRepository monthlyIncomeRepository,
        IPenaltyRuleRepository penaltyRuleRepository,
        IMarketDebtTypeRepository marketDebtTypeRepository)
    {
        _customerRepository = customerRepository;
        _jobTitleCategoryRepository = jobTitleCategoryRepository;
        _customerClusterRepository = customerClusterRepository;
        _monthlyIncomeRepository = monthlyIncomeRepository;
        _penaltyRuleRepository = penaltyRuleRepository;
        _marketDebtTypeRepository = marketDebtTypeRepository;
    }

    public async Task<Customer> ProcessAndSaveAsync(CustomerClassificationRequest request)
    {
        if (request == null)
        {
            throw new DomainException("Customer classification request data cannot be null.");
        }

        // 1. Validate Input Fields
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new DomainException("Name is required and cannot be empty.");
        }

        if (request.Age < 0)
        {
            throw new DomainException("Age must be an integer greater than or equal to zero.");
        }

        // Score: optional, if null consider 0, range [0, 1000]
        int scoreValue = request.Score ?? 0;
        if (scoreValue < 0 || scoreValue > 1000)
        {
            throw new DomainException("Score must be an integer between 0 and 1000.");
        }

        // MarketDebtTypes handling: if null, default to empty array []
        var debtTypes = request.MarketDebtTypes ?? Array.Empty<string>();
        if (debtTypes.Length > 0)
        {
            foreach (var debtType in debtTypes)
            {
                if (string.IsNullOrWhiteSpace(debtType))
                {
                    throw new DomainException("MarketDebtTypes elements cannot be empty or null.");
                }

                var existingType = await _marketDebtTypeRepository.GetByValueAsync(debtType);
                if (existingType == null)
                {
                    throw new DomainException($"Market Debt Type '{debtType}' does not exist.");
                }
            }
        }

        // Location Validation
        if (request.Location == null)
        {
            throw new DomainException("Location information is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Location.City))
        {
            throw new DomainException("Location City is required and cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Location.State))
        {
            throw new DomainException("Location State is required and cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Location.Region) || !ValidRegions.Contains(request.Location.Region))
        {
            throw new DomainException("Location Region is required. Allowed values are: Norte, Nordeste, Centro-Oeste, Sudeste, Sul.");
        }

        if (string.IsNullOrWhiteSpace(request.JobTitle))
        {
            throw new DomainException("JobTitle is required and cannot be empty.");
        }

        // 2. Instantiate Customer Domain Object with generated Random UUID
        var customer = new Customer
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Age = request.Age,
            Score = scoreValue,
            HasMarketDebt = request.HasMarketDebt,
            MarketDebtTypes = debtTypes,
            Location = request.Location,
            JobTitle = request.JobTitle
        };

        // -------------------------------------------------------------
        // 3. ENRICHMENT PIPELINE (AFTER REQUEST, BEFORE RESPONSE)
        // -------------------------------------------------------------

        // 3.1 Job Category Enrichment
        var categories = (await _jobTitleCategoryRepository.GetAllAsync())
            .OrderBy(j => j.Priority)
            .ToList();

        var matchedCategory = categories.FirstOrDefault(j =>
            j.Keywords == null ||
            j.Keywords.Length == 0 ||
            j.Keywords.Any(kw => customer.JobTitle.Contains(kw, StringComparison.OrdinalIgnoreCase))
        );

        if (matchedCategory != null)
        {
            customer.JobCategory = matchedCategory.Category;
            customer.JobMultiplier = matchedCategory.Multiplier;
        }
        else
        {
            customer.JobCategory = string.Empty;
            customer.JobMultiplier = 1.0m;
        }

        // 3.2 Customer Cluster Enrichment
        var clusters = (await _customerClusterRepository.GetAllAsync())
            .OrderBy(c => c.Priority)
            .ToList();

        var matchedCluster = clusters.FirstOrDefault(c =>
            customer.Score.Value >= c.Score &&
            customer.Age >= c.AgeMin &&
            (customer.Age <= c.AgeMax || c.AgeMax == 0) &&
            (!c.DebtConditionMarketDebtCheck || !customer.HasMarketDebt) &&
            (c.DebtConditionMarketTypes == null ||
             c.DebtConditionMarketTypes.Length == 0 ||
             !customer.MarketDebtTypes.Any(userType => c.DebtConditionMarketTypes.Any(dt => string.Equals(dt, userType, StringComparison.OrdinalIgnoreCase))))
        );

        if (matchedCluster != null)
        {
            customer.CustomerCluster = matchedCluster.ClusterId;
            customer.ClusterName = matchedCluster.Name;
            customer.BaseLimit = matchedCluster.BaseLimit ?? 0m;
            customer.CapLimit = matchedCluster.CapLimit ?? 0m;
        }
        else
        {
            customer.CustomerCluster = string.Empty;
            customer.ClusterName = string.Empty;
            customer.BaseLimit = 0m;
            customer.CapLimit = 0m;
        }

        // 3.3 Monthly Income Enrichment
        if (!string.IsNullOrEmpty(customer.JobCategory) && !string.IsNullOrEmpty(customer.CustomerCluster))
        {
            var incomeRecord = await _monthlyIncomeRepository.GetByCompositeKeyAsync(customer.JobCategory, customer.CustomerCluster);
            customer.MonthlyIncome = incomeRecord != null ? incomeRecord.Income : 0m;
        }
        else
        {
            customer.MonthlyIncome = 0m;
        }

        // 3.4 Penalty Factor Enrichment
        var penaltyRules = (await _penaltyRuleRepository.GetAllAsync())
            .OrderBy(p => p.Priority)
            .ToList();

        var matchingRule = penaltyRules.FirstOrDefault(p =>
            p.Trigger != null &&
            p.Trigger.Length > 0 &&
            p.Trigger.Any(trig => customer.MarketDebtTypes.Any(userDebt => string.Equals(userDebt, trig, StringComparison.OrdinalIgnoreCase)))
        );

        if (matchingRule != null)
        {
            customer.PenaltyFactor = matchingRule.Effect ?? 1.0m;
        }
        else
        {
            customer.PenaltyFactor = 1.0m;
        }

        // 3.5 Approved Limit Enrichment
        // Formula: min(customer.base_limit * customer.job_multiplier * customer.penalty_factor, customer.cap_limit)
        decimal rawLimit = customer.BaseLimit * customer.JobMultiplier * customer.PenaltyFactor;
        decimal cappedLimit = customer.CapLimit > 0m ? Math.Min(rawLimit, customer.CapLimit) : rawLimit;

        // Round to nearest 100
        decimal roundedLimit = Math.Round(cappedLimit / 100m, MidpointRounding.AwayFromZero) * 100m;
        if (roundedLimit < 0m)
        {
            roundedLimit = 0m;
        }

        customer.ApprovedLimit = roundedLimit;

        // 4. Save Enriched Customer to Persistence & Return
        return await _customerRepository.SaveAsync(customer);
    }
}
