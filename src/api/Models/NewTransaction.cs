using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace api.Models;

public struct NewTransaction
{
    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyName("requestedAt")]
    public DateTime RequestedAt { get; set; }


    [JsonConstructor]
    public NewTransaction(string correlationId, decimal amount)
    {
        CorrelationId = correlationId;
        Amount = amount;
        RequestedAt = DateTime.UtcNow;
    }
}