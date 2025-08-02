using System.Text.Json.Serialization;
using api.Config;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Endpoints;

public static class DownloadTransactionsEndpoint
{
    
    private static readonly string OTHER_SERVER = Environment.GetEnvironmentVariable("OTHER_SERVER") ?? throw new ArgumentNullException("OTHER_SERVER is not configured");

    public static void MapDownloadTransactionsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/transactions/download", async ([FromServices] PaymentSummaryService paymentSummaryService, [FromQuery] bool betweenServers = false) =>
        {
            var allDefault = paymentSummaryService.GetAllTransactionsDefault();
            var allFallback = paymentSummaryService.GetAllTransactionsFallback();
            var localTransactions = allDefault.Concat(allFallback);
            var remoteTransactions = new List<NewTransaction>();
            if (!betweenServers)
            {
                var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"http://{OTHER_SERVER}/transactions/download?betweenServers=true");
                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var otherServerTransactions = await response.Content.ReadFromJsonAsync(AppJsonSerializerContext.Default.ListNewTransaction);
                if (otherServerTransactions != null)
                {
                    remoteTransactions.AddRange(otherServerTransactions);
                }
            }

            var allTransactions = localTransactions.Concat(remoteTransactions).OrderBy(t => t.RequestedAt).ToList();

            return Results.Json(allTransactions, AppJsonSerializerContext.Default.ListNewTransaction);
        })
        .WithName("DownloadTransactions");
    }
}
