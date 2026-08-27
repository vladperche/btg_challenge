using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Interfaces;
using Entities.Models;
using Moq;
using Services;
using Xunit;

namespace UnitTests;

public class CreditLimitCalculationTests
{
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<IJobTitleCategoryRepository> _jobCatRepoMock = new();
    private readonly Mock<ICustomerClusterRepository> _clusterRepoMock = new();
    private readonly Mock<IMonthlyIncomeRepository> _incomeRepoMock = new();
    private readonly Mock<IPenaltyRuleRepository> _penaltyRepoMock = new();
    private readonly Mock<IMarketDebtTypeRepository> _marketDebtTypeRepoMock = new();

    public CreditLimitCalculationTests()
    {
        _customerRepoMock.Setup(r => r.SaveAsync(It.IsAny<Customer>())).ReturnsAsync((Customer c) => c);
        _incomeRepoMock.Setup(r => r.GetByCompositeKeyAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((MonthlyIncome?)null);
        _marketDebtTypeRepoMock.Setup(r => r.GetByValueAsync(It.IsAny<string>())).ReturnsAsync(new MarketDebtType { Value = "credit_card", Meaning = "Credit Card" });
    }

    [Theory]
    [InlineData(10000, 1.5, 1.0, 100000, 15000)]
    [InlineData(10000, 1.5, 0.5, 100000, 7500)]
    [InlineData(50000, 2.0, 1.0, 40000, 40000)]
    public async Task CreditLimit_CalculationAndCap_ReturnsExpectedApprovedLimit(
        decimal baseLimit,
        decimal multiplier,
        decimal penaltyEffect,
        decimal capLimit,
        decimal expectedApprovedLimit)
    {
        // Arrange
        var category = new JobTitleCategory { Category = "TEST_CAT", Priority = 1, Multiplier = multiplier, Keywords = new[] { "Developer" } };
        var cluster = new CustomerCluster { ClusterId = "TEST_CLUSTER", Priority = 1, Score = 0, AgeMin = 0, AgeMax = 99, BaseLimit = baseLimit, CapLimit = capLimit };
        
        _jobCatRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<JobTitleCategory> { category });
        _clusterRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CustomerCluster> { cluster });

        if (penaltyEffect < 1.0m)
        {
            var rule = new PenaltyRule { RuleId = "PENALTY_01", Priority = 1, Effect = penaltyEffect, Trigger = new[] { "credit_card" } };
            _penaltyRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PenaltyRule> { rule });
        }
        else
        {
            _penaltyRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PenaltyRule>());
        }

        var service = new CustomerService(
            _customerRepoMock.Object,
            _jobCatRepoMock.Object,
            _clusterRepoMock.Object,
            _incomeRepoMock.Object,
            _penaltyRepoMock.Object,
            _marketDebtTypeRepoMock.Object);

        var request = new CustomerClassificationRequest
        {
            Name = "Limit Test Customer",
            Age = 30,
            Score = 500,
            HasMarketDebt = penaltyEffect < 1.0m,
            MarketDebtTypes = penaltyEffect < 1.0m ? new[] { "credit_card" } : System.Array.Empty<string>(),
            JobTitle = "Developer",
            Location = new LocationInfo { City = "SP", State = "SP", Region = "Sudeste" }
        };

        // Act
        var result = await service.ProcessAndSaveAsync(request);

        // Assert
        Assert.Equal(expectedApprovedLimit, result.ApprovedLimit);
    }

    [Theory]
    [InlineData(5000, 1.0, 0.75, 100000, 3800)]
    [InlineData(15000, 1.0, 0.75, 100000, 11300)]
    [InlineData(1234, 1.0, 1.0, 100000, 1200)]
    [InlineData(1250, 1.0, 1.0, 100000, 1300)]
    public async Task CreditLimit_RoundToNearest100_RoundsCorrectly(
        decimal baseLimit,
        decimal multiplier,
        decimal penaltyEffect,
        decimal capLimit,
        decimal expectedApprovedLimit)
    {
        // Arrange
        var category = new JobTitleCategory { Category = "TEST_CAT", Priority = 1, Multiplier = multiplier, Keywords = new[] { "Worker" } };
        var cluster = new CustomerCluster { ClusterId = "TEST_CLUSTER", Priority = 1, Score = 0, AgeMin = 0, AgeMax = 99, BaseLimit = baseLimit, CapLimit = capLimit };
        
        _jobCatRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<JobTitleCategory> { category });
        _clusterRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CustomerCluster> { cluster });

        if (penaltyEffect < 1.0m)
        {
            var rule = new PenaltyRule { RuleId = "PENALTY_01", Priority = 1, Effect = penaltyEffect, Trigger = new[] { "credit_card" } };
            _penaltyRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PenaltyRule> { rule });
        }
        else
        {
            _penaltyRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PenaltyRule>());
        }

        var service = new CustomerService(
            _customerRepoMock.Object,
            _jobCatRepoMock.Object,
            _clusterRepoMock.Object,
            _incomeRepoMock.Object,
            _penaltyRepoMock.Object,
            _marketDebtTypeRepoMock.Object);

        var request = new CustomerClassificationRequest
        {
            Name = "Rounding Test",
            Age = 25,
            JobTitle = "Worker",
            HasMarketDebt = penaltyEffect < 1.0m,
            MarketDebtTypes = penaltyEffect < 1.0m ? new[] { "credit_card" } : System.Array.Empty<string>(),
            Location = new LocationInfo { City = "SP", State = "SP", Region = "Sudeste" }
        };

        // Act
        var result = await service.ProcessAndSaveAsync(request);

        // Assert
        Assert.Equal(expectedApprovedLimit, result.ApprovedLimit);
    }
}
