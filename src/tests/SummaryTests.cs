using api.Models;
using api.Services;

namespace tests;

public class SummaryTests
{
    [Fact]
    public void Summary_WhenAddTransactions_MustReturnCorrectRequestCount()
    {
        // Arrange
        var transaction1 = new NewTransaction() { Amount = 10.20m, CorrelationId = Guid.NewGuid().ToString(), RequestedAt = new DateTime(2025, 07, 31, 18, 10, 00) };
        var transaction2 = new NewTransaction() { Amount = 22.20m, CorrelationId = Guid.NewGuid().ToString(), RequestedAt = new DateTime(2025, 07, 31, 18, 16, 00) };
        var transaction3 = new NewTransaction() { Amount = 33.20m, CorrelationId = Guid.NewGuid().ToString(), RequestedAt = new DateTime(2025, 07, 31, 18, 26, 00) };
        var summary = new Summary();

        // Act
        summary.AddTransaction(transaction1);
        summary.AddTransaction(transaction2);
        summary.AddTransaction(transaction3);

        var result1 = summary.SumPaymentsBetween(new DateTime(2025, 07, 31, 18, 10, 00), new DateTime(2025, 07, 31, 18, 15, 10));
        var result2 = summary.SumPaymentsBetween(new DateTime(2025, 07, 31, 18, 10, 00), new DateTime(2025, 07, 31, 18, 16, 10));
        var result3 = summary.SumPaymentsBetween(new DateTime(2025, 07, 31, 18, 10, 00), new DateTime(2025, 07, 31, 18, 26, 10));
        var result4 = summary.SumPaymentsBetween(new DateTime(2025, 07, 31, 18, 10, 10), new DateTime(2025, 07, 31, 18, 26, 10));

        // Assert
        Assert.Equal(1, result1.TotalRequests);
        Assert.Equal(10.20m, result1.Sum);

        Assert.Equal(2, result2.TotalRequests);
        Assert.Equal(32.40m, result2.Sum);

        Assert.Equal(3, result3.TotalRequests);
        Assert.Equal(65.60m, result3.Sum);

        Assert.Equal(2, result4.TotalRequests);
        Assert.Equal(55.40m, result4.Sum);

    }
}
