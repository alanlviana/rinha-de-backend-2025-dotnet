using System.Text.Json.Serialization;
using api.Endpoints;
using api.Models;
using api.Services;

namespace api.Config;

[JsonSerializable(typeof(List<NewTransaction>))]
[JsonSerializable(typeof(NewTransaction))]
[JsonSerializable(typeof(PaymentProviderSummary))]
[JsonSerializable(typeof(PaymentSummary))]
[JsonSerializable(typeof(UpdateHealthCheck))]
[JsonSerializable(typeof(PaymentProviderHealthCheck))]

public partial class AppJsonSerializerContext : JsonSerializerContext
{

}