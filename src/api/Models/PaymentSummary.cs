using System.Text.Json.Serialization;

namespace api.Models;


public class PaymentSummary
{
    [JsonPropertyName("default")]

    public required PaymentProviderSummary Default { get; set; }
    [JsonPropertyName("fallback")]

    public required PaymentProviderSummary Fallback { get; set; }

}

public class PaymentProviderSummary
{
    [JsonPropertyName("totalRequests")]

    public long TotalRequests { get; set; }
    [JsonPropertyName("totalAmount")]

    public decimal TotalAmount { get; set; }
}
