using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Pazyn.DDD.Tests.SampleDomain
{
    public class NegativeExpenseNumberEventHandler : INotificationHandler<ExpenseEvent>
    {
        private ExpenseDbContext ExpenseDbContext { get; }

        public NegativeExpenseNumberEventHandler(ExpenseDbContext expenseDbContext)
        {
            ExpenseDbContext = expenseDbContext;
            expenseDbContext.EnsureEntitiesAreAttached();
        }

        public async Task Handle(ExpenseEvent notification, CancellationToken cancellationToken)
        {
            if (!Int32.TryParse(notification.Number.Value, out var n))
            {
                return;
            }

            if (n >= 0)
            {
                return;
            }

            var expense = new Expense(new ExpenseNumber((n + 1).ToString()), ExpenseType.Bills);
            expense.AddDomainEvent();
            ExpenseDbContext.Expenses.Add(expense);
            await ExpenseDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}