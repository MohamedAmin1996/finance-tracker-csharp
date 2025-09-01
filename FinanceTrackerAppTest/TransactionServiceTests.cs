using FinanceTrackerApp.Data;
using FinanceTrackerApp.Models;
using FinanceTrackerApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinanceTrackerAppTest
{
    public class TransactionServiceTests
    {
        private FinanceDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<FinanceDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique DB per test
                .Options;

            return new FinanceDbContext(options);
        }

        private ILogger<TransactionService> GetLogger()
        {
            return new Mock<ILogger<TransactionService>>().Object;
        }

        [Fact]
        public async Task AddAsync_ShouldAddIncomeTransaction()
        {
            // Arrange
            var ctx = GetInMemoryDbContext();
            var logger = GetLogger();
            var service = new TransactionService(ctx, logger);

            // Act
            await service.AddAsync(TxType.Income, 1500, "Salary", "Test Income");

            // Assert
            var transactions = ctx.Transactions.ToList();
            Assert.Single(transactions);
            Assert.Equal(1500, transactions[0].Amount);
            Assert.Equal("Salary", transactions[0].Category);
            Assert.Equal(TxType.Income, transactions[0].Type);
        }

        [Fact]
        public async Task AddAsync_ShouldAddExpenseTransaction()
        {
            // Arrange
            var ctx = GetInMemoryDbContext();
            var logger = GetLogger();
            var service = new TransactionService(ctx, logger);

            // Act
            await service.AddAsync(TxType.Expense, 500, "Groceries", "Weekly Shopping");

            // Assert
            var transactions = ctx.Transactions.ToList();
            Assert.Single(transactions);
            Assert.Equal(500, transactions[0].Amount);
            Assert.Equal("Groceries", transactions[0].Category);
            Assert.Equal(TxType.Expense, transactions[0].Type);
        }
    }
}
