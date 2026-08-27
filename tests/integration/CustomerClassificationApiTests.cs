using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Entities.Constants;
using Entities.Interfaces;
using Entities.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace IntegrationTests;

public class CustomerClassificationApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<IJobTitleCategoryRepository> _jobCatRepoMock = new();
    private readonly Mock<ICustomerClusterRepository> _clusterRepoMock = new();
    private readonly Mock<IMonthlyIncomeRepository> _incomeRepoMock = new();
    private readonly Mock<IPenaltyRuleRepository> _penaltyRepoMock = new();
    private readonly Mock<IMarketDebtTypeRepository> _marketDebtTypeRepoMock = new();

    public CustomerClassificationApiTests(WebApplicationFactory<Program> factory)
    {
        _customerRepoMock.Setup(r => r.SaveAsync(It.IsAny<Customer>())).ReturnsAsync((Customer c) => c);

        _jobCatRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<JobTitleCategory>
        {
            new JobTitleCategory { Category = "SENIOR_PROFESSIONAL", Priority = 1, Multiplier = 1.5m, Keywords = new[] { "Senior", "Lead" } }
        });

        _clusterRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CustomerCluster>
        {
            new CustomerCluster { ClusterId = "CLUSTER_DIAMOND", Priority = 1, Name = "Diamond", Score = 700, AgeMin = 18, AgeMax = 65, BaseLimit = 50000, CapLimit = 100000, DebtConditionMarketDebtCheck = true }
        });

        _incomeRepoMock.Setup(r => r.GetByCompositeKeyAsync("SENIOR_PROFESSIONAL", "CLUSTER_DIAMOND"))
            .ReturnsAsync(new MonthlyIncome { Category = "SENIOR_PROFESSIONAL", ClusterId = "CLUSTER_DIAMOND", Income = 18500m });

        _penaltyRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PenaltyRule>());

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => _customerRepoMock.Object);
                services.AddScoped(_ => _jobCatRepoMock.Object);
                services.AddScoped(_ => _clusterRepoMock.Object);
                services.AddScoped(_ => _incomeRepoMock.Object);
                services.AddScoped(_ => _penaltyRepoMock.Object);
                services.AddScoped(_ => _marketDebtTypeRepoMock.Object);
            });
        });
    }

    [Fact]
    public async Task PostClassify_ValidInput_ReturnsHttp200AndCorrectOutputContract()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppConstants.ApiKeyHeaderName, "BTG_PROTOTYPING_SECRET_KEY_12345");

        var payload = new
        {
            name = "Alex Smith",
            age = 30,
            score = 750,
            has_market_debt = false,
            market_debt_types = new string[] { },
            location = new
            {
                city = "Sao Paulo",
                state = "SP",
                region = "Sudeste"
            },
            job_title = "Senior Software Engineer"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/customers/classify", payload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("id", out var idProp) && !string.IsNullOrEmpty(idProp.GetString()));
        Assert.Equal("Alex Smith", body.GetProperty("name").GetString());
        Assert.Equal(30, body.GetProperty("age").GetInt32());
        Assert.Equal(750, body.GetProperty("score").GetInt32());
        Assert.Equal("SENIOR_PROFESSIONAL", body.GetProperty("job_category").GetString());
        Assert.Equal(1.5m, body.GetProperty("job_multiplier").GetDecimal());
        Assert.Equal("CLUSTER_DIAMOND", body.GetProperty("customer_cluster").GetString());
        Assert.Equal("Diamond", body.GetProperty("cluster_name").GetString());
        Assert.Equal(50000m, body.GetProperty("base_limit").GetDecimal());
        Assert.Equal(100000m, body.GetProperty("cap_limit").GetDecimal());
        Assert.Equal(18500m, body.GetProperty("monthly_income").GetDecimal());
        Assert.Equal(1.0m, body.GetProperty("penalty_factor").GetDecimal());
        Assert.Equal(75000m, body.GetProperty("approved_limit").GetDecimal());
    }

    [Theory]
    [InlineData("", 30, 700, "Name is required")]
    [InlineData("John", -1, 700, "Age must be an integer")]
    [InlineData("John", 30, 1500, "Score must be an integer between 0 and 1000")]
    public async Task PostClassify_InvalidInputFields_ReturnsHttp400BadRequest(
        string name, int age, int score, string expectedErrorSubstring)
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppConstants.ApiKeyHeaderName, "BTG_PROTOTYPING_SECRET_KEY_12345");

        var payload = new
        {
            name = name,
            age = age,
            score = score,
            has_market_debt = false,
            market_debt_types = new string[] { },
            location = new
            {
                city = "Sao Paulo",
                state = "SP",
                region = "Sudeste"
            },
            job_title = "Software Developer"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/customers/classify", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains(expectedErrorSubstring, responseContent, System.StringComparison.OrdinalIgnoreCase);
    }
}
