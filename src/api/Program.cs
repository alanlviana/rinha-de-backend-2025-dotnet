using api.Config;
using api.Endpoints;
using api.Services;
using api.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
  options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddSingleton<QueueTransactionService>();
builder.Services.AddSingleton<PaymentSummaryService>();
builder.Services.AddSingleton<PaymentProviderHealthCheckService>();

builder.Services.AddHostedService<PaymentProcessor>();
builder.Services.AddHostedService<HealthCheckWorker>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapCreatePaymentsEndpoint();
app.MapPaymentsSummaryEndpoint();
app.MapUpdateHealthCheckEndpoint();

app.Run();
