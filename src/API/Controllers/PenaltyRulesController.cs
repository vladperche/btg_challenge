using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Interfaces;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/penalty-rules")]
public class PenaltyRulesController : ControllerBase
{
    private readonly IPenaltyRuleService _penaltyRuleService;

    public PenaltyRulesController(IPenaltyRuleService penaltyRuleService)
    {
        _penaltyRuleService = penaltyRuleService;
    }

    /// <summary>
    /// Return a list of all Penalty Rules.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PenaltyRule>>> GetAll()
    {
        var records = await _penaltyRuleService.GetAllAsync();
        return Ok(records);
    }

    /// <summary>
    /// Return a specific Penalty Rule by rule ID key.
    /// </summary>
    [HttpGet("{ruleId}")]
    public async Task<ActionResult<PenaltyRule>> GetByRuleId(string ruleId)
    {
        var record = await _penaltyRuleService.GetByRuleIdAsync(ruleId);
        if (record == null)
        {
            return NotFound(new { message = $"Penalty Rule with key '{ruleId}' was not found." });
        }

        return Ok(record);
    }

    /// <summary>
    /// Insert or update a Penalty Rule record on persistence (Upsert).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PenaltyRule>> Save([FromBody] PenaltyRule penaltyRule)
    {
        var saved = await _penaltyRuleService.SaveAsync(penaltyRule);
        return Ok(saved);
    }

    /// <summary>
    /// Delete a specific Penalty Rule record by rule ID key, if it exists.
    /// </summary>
    [HttpDelete("{ruleId}")]
    public async Task<IActionResult> Delete(string ruleId)
    {
        var deleted = await _penaltyRuleService.DeleteAsync(ruleId);
        if (!deleted)
        {
            return NotFound(new { message = $"Penalty Rule with key '{ruleId}' was not found." });
        }

        return NoContent();
    }
}
