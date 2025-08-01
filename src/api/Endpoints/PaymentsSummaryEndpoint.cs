using System.Text.Json.Serialization;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Endpoints;

public static class PaymentsSummaryEndpoint
{
    public static void MapPaymentsSummaryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/payments-summary", async ([FromQuery] DateTime from, [FromQuery] DateTime to, [FromServices] PaymentSummaryService paymentSummaryService, [FromQuery] bool betweenServers = false) =>
        {
            var summaryDefault = paymentSummaryService.SumPaymentsBetweenDefault(from, to);
            var summaryFallback = paymentSummaryService.SumPaymentsBetweenFallback(from, to);

            if (!betweenServers)
            {
                var httpClient = new HttpClient();
                var OTHER_SERVER = Environment.GetEnvironmentVariable("OTHER_SERVER");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{OTHER_SERVER}/payments-summary?betweenServers=true&to={to.ToString("O")}&from={from.ToString("O")}");
                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var otherServerSummary = await response.Content.ReadFromJsonAsync(PaymentSummaryJsonContext.Default.PaymentSummary);

                if (otherServerSummary == null)
                {
                    throw new InvalidOperationException("Algo de errado");
                }

                return new PaymentSummary()
                {
                    Default = new PaymentProviderSummary()
                    {
                        TotalRequests = summaryDefault.TotalRequests + otherServerSummary.Default.TotalRequests,
                        TotalAmount = summaryDefault.Sum + otherServerSummary.Default.TotalAmount,

                    },
                    Fallback = new PaymentProviderSummary()
                    {
                        TotalRequests = summaryFallback.TotalRequests +  otherServerSummary.Fallback.TotalRequests,
                        TotalAmount = summaryFallback.Sum + otherServerSummary.Fallback.TotalAmount,

                    },
                };
            }

            return new PaymentSummary()
            {
                Default = new PaymentProviderSummary()
                {
                    TotalRequests = summaryDefault.Item1,
                    TotalAmount = summaryDefault.Item2,

                },
                Fallback = new PaymentProviderSummary()
                {
                    TotalRequests = summaryFallback.Item1,
                    TotalAmount = summaryFallback.Item2,

                },
            };
        })
        .WithName("PaymentsSummary");
    }
}

[JsonSerializable(typeof(PaymentSummary))]
[JsonSerializable(typeof(PaymentProviderSummary))]
public partial class PaymentSummaryJsonContext : JsonSerializerContext
{
}