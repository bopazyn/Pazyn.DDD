using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pazyn.DDD.Tests.SampleDomain;
using Xunit;

namespace Pazyn.DDD.Tests
{
    public class DomainDbContextInMemoryTests
    {
        [Fact]
        public async Task DomainEvents_TriggerOnce()
        {
            var mock = new Mock<IMediator>();
            mock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var serviceProvider = new ServiceCollection()
                .AddSingleton(mock.Object)
                .AddDbContextPool<ExpenseDbContext>(x => x.UseInMemoryDatabase(nameof(ExpenseDbContext)))
                .BuildServiceProvider();

            using (var expenseDbContext = serviceProvider.GetRequiredService<ExpenseDbContext>())
            {
                var expense = new Expense(ExpenseType.Hobby);
                expense.AddDomainEvent();

                expenseDbContext.Expenses.Add(expense);
                await expenseDbContext.SaveChangesAsync();
                await expenseDbContext.SaveChangesAsync();
                await expenseDbContext.SaveChangesAsync();

                mock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}