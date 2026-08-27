using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Domains;
using Entities.Interfaces;
using Entities.Models;

namespace Services;

public class MonthlyIncomeService : IMonthlyIncomeService
{
    private readonly IMonthlyIncomeRepository _incomeRepository;
    private readonly IJobTitleCategoryRepository _jobTitleCategoryRepository;
    private readonly ICustomerClusterRepository _customerClusterRepository;

    public MonthlyIncomeService(
        IMonthlyIncomeRepository incomeRepository,
        IJobTitleCategoryRepository jobTitleCategoryRepository,
        ICustomerClusterRepository customerClusterRepository)
    {
        _incomeRepository = incomeRepository;
        _jobTitleCategoryRepository = jobTitleCategoryRepository;
        _customerClusterRepository = customerClusterRepository;
    }

    public async Task<IEnumerable<MonthlyIncome>> GetAllAsync()
    {
        return await _incomeRepository.GetAllAsync();
    }

    public async Task<IEnumerable<MonthlyIncome>> GetByCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new DomainException("Category identifier cannot be empty.");
        }

        return await _incomeRepository.GetByCategoryAsync(category);
    }

    public async Task<MonthlyIncome?> GetByCompositeKeyAsync(string category, string clusterId)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new DomainException("Category identifier cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(clusterId))
        {
            throw new DomainException("Cluster ID identifier cannot be empty.");
        }

        return await _incomeRepository.GetByCompositeKeyAsync(category, clusterId);
    }

    public async Task<MonthlyIncome> SaveAsync(MonthlyIncome monthlyIncome)
    {
        if (monthlyIncome == null)
        {
            throw new DomainException("Monthly income data cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(monthlyIncome.Category))
        {
            throw new DomainException("Category identifier is required and cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(monthlyIncome.ClusterId))
        {
            throw new DomainException("Cluster ID identifier is required and cannot be empty.");
        }

        if (monthlyIncome.Income < 0)
        {
            throw new DomainException("Income must be a decimal greater than or equal to zero.");
        }

        // Cross-Entity Validation 1: Check existence in Job Title Category
        var existingCategory = await _jobTitleCategoryRepository.GetByCategoryAsync(monthlyIncome.Category);
        if (existingCategory == null)
        {
            throw new DomainException($"Job Title Category '{monthlyIncome.Category}' does not exist.");
        }

        // Cross-Entity Validation 2: Check existence in Customer Cluster
        var existingCluster = await _customerClusterRepository.GetByClusterIdAsync(monthlyIncome.ClusterId);
        if (existingCluster == null)
        {
            throw new DomainException($"Customer Cluster '{monthlyIncome.ClusterId}' does not exist.");
        }

        return await _incomeRepository.SaveAsync(monthlyIncome);
    }

    public async Task<bool> DeleteAsync(string category, string clusterId)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new DomainException("Category identifier cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(clusterId))
        {
            throw new DomainException("Cluster ID identifier cannot be empty.");
        }

        return await _incomeRepository.DeleteAsync(category, clusterId);
    }
}
