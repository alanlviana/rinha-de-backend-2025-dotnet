using System.Text.Json.Serialization;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Endpoints;

public static class UpdateHealthCheckEndpoint
{
    public static void MapUpdateHealthCheckEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/updateHealthChecks", ([FromBody] UpdateHealthCheck updateHealthChecks, PaymentProviderHealthCheckService paymentProviderHealthCheckService) =>
        {
            paymentProviderHealthCheckService.Default.Failing = updateHealthChecks.Default.Failing;
            paymentProviderHealthCheckService.Default.MinResponseTime = updateHealthChecks.Default.MinResponseTime;
            paymentProviderHealthCheckService.Fallback.Failing = updateHealthChecks.Fallback.Failing;
            paymentProviderHealthCheckService.Fallback.MinResponseTime = updateHealthChecks.Fallback.MinResponseTime;
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