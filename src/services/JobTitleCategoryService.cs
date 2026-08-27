using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Domains;
using Entities.Interfaces;
using Entities.Models;

namespace Services;

public class JobTitleCategoryService : IJobTitleCategoryService
{
    private readonly IJobTitleCategoryRepository _repository;

    public JobTitleCategoryService(IJobTitleCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<JobTitleCategory>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<JobTitleCategory?> GetByCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new DomainException("Category identifier cannot be empty.");
        }

        return await _repository.GetByCategoryAsync(category);
    }

    public async Task<JobTitleCategory> SaveAsync(JobTitleCategory jobTitleCategory)
    {
        if (jobTitleCategory == null)
        {
            throw new DomainException("Job title category data cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(jobTitleCategory.Category))
        {
            throw new DomainException("Category identifier is required and cannot be empty.");
        }

        if (jobTitleCategory.Priority <= 0)
        {
            throw new DomainException("Priority must be a positive integer greater than zero.");
        }

        if (jobTitleCategory.Multiplier < 0)
        {
            throw new DomainException("Multiplier must be a decimal greater than or equal to zero.");
        }

        // Keywords handling: if null, default to empty array []
        if (jobTitleCategory.Keywords == null)
        {
            jobTitleCategory.Keywords = Array.Empty<string>();
        }
        else
        {
            // Validate that provided keywords are not null or empty/whitespace
            if (jobTitleCategory.Keywords.Any(k => string.IsNullOrWhiteSpace(k)))
            {
                throw new DomainException("Keywords elements must not be empty or null.");
            }
        }

        return await _repository.SaveAsync(jobTitleCategory);
    }

    public async Task<bool> DeleteAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new DomainException("Category identifier cannot be empty.");
        }

        return await _repository.DeleteAsync(category);
    }
}
