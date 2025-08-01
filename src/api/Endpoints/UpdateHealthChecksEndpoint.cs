using System.Text.Json.Serialization;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Endpoints;

public static class UpdateHealthCheckEndpoint
{
    public static void MapUpdateHealthCheckEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/updateHealthChecks", (ILogger<Program> logger,[FromBody] UpdateHealthCheck updateHealthChecks, PaymentProviderHealthCheckService paymentProviderHealthCheckService) =>
        {
            paymentProviderHealthCheckService.Default.Failing = updateHealthChecks.Default.Failing;
            paymentProviderHealthCheckService.Default.MinResponseTime = updateHealthChecks.Default.MinResponseTime;
            paymentProviderHealthCheckService.Fallback.Failing = updateHealthChecks.Fallback.Failing;
            paymentProviderHealthCheckService.Fallback.MinResponseTime = updateHealthChecks.Fallback.MinResponseTime;
            logger.LogInformation($"d.f={updateHealthChecks.Default.Failing}|d.mt={updateHealthChecks.Default.MinResponseTime}|f.f={updateHealthChecks.Fallback.Failing}|f.mt={updateHealthChecks.Fallback.MinResponseTime}");
        })
        .WithName("UpdateHealthChecks");
    }
}

public class UpdateHealthCheck
{
    [JsonPropertyName("default")]
    public PaymentProviderHealthCheck Default { get; set; }
    [JsonPropertyName("fallback")]
    public PaymentProviderHealthCheck Fallback { get; set; }

}