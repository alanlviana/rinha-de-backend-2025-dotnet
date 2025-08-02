
using System.Text.Json.Serialization;
using api.Endpoints;
using api.Services;

namespace api.Worker;

public class HealthCheckWorker : BackgroundService
{
    private readonly TimeSpan DELAY_HEALTHCHECK = TimeSpan.FromMilliseconds(5050);

    private readonly string MASTER_HEALTH_CHECK = Environment.GetEnvironmentVariable("MASTER_HEALTH_CHECK") ?? throw new ArgumentException("MASTER_HEALTH_CHECK is not configured");
    private readonly string DEFAULT_BASE_URL = Environment.GetEnvironmentVariable("DEFAULT_BASE_URL") ?? throw new ArgumentException("DEFAULT_BASE_URL is not configured");
    private readonly string FALLBACK_BASE_URL = Environment.GetEnvironmentVariable("FALLBACK_BASE_URL") ?? throw new ArgumentException("FALLBACK_BASE_URL is not configured");
    private readonly string OTHER_SERVER = Environment.GetEnvironmentVariable("OTHER_SERVER") ?? throw new ArgumentException("OTHER_SERVER is not configured");
    private readonly PaymentProviderHealthCheckService _paymentProviderHealthCheckService;
    private readonly HttpClient _httpClient;

    public HealthCheckWorker(PaymentProviderHealthCheckService paymentProviderHealthCheckService, HttpClient httpClient)
    {
        this._paymentProviderHealthCheckService = paymentProviderHealthCheckService;
        this._httpClient = httpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (MASTER_HEALTH_CHECK != "true")
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateHealthCheckAsync();
            }
            catch (Exception)
            {

            }
            finally
            {
                await Task.Delay(DELAY_HEALTHCHECK);
            }
        }
    }

    private async Task UpdateHealthCheckAsync()
    {
        var defaultHealthCheck = await GetHealthCheckAsync(DEFAULT_BASE_URL);
        if (defaultHealthCheck != null)
        {
            _paymentProviderHealthCheckService.UpdateDefault(defaultHealthCheck.Failing, defaultHealthCheck.MinResponseTime);
        }
        var fallbackHealthCheck = await GetHealthCheckAsync(FALLBACK_BASE_URL);
        if (fallbackHealthCheck != null)
        {
            _paymentProviderHealthCheckService.UpdateFallback(fallbackHealthCheck.Failing, fallbackHealthCheck.MinResponseTime);
        }

        await UpdateOtherServer(_paymentProviderHealthCheckService.Default, _paymentProviderHealthCheckService.Fallback);

    }

    private async Task UpdateOtherServer(PaymentProviderHealthCheck defaultHealthCheck, PaymentProviderHealthCheck fallbackHealthCheck)
    {
        var httpClient = new HttpClient();
        var response = await httpClient.PostAsJsonAsync($"http://{OTHER_SERVER}/updateHealthChecks",
            new UpdateHealthCheck()
            {
                Default = defaultHealthCheck,
                Fallback = fallbackHealthCheck
            },
            PaymentProviderHealthCheckJsonContext.Default.UpdateHealthCheck,
            cancellationToken: default
        );

    }

    private async Task<PaymentProviderHealthCheck?> GetHealthCheckAsync(string payment_base_url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{payment_base_url}/payments/service-health");
        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return null;
        }

        var paymentProviderHealthCheck = await response.Content.ReadFromJsonAsync(PaymentProviderHealthCheckJsonContext.Default.PaymentProviderHealthCheck);
        return paymentProviderHealthCheck;
    }
}

[JsonSerializable(typeof(UpdateHealthCheck))]
[JsonSerializable(typeof(PaymentProviderHealthCheck))]
public partial class PaymentProviderHealthCheckJsonContext : JsonSerializerContext { }