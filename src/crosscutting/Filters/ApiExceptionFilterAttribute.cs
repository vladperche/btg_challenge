using Entities.Domains;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Crosscutting.Filters;

public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is DomainException domainEx)
        {
            var details = new ProblemDetails
            {
                Title = "Domain Business Rule Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = domainEx.Message
            };

            context.Result = new BadRequestObjectResult(details);
            context.ExceptionHandled = true;
            return;
        }

        var genericDetails = new ProblemDetails
        {
            Title = "An unhandled internal server error occurred",
            Status = StatusCodes.Status500InternalServerError,
            Detail = context.Exception.Message
        };

        context.Result = new ObjectResult(genericDetails)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        context.ExceptionHandled = true;
    }
}
