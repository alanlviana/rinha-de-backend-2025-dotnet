using System.Text.Json.Serialization;

namespace api.Services;

public class PaymentProviderHealthCheckService
{
    public PaymentProviderHealthCheck Default { get; } = new PaymentProviderHealthCheck();
    public PaymentProviderHealthCheck Fallback { get; } = new PaymentProviderHealthCheck();

    public void UpdateDefault(bool failing, int minResponseTime)
    {
        Default.Failing = failing;
        Default.MinResponseTime = minResponseTime;
    }

    public void UpdateFallback(bool failing, int minResponseTime)
    {
        Fallback.Failing = failing;
        Fallback.MinResponseTime = minResponseTime;
    }

}

public class PaymentProviderHealthCheck
{
    [JsonPropertyName("failing")]
    public bool Failing { get; set; } = false;
    [JsonPropertyName("minResponseTime")]
    public int MinResponseTime { get; set; } = 0;
}
