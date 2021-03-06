using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Pazyn.DDD.SingleValueRecords;
using Pazyn.DDD.Tests.SampleDomain;
using Xunit;
using Xunit.Abstractions;

namespace Pazyn.DDD.Tests
{
    public class DomainDbContextSqlServerTests : IDisposable
    {
        public DomainDbContextSqlServerTests(ITestOutputHelper output)
        {
            var xUnitLogger = new XUnitLogger(output);

            ServiceProvider = new ServiceCollection()
                .AddMediatR(typeof(DomainDbContextSqlServerTests).Assembly)
                .AddDbContext<ExpenseDbContext>(builder =>
                    builder.UseSqlServer("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=PazynDDDTest;Integrated Security=True")
                        .EnableSensitiveDataLogging()
                        .UseSingleValueRecords(
                            new ValueConverter<ExpenseNumber, String>(y => y.Value, y => new ExpenseNumber(y)))
                        .UseLoggerFactory(xUnitLogger.ToLoggerFactory()), ServiceLifetime.Transient)
                .BuildServiceProvider();

            using var expenseDbContext = ServiceProvider.GetRequiredService<ExpenseDbContext>();
            expenseDbContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            using var expenseDbContext = GetDbContext();
            expenseDbContext.Database.EnsureDeleted();
        }

        private IServiceProvider ServiceProvider { get; }

        private ExpenseDbContext GetDbContext()
        {
            var expenseDbContext = ServiceProvider.GetRequiredService<ExpenseDbContext>();
            expenseDbContext.EnsureEntitiesAreAttached();
            return expenseDbContext;
        }

        [Fact]
        public async Task DomainDbContext_PreAttachEntities()
        {
            {
                await using var expenseDbContext = GetDbContext();

                var entities = new Expense(new ExpenseNumber("2"), ExpenseType.Bills);
                expenseDbContext.Expenses.Add(entities);

                await expenseDbContext.SaveChangesAsync();
            }
            {
                await using var expenseDbContext = GetDbContext();
                var savedExpenses = expenseDbContext.Expenses
                    .Include(x => x.Type)
                    .ToArray();

                foreach (var savedExpense in savedExpenses)
                {
                    savedExpense.Type = ExpenseType.Bills;
                }

                await expenseDbContext.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task QueryWithMostCommonFunctionsOnValueObjects()
        {
            {
                await using var expenseDbContext = GetDbContext();
                expenseDbContext.Expenses.AddRange(
                    new Expense(new ExpenseNumber("1"), ExpenseType.Hobby),
                    new Expense(new ExpenseNumber("2"), ExpenseType.Food),
                    new Expense(new ExpenseNumber("3"), ExpenseType.Food));
                await expenseDbContext.SaveChangesAsync();
            }
            {
                await using var expenseDbContext = GetDbContext();
                Assert.Equal(2, await expenseDbContext.Expenses.CountAsync(x => x.Type == ExpenseType.Food));

                Assert.Equal(1, await expenseDbContext.Expenses.CountAsync(x => x.Number == new ExpenseNumber("1")));
                Assert.Equal(1, await expenseDbContext.Expenses.CountAsync(x => x.Number.Value == "2"));
                Assert.Equal(1, await expenseDbContext.Expenses.CountAsync(x => x.Number.Value.Contains("3")));
            }
        }

        [Fact]
        public async Task EventsAreTriggerOnlyOnce()
        {
            await using var expenseDbContext = GetDbContext();

            var expense = new Expense(new ExpenseNumber("1"), ExpenseType.Hobby);
            expense.AddDomainEvent();
            expenseDbContext.Expenses.Add(expense);

            await expenseDbContext.SaveChangesAsync();
            Assert.Empty(expense.DomainEvents);
        }

        [Theory]
        [InlineData(-2, 3)]
        [InlineData(-10, 11)]
        public async Task EventsAreTriggerInChain(Int32 n, Int32 expected)
        {
            await using var expenseDbContext = GetDbContext();

            var expense = new Expense(new ExpenseNumber(n.ToString()), ExpenseType.Hobby);
            expense.AddDomainEvent();
            expenseDbContext.Expenses.Add(expense);

            await expenseDbContext.SaveChangesAsync();
            Assert.Equal(expected, await expenseDbContext.Expenses.CountAsync());
        }

        [Fact]
        public async Task EventsAreTriggerAfterTransactionCommit()
        {
            await using var expenseDbContext = GetDbContext();

            var expense = new Expense(new ExpenseNumber("1"), ExpenseType.Hobby);
            expense.AddDomainEvent();
            expenseDbContext.Expenses.Add(expense);

            await using var transaction = await expenseDbContext.BeginTransactionAsync();
            await expenseDbContext.SaveChangesAsync();
            Assert.NotEmpty(expense.DomainEvents);

            await expenseDbContext.CommitTransactionAsync(transaction);
            Assert.Empty(expense.DomainEvents);
        }
    }
}