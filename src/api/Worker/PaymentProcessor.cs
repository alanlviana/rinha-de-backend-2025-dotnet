
using api.Models;
using api.Services;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace api.Worker;

public class PaymentProcessor : BackgroundService
{
    private const int POLL_DELAY = 10;

    private readonly string DEFAULT_BASE_URL = Environment.GetEnvironmentVariable("DEFAULT_BASE_URL") ?? throw new ArgumentException("DEFAULT_BASE_URL is not configured");
    private readonly string FALLBACK_BASE_URL = Environment.GetEnvironmentVariable("FALLBACK_BASE_URL") ?? throw new ArgumentException("FALLBACK_BASE_URL is not configured");
    private readonly QueueTransactionService _queueTransactionService;
    private readonly PaymentSummaryService _paymentSummaryService;
    private readonly PaymentProviderHealthCheckService _paymentProviderHealthCheckService;

    public PaymentProcessor(QueueTransactionService queueTransactionService, PaymentSummaryService paymentSummaryService, PaymentProviderHealthCheckService paymentProviderHealthCheckService)
    {
        this._queueTransactionService = queueTransactionService;
        this._paymentSummaryService = paymentSummaryService;
        this._paymentProviderHealthCheckService = paymentProviderHealthCheckService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = Channel.CreateUnbounded<NewTransaction>();

        _ = Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queueTransactionService.TryDequeue(out var transaction))
                {
                    await channel.Writer.WriteAsync(transaction, stoppingToken);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(POLL_DELAY));
                }
            }
            channel.Writer.Complete();
        });

        await Parallel.ForEachAsync(channel.Reader.ReadAllAsync(stoppingToken),
            new ParallelOptions()
            {
                MaxDegreeOfParallelism = 8,
                CancellationToken = stoppingToken
            },
            async (transaction, ct) =>
            {
                try
                {
                    await ProcessPayment(transaction, ct);
                }
                catch (Exception ex)
                {
                    _queueTransactionService.EnqueueTransaction(transaction);
                }
            }
        );

    }
    private async Task ProcessPayment(NewTransaction transaction, CancellationToken cancellationToken)
    {
        var paymentProviderBaseUrl = ChoosePaymentProvider();

        Console.WriteLine($"Processing payment {transaction.CorrelationId}");
        HttpClient httpClient = new HttpClient();
        var result = await httpClient.PostAsJsonAsync(
            $"{paymentProviderBaseUrl}/payments/",
            transaction,
            NewTransactionJsonContext.Default.NewTransaction,
            cancellationToken
        );

        if (result.IsSuccessStatusCode)
        {
            _paymentSummaryService.AddTransactionDefault(transaction);
            Console.WriteLine($"Payment process succefully - {transaction.Amount} - {paymentProviderBaseUrl} - {transaction.CorrelationId} - {transaction.RequestedAt.ToString("R")} - 200");
        }
        else if (Is4xxClientError(result.StatusCode))
        {
            Console.WriteLine($"Payment process failed - {paymentProviderBaseUrl} - {transaction.CorrelationId} - {result.StatusCode}");
        }
        else
        {
            throw new Exception("Status code is not 2XX or 4XX");
        }
    }

    private string ChoosePaymentProvider()
    {
        if (_paymentProviderHealthCheckService.Fallback.Failing)
        {
            return DEFAULT_BASE_URL;
        }

        if (_paymentProviderHealthCheckService.Default.Failing)
        {
            return FALLBACK_BASE_URL;
        }

        return DEFAULT_BASE_URL;
    }

    public static bool Is4xxClientError(HttpStatusCode statusCode)
    {
        int code = (int)statusCode;
        return code >= 400 && code <= 499;
    }
}


[JsonSerializable(typeof(NewTransaction))]
public partial class NewTransactionJsonContext : JsonSerializerContext
{
}