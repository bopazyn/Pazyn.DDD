using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Pazyn.DDD.SingleValue;
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
                        .UseValueObjectsSqlServer(
                            new ValueConverter<ExpenseNumber, String>(y => y.Value, y => new ExpenseNumber(y)))
                        .UseLoggerFactory(xUnitLogger.ToLoggerFactory()), ServiceLifetime.Transient)
                .BuildServiceProvider();

            using var expenseDbContext = GetDbContext();
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
    }
}