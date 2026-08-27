using Entities.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Repositories.Context;

public class MongoDbContext : DbContext
{
    public DbSet<MarketDebtType> MarketDebtTypes { get; set; } = null!;
    public DbSet<JobTitleCategory> JobTitleCategories { get; set; } = null!;
    public DbSet<CustomerCluster> CustomerClusters { get; set; } = null!;
    public DbSet<MonthlyIncome> MonthlyIncomes { get; set; } = null!;
    public DbSet<PenaltyRule> PenaltyRules { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;

    public MongoDbContext(DbContextOptions<MongoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MarketDebtType>(entity =>
        {
            entity.HasKey(m => m.Value);
            entity.Property(m => m.Value).HasElementName("_id");
            entity.ToCollection("market_debt_types");
        });

        modelBuilder.Entity<JobTitleCategory>(entity =>
        {
            entity.HasKey(j => j.Category);
            entity.Property(j => j.Category).HasElementName("_id");
            entity.ToCollection("job_title_categories");
        });

        modelBuilder.Entity<CustomerCluster>(entity =>
        {
            entity.HasKey(c => c.ClusterId);
            entity.Property(c => c.ClusterId).HasElementName("_id");
            entity.ToCollection("customer_clusters");
        });

        modelBuilder.Entity<MonthlyIncome>(entity =>
        {
            entity.HasKey(m => new { m.Category, m.ClusterId });
            entity.ToCollection("monthly_incomes");
        });

        modelBuilder.Entity<PenaltyRule>(entity =>
        {
            entity.HasKey(p => p.RuleId);
            entity.Property(p => p.RuleId).HasElementName("_id");
            entity.ToCollection("penalty_rules");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasElementName("_id");
            entity.OwnsOne(c => c.Location);
            entity.ToCollection("customers");
        });
    }
}
