using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Interfaces;
using Entities.Models;
using Moq;
using Services;
using Xunit;

namespace UnitTests;

public class JobCategoryMatchingTests
{
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<IJobTitleCategoryRepository> _jobCatRepoMock = new();
    private readonly Mock<ICustomerClusterRepository> _clusterRepoMock = new();
    private readonly Mock<IMonthlyIncomeRepository> _incomeRepoMock = new();
    private readonly Mock<IPenaltyRuleRepository> _penaltyRepoMock = new();
    private readonly Mock<IMarketDebtTypeRepository> _marketDebtTypeRepoMock = new();

    public JobCategoryMatchingTests()
    {
        _customerRepoMock.Setup(r => r.SaveAsync(It.IsAny<Customer>())).ReturnsAsync((Customer c) => c);
        _clusterRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CustomerCluster>());
        _incomeRepoMock.Setup(r => r.GetByCompositeKeyAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((MonthlyIncome?)null);
        _penaltyRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PenaltyRule>());
    }

    [Theory]
    [InlineData("SENIOR SOFTWARE ENGINEER", "EXECUTIVE", 2.0)]
    [InlineData("junior developer", "TECHNICAL", 1.2)]
    public async Task JobCategory_CaseInsensitiveKeywordMatching_AssignsCategory(string inputJobTitle, string expectedCategory, decimal expectedMultiplier)
    {
        // Arrange
        var categories = new List<JobTitleCategory>
        {
            new JobTitleCategory
            {
                Category = "EXECUTIVE",
                Priority = 1,
                Multiplier = 2.0m,
                Keywords = new[] { "Senior", "Director", "Executive" }
            },
            new JobTitleCategory
            {
                Category = "TECHNICAL",
                Priority = 2,
                Multiplier = 1.2m,
                Keywords = new[] { "Developer", "Engineer" }
            }
        };

        _jobCatRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var service = new CustomerService(
            _customerRepoMock.Object,
            _jobCatRepoMock.Object,
            _clusterRepoMock.Object,
            _incomeRepoMock.Object,
            _penaltyRepoMock.Object,
            _marketDebtTypeRepoMock.Object);

        var request = new CustomerClassificationRequest
        {
            Name = "Alice",
            Age = 28,
            JobTitle = inputJobTitle,
            Location = new LocationInfo { City = "Salvador", State = "BA", Region = "Nordeste" }
        };

        // Act
        var result = await service.ProcessAndSaveAsync(request);

        // Assert
        Assert.Equal(expectedCategory, result.JobCategory);
        Assert.Equal(expectedMultiplier, result.JobMultiplier);
    }

    [Fact]
    [Trait("Category", "PriorityOrdering")]
    public async Task JobCategory_PriorityOrdering_PicksLowerPriorityNumberFirst()
    {
        // Arrange: "Senior Engineer" contains keywords for both Priority 1 and Priority 2
        var categories = new List<JobTitleCategory>
        {
            new JobTitleCategory
            {
                Category = "PRIORITY_1_CAT",
                Priority = 1,
                Multiplier = 2.5m,
                Keywords = new[] { "Senior" }
            },
            new JobTitleCategory
            {
                Category = "PRIORITY_2_CAT",
                Priority = 2,
                Multiplier = 1.5m,
                Keywords = new[] { "Engineer" }
            }
        };

        _jobCatRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var service = new CustomerService(
            _customerRepoMock.Object,
            _jobCatRepoMock.Object,
            _clusterRepoMock.Object,
            _incomeRepoMock.Object,
            _penaltyRepoMock.Object,
            _marketDebtTypeRepoMock.Object);

        var request = new CustomerClassificationRequest
        {
            Name = "Bob",
            Age = 35,
            JobTitle = "Senior Engineer",
            Location = new LocationInfo { City = "Manaus", State = "AM", Region = "Norte" }
        };

        // Act
        var result = await service.ProcessAndSaveAsync(request);

        // Assert
        Assert.Equal("PRIORITY_1_CAT", result.JobCategory);
        Assert.Equal(2.5m, result.JobMultiplier);
    }

    [Fact]
    public async Task JobCategory_EmptyKeywordsArray_MatchesAnyJobTitle()
    {
        // Arrange: Category with empty keywords [] should match any job title as a wildcard
        var categories = new List<JobTitleCategory>
        {
            new JobTitleCategory
            {
                Category = "WILDCARD_OTHER",
                Priority = 10,
                Multiplier = 0.8m,
                Keywords = System.Array.Empty<string>()
            }
        };

        _jobCatRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var service = new CustomerService(
            _customerRepoMock.Object,
            _jobCatRepoMock.Object,
            _clusterRepoMock.Object,
            _incomeRepoMock.Object,
            _penaltyRepoMock.Object,
            _marketDebtTypeRepoMock.Object);

        var request = new CustomerClassificationRequest
        {
            Name = "Charlie",
            Age = 40,
            JobTitle = "Unrecognized Custom Job Title",
            Location = new LocationInfo { City = "Brasília", State = "DF", Region = "Centro-Oeste" }
        };

        // Act
        var result = await service.ProcessAndSaveAsync(request);

        // Assert
        Assert.Equal("WILDCARD_OTHER", result.JobCategory);
        Assert.Equal(0.8m, result.JobMultiplier);
    }
}
