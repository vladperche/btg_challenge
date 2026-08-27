using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Interfaces;
using Entities.Models;
using Moq;
using Services;
using Xunit;

namespace UnitTests;

public class ClusterAssignmentTests
{
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<IJobTitleCategoryRepository> _jobCatRepoMock = new();
    private readonly Mock<ICustomerClusterRepository> _clusterRepoMock = new();
    private readonly Mock<IMonthlyIncomeRepository> _incomeRepoMock = new();
    private readonly Mock<IPenaltyRuleRepository> _penaltyRepoMock = new();
    private readonly Mock<IMarketDebtTypeRepository> _marketDebtTypeRepoMock = new();

    public ClusterAssignmentTests()
    {
        _customerRepoMock.Setup(r => r.SaveAsync(It.IsAny<Customer>())).ReturnsAsync((Customer c) => c);
        _jobCatRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<JobTitleCategory>());
        _incomeRepoMock.Setup(r => r.GetByCompositeKeyAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((MonthlyIncome?)null);
        _penaltyRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PenaltyRule>());
    }

    [Theory]
    [InlineData(700, "CLUSTER_HIGH")]
    [InlineData(699, "CLUSTER_LOW")]
    public async Task CustomerCluster_ScoreBoundaryCondition_AssignsCorrectCluster(int score, string expectedCluster)
    {
        // Arrange
        var clusters = new List<CustomerCluster>
        {
            new CustomerCluster
            {
                ClusterId = "CLUSTER_HIGH",
                Priority = 1,
                Name = "High Tier",
                Score = 700,
                AgeMin = 18,
                AgeMax = 60,
                BaseLimit = 10000,
                CapLimit = 50000
            },
            new CustomerCluster
            {
                ClusterId = "CLUSTER_LOW",
                Priority = 2,
                Name = "Low Tier",
                Score = 0,
                AgeMin = 18,
                AgeMax = 60,
                BaseLimit = 2000,
                CapLimit = 10000
            }
        };

        _clusterRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(clusters);

        var service = new CustomerService(
            _customerRepoMock.Object,
            _jobCatRepoMock.Object,
            _clusterRepoMock.Object,
            _incomeRepoMock.Object,
            _penaltyRepoMock.Object,
            _marketDebtTypeRepoMock.Object);

        var request = new CustomerClassificationRequest
        {
            Name = "John Doe",
            Age = 30,
            Score = score,
            HasMarketDebt = false,
            JobTitle = "Employee",
            Location = new LocationInfo { City = "SP", State = "SP", Region = "Sudeste" }
        };

        // Act
        var result = await service.ProcessAndSaveAsync(request);

        // Assert
        Assert.Equal(expectedCluster, result.CustomerCluster);
    }

    [Theory]
    [InlineData(18, "CLUSTER_AGE_MATCH")]
    [InlineData(60, "CLUSTER_AGE_MATCH")]
    [InlineData(61, "CLUSTER_UNLIMITED_AGE")]
    public async Task CustomerCluster_AgeBoundaryConditions_AssignsCorrectCluster(int age, string expectedCluster)
    {
        // Arrange
        var clusters = new List<CustomerCluster>
        {
            new CustomerCluster
            {
                ClusterId = "CLUSTER_AGE_MATCH",
                Priority = 1,
                Name = "Bounded Age Tier",
                Score = 100,
                AgeMin = 18,
                AgeMax = 60,
                BaseLimit = 5000,
                CapLimit = 20000
            },
            new CustomerCluster
            {
                ClusterId = "CLUSTER_UNLIMITED_AGE",
                Priority = 2,
                Name = "Unlimited Age Tier",
                Score = 100,
                AgeMin = 18,
                AgeMax = 0, // 0 = unlimited
                BaseLimit = 3000,
                CapLimit = 15000
            }
        };

        _clusterRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(clusters);

        var service = new CustomerService(
            _customerRepoMock.Object,
            _jobCatRepoMock.Object,
            _clusterRepoMock.Object,
            _incomeRepoMock.Object,
            _penaltyRepoMock.Object,
            _marketDebtTypeRepoMock.Object);

        var request = new CustomerClassificationRequest
        {
            Name = "Test User",
            Age = age,
            Score = 500,
            HasMarketDebt = false,
            JobTitle = "Worker",
            Location = new LocationInfo { City = "Rio", State = "RJ", Region = "Sudeste" }
        };

        // Act
        var result = await service.ProcessAndSaveAsync(request);

        // Assert
        Assert.Equal(expectedCluster, result.CustomerCluster);
    }

    [Fact]
    [Trait("Category", "ClusterD_Denial")]
    public async Task CustomerCluster_ClusterDDenial_ReturnsApprovedLimitZero()
    {
        // Arrange: CLUSTER_D represents a denial cluster with BaseLimit = 0 and CapLimit = 0
        var clusters = new List<CustomerCluster>
        {
            new CustomerCluster
            {
                ClusterId = "CLUSTER_D",
                Priority = 1,
                Name = "Denied Cluster D",
                Score = 0,
                AgeMin = 0,
                AgeMax = 99,
                BaseLimit = 0,
                CapLimit = 0,
                DebtConditionMarketDebtCheck = true
            }
        };

        _clusterRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(clusters);

        var service = new CustomerService(
            _customerRepoMock.Object,
            _jobCatRepoMock.Object,
            _clusterRepoMock.Object,
            _incomeRepoMock.Object,
            _penaltyRepoMock.Object,
            _marketDebtTypeRepoMock.Object);

        var request = new CustomerClassificationRequest
        {
            Name = "Denied Customer",
            Age = 25,
            Score = 100,
            HasMarketDebt = false,
            JobTitle = "Worker",
            Location = new LocationInfo { City = "Curitiba", State = "PR", Region = "Sul" }
        };

        // Act
        var result = await service.ProcessAndSaveAsync(request);

        // Assert
        Assert.Equal("CLUSTER_D", result.CustomerCluster);
        Assert.Equal(0m, result.BaseLimit);
        Assert.Equal(0m, result.CapLimit);
        Assert.Equal(0m, result.ApprovedLimit);
    }
}
