using System.Net.Sockets;
using System.Text.Json.Serialization;
using api.Models;
using api.Services;
using api.Util;
using Microsoft.AspNetCore.Mvc;

namespace api.Endpoints;

public static class PaymentsSummaryEndpoint
{
    private static readonly string OTHER_SERVER = Environment.GetEnvironmentVariable("OTHER_SERVER") ?? throw new ArgumentNullException("OTHER_SERVER is not configured");
    

    public static void MapPaymentsSummaryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/payments-summary", async ([FromQuery] DateTime from, [FromQuery] DateTime to, [FromServices] PaymentSummaryService paymentSummaryService, [FromQuery] bool betweenServers = false) =>
        {
            var summaryDefault = paymentSummaryService.SumPaymentsBetweenDefault(from, to);
            var summaryFallback = paymentSummaryService.SumPaymentsBetweenFallback(from, to);

            if (!betweenServers)
            {
                var httpClient = SocketHttpClient.HttpClient(OTHER_SERVER);
                var request = new HttpRequestMessage(HttpMethod.Get, $"http://socket/payments-summary?betweenServers=true&to={to.ToString("O")}&from={from.ToString("O")}");
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
                        TotalRequests = summaryFallback.TotalRequests + otherServerSummary.Fallback.TotalRequests,
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