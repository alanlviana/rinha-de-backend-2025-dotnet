using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Endpoints;

public static class CreatePaymentsEndpoint
{
    public static void MapCreatePaymentsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/payments", ([FromBody] NewTransaction transaction, [FromServices] QueueTransactionService queueTransactionService) =>
        {
            queueTransactionService.EnqueueTransaction(transaction);
        })
        .WithName("Payments");
    }
}