using System.Net.Mail;
using api.Models;

namespace api.Services;

public class PaymentSummaryService
{
    private readonly Summary _summaryDefault = new Summary();
    private readonly Summary _summaryFallback = new Summary();

    public void AddTransactionDefault(NewTransaction newTransaction)
    {
        _summaryDefault.AddTransaction(newTransaction);
    }

    public (long TotalRequests, decimal Sum) SumPaymentsBetweenDefault(DateTime from, DateTime to)
    {
        return _summaryDefault.SumPaymentsBetween(from, to);
    }

        public void AddTransactionFallback(NewTransaction newTransaction)
    {
        _summaryFallback.AddTransaction(newTransaction);
    }

    public (long TotalRequests, decimal Sum) SumPaymentsBetweenFallback(DateTime from, DateTime to)
    {
        return _summaryFallback.SumPaymentsBetween(from, to);
    }
}

public class Summary
{

    private readonly SortedDictionary<DateTime, List<decimal>> _data = new SortedDictionary<DateTime, List<decimal>>();

    private readonly object _lock = new();

    
    public void AddTransaction(NewTransaction newTransaction)
    {
        lock (_lock)
        {
            if (!_data.TryGetValue(newTransaction.RequestedAt, out var list))
            {
                list = new List<decimal>();
                _data[newTransaction.RequestedAt] = list;
            }
            list.Add(newTransaction.Amount);
        }
    }


    public (long TotalRequests, decimal Sum) SumPaymentsBetween(DateTime from, DateTime to)
    {
        lock (_lock)
        {
            long requestCount = 0;
            decimal sum = 0;
            foreach (var item in _data.Where(kvp => kvp.Key >= from && kvp.Key <= to))
            {
                foreach (var amount in item.Value)
                {
                    requestCount++;
                    sum += amount;
                }
            }
            return (requestCount, sum);
        }
    }
}