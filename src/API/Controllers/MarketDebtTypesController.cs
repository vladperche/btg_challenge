using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Interfaces;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/market-debt-types")]
public class MarketDebtTypesController : ControllerBase
{
    private readonly IMarketDebtTypeService _marketDebtTypeService;

    public MarketDebtTypesController(IMarketDebtTypeService marketDebtTypeService)
    {
        _marketDebtTypeService = marketDebtTypeService;
    }

    /// <summary>
    /// Return a list of all Market Debt Types.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MarketDebtType>>> GetAll()
    {
        var records = await _marketDebtTypeService.GetAllAsync();
        return Ok(records);
    }

    /// <summary>
    /// Return a specific Market Debt Type by value identifier.
    /// </summary>
    [HttpGet("{value}")]
    public async Task<ActionResult<MarketDebtType>> GetByValue(string value)
    {
        var record = await _marketDebtTypeService.GetByValueAsync(value);
        if (record == null)
        {
            return NotFound(new { message = $"Market Debt Type with value '{value}' was not found." });
        }

        return Ok(record);
    }

    /// <summary>
    /// Insert or update a Market Debt Type record on persistence (Upsert).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MarketDebtType>> Save([FromBody] MarketDebtType marketDebtType)
    {
        var saved = await _marketDebtTypeService.SaveAsync(marketDebtType);
        return Ok(saved);
    }

    /// <summary>
    /// Delete a specific Market Debt Type record by value, if it exists.
    /// </summary>
    [HttpDelete("{value}")]
    public async Task<IActionResult> Delete(string value)
    {
        var deleted = await _marketDebtTypeService.DeleteAsync(value);
        if (!deleted)
        {
            return NotFound(new { message = $"Market Debt Type with value '{value}' was not found." });
        }

        return NoContent();
    }
}
