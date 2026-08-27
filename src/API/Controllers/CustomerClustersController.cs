using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Interfaces;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/customer-clusters")]
public class CustomerClustersController : ControllerBase
{
    private readonly ICustomerClusterService _customerClusterService;

    public CustomerClustersController(ICustomerClusterService customerClusterService)
    {
        _customerClusterService = customerClusterService;
    }

    /// <summary>
    /// Return a list of all Customer Clusters.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerCluster>>> GetAll()
    {
        var records = await _customerClusterService.GetAllAsync();
        return Ok(records);
    }

    /// <summary>
    /// Return a specific Customer Cluster by cluster ID key.
    /// </summary>
    [HttpGet("{clusterId}")]
    public async Task<ActionResult<CustomerCluster>> GetByClusterId(string clusterId)
    {
        var record = await _customerClusterService.GetByClusterIdAsync(clusterId);
        if (record == null)
        {
            return NotFound(new { message = $"Customer Cluster with key '{clusterId}' was not found." });
        }

        return Ok(record);
    }

    /// <summary>
    /// Insert or update a Customer Cluster record on persistence (Upsert).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CustomerCluster>> Save([FromBody] CustomerCluster customerCluster)
    {
        var saved = await _customerClusterService.SaveAsync(customerCluster);
        return Ok(saved);
    }

    /// <summary>
    /// Delete a specific Customer Cluster record by cluster ID key, if it exists.
    /// </summary>
    [HttpDelete("{clusterId}")]
    public async Task<IActionResult> Delete(string clusterId)
    {
        var deleted = await _customerClusterService.DeleteAsync(clusterId);
        if (!deleted)
        {
            return NotFound(new { message = $"Customer Cluster with key '{clusterId}' was not found." });
        }

        return NoContent();
    }
}
