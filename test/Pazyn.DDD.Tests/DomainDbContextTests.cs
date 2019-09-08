using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Pazyn.DDD.Tests
{
    public class Expense : Entity<Int32>
    {
        public ExpenseType Type { get; private set; }

        private Expense()
        {
        }

        public Expense(ExpenseType type) : this()
        {
            SetType(type);
        }

        public void SetType(ExpenseType type) => Type = type;
    }

    public class ExpenseType : Entity<Int32>
    {
        public String Name { get; private set; }

        private ExpenseType()
        {
        }

        private ExpenseType(Int32 id, String name) : this()
        {
            Id = id;
            Name = name;
        }

        public static readonly ExpenseType Hobby = new ExpenseType(1, nameof(Hobby));
        public static readonly ExpenseType Food = new ExpenseType(2, nameof(Food));
        public static readonly ExpenseType Bills = new ExpenseType(3, nameof(Bills));
    }

    public class ExpenseDbContext : DomainDbContext
    {
        public DbSet<Expense> Expenses { get; set; }

        public ExpenseDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void PreAttachEntities() => 
            AttachRange(ExpenseType.Hobby, ExpenseType.Food, ExpenseType.Bills);
    }

    public class DomainDbContextTests
    {
        [Fact]
        public async Task DomainDbContext_PreAttachEntities()
        {
            var serviceProvider = new ServiceCollection()
                .AddMediatR(typeof(DomainDbContextTests).Assembly)
                .AddDbContextPool<ExpenseDbContext>(x => x.UseInMemoryDatabase(nameof(ExpenseDbContext)))
                .BuildServiceProvider();

            using (var expenseDbContext = serviceProvider.GetRequiredService<ExpenseDbContext>())
            {
                expenseDbContext.Expenses.AddRange(
                    new Expense(ExpenseType.Hobby),
                    new Expense(ExpenseType.Food),
                    new Expense(ExpenseType.Bills));
                await expenseDbContext.SaveChangesAsync();
            }

            using (var expenseDbContext = serviceProvider.GetRequiredService<ExpenseDbContext>())
            {
                var savedExpenses = expenseDbContext.Expenses
                    .Include(x => x.Type)
                    .ToArray();

                foreach (var savedExpense in savedExpenses)
                {
                    savedExpense.SetType(ExpenseType.Bills);
                }

                await expenseDbContext.SaveChangesAsync();
            }
        }
    }
}