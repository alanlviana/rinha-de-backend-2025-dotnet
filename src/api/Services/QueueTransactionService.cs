using System.Collections.Concurrent;
using System.Transactions;
using api.Models;

namespace api.Services;

public class QueueTransactionService
{
    private readonly ConcurrentQueue<NewTransaction> _transactionQueue = new ConcurrentQueue<NewTransaction>();

    
    public void EnqueueTransaction(NewTransaction newTransaction)
    {
        _transactionQueue.Enqueue(newTransaction);
    }

    public bool TryDequeue(out NewTransaction transaction)
    {
        return _transactionQueue.TryDequeue(out transaction);
    }
}