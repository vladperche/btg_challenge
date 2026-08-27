using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Interfaces;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/job-title-categories")]
public class JobTitleCategoriesController : ControllerBase
{
    private readonly IJobTitleCategoryService _jobTitleCategoryService;

    public JobTitleCategoriesController(IJobTitleCategoryService jobTitleCategoryService)
    {
        _jobTitleCategoryService = jobTitleCategoryService;
    }

    /// <summary>
    /// Return a list of all Job Title Categories.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobTitleCategory>>> GetAll()
    {
        var records = await _jobTitleCategoryService.GetAllAsync();
        return Ok(records);
    }

    /// <summary>
    /// Return a specific Job Title Category by category key.
    /// </summary>
    [HttpGet("{category}")]
    public async Task<ActionResult<JobTitleCategory>> GetByCategory(string category)
    {
        var record = await _jobTitleCategoryService.GetByCategoryAsync(category);
        if (record == null)
        {
            return NotFound(new { message = $"Job Title Category with key '{category}' was not found." });
        }

        return Ok(record);
    }

    /// <summary>
    /// Insert or update a Job Title Category record on persistence (Upsert).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<JobTitleCategory>> Save([FromBody] JobTitleCategory jobTitleCategory)
    {
        var saved = await _jobTitleCategoryService.SaveAsync(jobTitleCategory);
        return Ok(saved);
    }

    /// <summary>
    /// Delete a specific Job Title Category record by category key, if it exists.
    /// </summary>
    [HttpDelete("{category}")]
    public async Task<IActionResult> Delete(string category)
    {
        var deleted = await _jobTitleCategoryService.DeleteAsync(category);
        if (!deleted)
        {
            return NotFound(new { message = $"Job Title Category with key '{category}' was not found." });
        }

        return NoContent();
    }
}
