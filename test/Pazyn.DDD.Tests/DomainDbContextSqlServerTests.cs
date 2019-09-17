using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pazyn.DDD.Tests.SampleDomain;
using Xunit;

namespace Pazyn.DDD.Tests
{
    public class DomainDbContextSqlServerTests : IDisposable
    {
        public DomainDbContextSqlServerTests()
        {
            ServiceProvider = new ServiceCollection()
                .AddMediatR(typeof(DomainDbContextInMemoryTests).Assembly)
                .AddDbContext<ExpenseDbContext>(x =>
                    x.UseSqlServer("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=PazynDDDTest;Integrated Security=True"), ServiceLifetime.Transient)
                .BuildServiceProvider();

            using (var expenseDbContext = GetDbContext())
            {
                expenseDbContext.Database.EnsureCreated();
            }
        }

        public void Dispose()
        {
            using (var expenseDbContext = GetDbContext())
            {
                expenseDbContext.Database.EnsureDeleted();
            }
        }

        private IServiceProvider ServiceProvider { get; }

        private ExpenseDbContext GetDbContext() =>
            ServiceProvider.GetRequiredService<ExpenseDbContext>();

        [Fact]
        public async Task DomainDbContext_PreAttachEntities()
        {
            using (var expenseDbContext = GetDbContext())
            {
                expenseDbContext.EnsureEntitiesAreAttached();
                expenseDbContext.Expenses.AddRange(
                    new Expense(ExpenseType.Hobby),
                    new Expense(ExpenseType.Food),
                    new Expense(ExpenseType.Bills));
                await expenseDbContext.SaveChangesAsync();
            }

            using (var expenseDbContext = GetDbContext())
            {
                expenseDbContext.EnsureEntitiesAreAttached();
                var savedExpenses = expenseDbContext.Expenses
                    .Include(x => x.Type)
                    .ToArray();

                foreach (var savedExpense in savedExpenses) savedExpense.SetType(ExpenseType.Bills);

                await expenseDbContext.SaveChangesAsync();
            }
        }
    }
}