using API.Security;
using Crosscutting.DependencyInjection;
using Crosscutting.Filters;
using Entities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers and Global ApiExceptionFilter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilterAttribute>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

// Configure Infrastructure & Services DI
builder.Services.AddInfrastructureAndServices(builder.Configuration);

// Configure Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BTG Prototyping REST API",
        Version = "v1",
        Description = "Local prototyping REST API environment with Redis Cache-Aside and MongoDB persistence."
    });

    c.AddSecurityDefinition(AppConstants.ApiKeyHeaderName, new OpenApiSecurityScheme
    {
        Description = $"API Key authentication header using '{AppConstants.ApiKeyHeaderName}' header.",
        Name = AppConstants.ApiKeyHeaderName,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = AppConstants.ApiKeyHeaderName
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BTG Prototyping API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root '/'
    });
}

app.UseRouting();

// Custom Security Middleware
app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();

public partial class Program { }
