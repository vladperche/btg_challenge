using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Interfaces;
using Entities.Models;
using Moq;
using Services;
using Xunit;

namespace UnitTests;

public class MonthlyIncomeLookupTests
{
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<IJobTitleCategoryRepository> _jobCatRepoMock = new();
    private readonly Mock<ICustomerClusterRepository> _clusterRepoMock = new();
    private readonly Mock<IMonthlyIncomeRepository> _incomeRepoMock = new();
    private readonly Mock<IPenaltyRuleRepository> _penaltyRepoMock = new();
    private readonly Mock<IMarketDebtTypeRepository> _marketDebtTypeRepoMock = new();

    public MonthlyIncomeLookupTests()
    {
        _customerRepoMock.Setup(r => r.SaveAsync(It.IsAny<Customer>())).ReturnsAsync((Customer c) => c);
        _penaltyRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PenaltyRule>());
    }

    [Theory]
    [InlineData("EXECUTIVE", "CLUSTER_A", 25000, 25000)]
    [InlineData("EXECUTIVE", "CLUSTER_B", 15000, 15000)]
    [InlineData("MID_PROFESSIONAL", "CLUSTER_A", 12000, 12000)]
    [InlineData("MID_PROFESSIONAL", "CLUSTER_B", 8000, 8000)]
    public async Task MonthlyIncomeLookup_AllCombinations_ReturnsCorrectIncome(
        string category,
        string clusterId,
        decimal dbIncome,
        decimal expectedMonthlyIncome)
    {
        // Arrange
        var jobCat = new JobTitleCategory { Category = category, Priority = 1, Multiplier = 1.0m, Keywords = new[] { "Employee" } };
        var cluster = new CustomerCluster { ClusterId = clusterId, Priority = 1, Score = 0, AgeMin = 0, AgeMax = 99, BaseLimit = 1000, CapLimit = 10000 };

        _jobCatRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<JobTitleCategory> { jobCat });
        _clusterRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CustomerCluster> { cluster });
        _incomeRepoMock.Setup(r => r.GetByCompositeKeyAsync(category, clusterId))
            .ReturnsAsync(new MonthlyIncome { Category = category, ClusterId = clusterId, Income = dbIncome });

        var service = new CustomerService(
            _customerRepoMock.Object,
            _jobCatRepoMock.Object,
            _clusterRepoMock.Object,
            _incomeRepoMock.Object,
            _penaltyRepoMock.Object,
            _marketDebtTypeRepoMock.Object);

        var request = new CustomerClassificationRequest
        {
            Name = "Income Test User",
            Age = 30,
            JobTitle = "Employee",
            Location = new LocationInfo { City = "Porto Alegre", State = "RS", Region = "Sul" }
        };

        // Act
        var result = await service.ProcessAndSaveAsync(request);

        // Assert
        Assert.Equal(expectedMonthlyIncome, result.MonthlyIncome);
    }

    [Fact]
    public async Task MonthlyIncomeLookup_MissingCombination_DefaultsToZero()
    {
        // Arrange: Income record is not found in database for combination
        var jobCat = new JobTitleCategory { Category = "UNKNOWN_CAT", Priority = 1, Multiplier = 1.0m, Keywords = new[] { "Worker" } };
        var cluster = new CustomerCluster { ClusterId = "UNKNOWN_CLUSTER", Priority = 1, Score = 0, AgeMin = 0, AgeMax = 99, BaseLimit = 1000, CapLimit = 10000 };

        _jobCatRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<JobTitleCategory> { jobCat });
        _clusterRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CustomerCluster> { cluster });
        _incomeRepoMock.Setup(r => r.GetByCompositeKeyAsync("UNKNOWN_CAT", "UNKNOWN_CLUSTER"))
            .ReturnsAsync((MonthlyIncome?)null);

        var service = new CustomerService(
            _customerRepoMock.Object,
            _jobCatRepoMock.Object,
            _clusterRepoMock.Object,
            _incomeRepoMock.Object,
            _penaltyRepoMock.Object,
            _marketDebtTypeRepoMock.Object);

        var request = new CustomerClassificationRequest
        {
            Name = "Default Income User",
            Age = 30,
            JobTitle = "Worker",
            Location = new LocationInfo { City = "Recife", State = "PE", Region = "Nordeste" }
        };

        // Act
        var result = await service.ProcessAndSaveAsync(request);

        // Assert
        Assert.Equal(0m, result.MonthlyIncome);
    }
}
