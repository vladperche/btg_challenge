using Crosscutting.Configuration;
using Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
using Repositories;
using Repositories.Context;
using Services;
using StackExchange.Redis;

namespace Crosscutting.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureAndServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Bind Settings
        var mongoSection = configuration.GetSection("ConnectionStrings:MongoDB");
        var mongoConnString = mongoSection.Value ?? configuration["ConnectionStrings:MongoDB"] ?? "mongodb://root:MySecureMongoPassword123!@localhost:27017/?authSource=admin";

        var redisSection = configuration.GetSection("ConnectionStrings:Redis");
        var redisConnString = redisSection.Value ?? configuration["ConnectionStrings:Redis"] ?? "localhost:6379,password=MySecureRedisPassword123!";

        // 2. Register MongoDB DbContext using MongoDB EF Core
        var mongoUrl = new MongoUrl(mongoConnString);
        var databaseName = string.IsNullOrEmpty(mongoUrl.DatabaseName) ? "btg_prototyping_db" : mongoUrl.DatabaseName;

        services.AddDbContext<MongoDbContext>(options =>
        {
            options.UseMongoDB(mongoConnString, databaseName);
        });

        // 3. Register Redis ConnectionMultiplexer
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnString));

        // 4. Register Services & Repositories
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IMarketDebtTypeRepository, MarketDebtTypeRepository>();
        services.AddScoped<IJobTitleCategoryRepository, JobTitleCategoryRepository>();
        services.AddScoped<ICustomerClusterRepository, CustomerClusterRepository>();
        services.AddScoped<IMonthlyIncomeRepository, MonthlyIncomeRepository>();
        services.AddScoped<IPenaltyRuleRepository, PenaltyRuleRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IMarketDebtTypeService, MarketDebtTypeService>();
        services.AddScoped<IJobTitleCategoryService, JobTitleCategoryService>();
        services.AddScoped<ICustomerClusterService, CustomerClusterService>();
        services.AddScoped<IMonthlyIncomeService, MonthlyIncomeService>();
        services.AddScoped<IPenaltyRuleService, PenaltyRuleService>();
        services.AddScoped<ICustomerService, CustomerService>();

        return services;
    }
}
