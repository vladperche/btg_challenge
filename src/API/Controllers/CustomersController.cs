using System.Threading.Tasks;
using Entities.Interfaces;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Process, enrich, classify, and persist a Customer record.
    /// Request Body contains only input fields (enriched fields are calculated after request and returned in response).
    /// </summary>
    [HttpPost("classify")]
    public async Task<ActionResult<Customer>> Create([FromBody] CustomerClassificationRequest request)
    {
        var processed = await _customerService.ProcessAndSaveAsync(request);
        return Ok(processed);
    }
}
