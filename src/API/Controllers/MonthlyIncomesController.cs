using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Interfaces;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/monthly-incomes")]
public class MonthlyIncomesController : ControllerBase
{
    private readonly IMonthlyIncomeService _monthlyIncomeService;

    public MonthlyIncomesController(IMonthlyIncomeService monthlyIncomeService)
    {
        _monthlyIncomeService = monthlyIncomeService;
    }

    /// <summary>
    /// Return a list of all Monthly Incomes.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MonthlyIncome>>> GetAll()
    {
        var records = await _monthlyIncomeService.GetAllAsync();
        return Ok(records);
    }

    /// <summary>
    /// Return a list of Monthly Incomes corresponding to a specific category.
    /// </summary>
    [HttpGet("{category}")]
    public async Task<ActionResult<IEnumerable<MonthlyIncome>>> GetByCategory(string category)
    {
        var records = await _monthlyIncomeService.GetByCategoryAsync(category);
        return Ok(records);
    }

    /// <summary>
    /// Insert or update a Monthly Income record on persistence (Upsert by Category + ClusterId pair).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MonthlyIncome>> Save([FromBody] MonthlyIncome monthlyIncome)
    {
        var saved = await _monthlyIncomeService.SaveAsync(monthlyIncome);
        return Ok(saved);
    }

    /// <summary>
    /// Delete a specific Monthly Income record by composite key pair {category}/{cluster}, if it exists.
    /// </summary>
    [HttpDelete("{category}/{cluster}")]
    public async Task<IActionResult> Delete(string category, string cluster)
    {
        var deleted = await _monthlyIncomeService.DeleteAsync(category, cluster);
        if (!deleted)
        {
            return NotFound(new { message = $"Monthly Income record for Category '{category}' and Cluster '{cluster}' was not found." });
        }

        return NoContent();
    }
}
