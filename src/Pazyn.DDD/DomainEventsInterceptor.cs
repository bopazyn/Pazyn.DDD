using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Pazyn.DDD
{
    public class DomainEventsInterceptor : ISaveChangesInterceptor, IDbTransactionInterceptor
    {
        private static DomainEventsToDispatch FindDomainEvents<T>(DomainDbContext domainDbContext) where T : struct
        {
            var domainEntities = domainDbContext.ChangeTracker
                .Entries<AggregateRoot<T>>()
                .Where(x => x.Entity.DomainEvents.Any())
                .ToArray();

            var notifications = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToArray();

            return new DomainEventsToDispatch
            {
                DomainEvents = notifications,
                ClearDomainEvents = () =>
                {
                    foreach (var entity in domainEntities)
                    {
                        entity.Entity.ClearDomainEvents();
                    }
                }
            };
        }

        private static DomainEventsToDispatch[] FindDomainEvents(DomainDbContext domainDbContext) =>
            Enumerable.Empty<DomainEventsToDispatch>()
                .Append(FindDomainEvents<byte>(domainDbContext))
                .Append(FindDomainEvents<short>(domainDbContext))
                .Append(FindDomainEvents<ushort>(domainDbContext))
                .Append(FindDomainEvents<int>(domainDbContext))
                .Append(FindDomainEvents<uint>(domainDbContext))
                .Append(FindDomainEvents<long>(domainDbContext))
                .Append(FindDomainEvents<ulong>(domainDbContext))
                .Append(FindDomainEvents<Guid>(domainDbContext))
                .ToArray();

        public async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = new())
        {
            if (eventData.Context is DomainDbContext {HasActiveTransaction: false} domainDbContext)
            {
                await PublishDomainEvents(domainDbContext, cancellationToken);
            }

            return result;
        }

        private async Task PublishDomainEvents(DomainDbContext domainDbContext, CancellationToken cancellationToken)
        {
            var domainEventsToDispatches = FindDomainEvents(domainDbContext);

            foreach (var domainEventsToDispatch in domainEventsToDispatches)
            {
                domainEventsToDispatch.ClearDomainEvents();
            }

            foreach (var domainEvent in domainEventsToDispatches.SelectMany(x => x.DomainEvents))
            {
                await domainDbContext.Publisher.Publish(domainEvent, cancellationToken);
            }
        }

        public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result) => result;

        public int SavedChanges(SaveChangesCompletedEventData eventData, int result) => result;

        public void SaveChangesFailed(DbContextErrorEventData eventData)
        {
        }

        public ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = new()) => ValueTask.FromResult(result);

        public Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = new()) => Task.CompletedTask;

        public InterceptionResult<DbTransaction> TransactionStarting(DbConnection connection, TransactionStartingEventData eventData, InterceptionResult<DbTransaction> result) => result;

        public DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result) => result;

        public ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(DbConnection connection, TransactionStartingEventData eventData, InterceptionResult<DbTransaction> result, CancellationToken cancellationToken = new()) =>
            ValueTask.FromResult(result);

        public ValueTask<DbTransaction> TransactionStartedAsync(DbConnection connection, TransactionEndEventData eventData, DbTransaction result, CancellationToken cancellationToken = new()) => ValueTask.FromResult(result);

        public DbTransaction TransactionUsed(DbConnection connection, TransactionEventData eventData, DbTransaction result) => result;

        public ValueTask<DbTransaction> TransactionUsedAsync(DbConnection connection, TransactionEventData eventData, DbTransaction result, CancellationToken cancellationToken = new()) => ValueTask.FromResult(result);

        public InterceptionResult TransactionCommitting(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result) => result;

        public void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
        {
        }

        public ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = new()) => ValueTask.FromResult(result);

        public async Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = new())
        {
            if (eventData.Context is DomainDbContext domainDbContext)
            {
                await PublishDomainEvents(domainDbContext, cancellationToken);
            }
        }

        public InterceptionResult TransactionRollingBack(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result) => result;

        public void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
        {
        }

        public ValueTask<InterceptionResult> TransactionRollingBackAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = new()) => ValueTask.FromResult(result);

        public Task TransactionRolledBackAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = new())
        {
            if (eventData.Context is DomainDbContext domainDbContext)
            {
                var domainEventsToDispatches = FindDomainEvents(domainDbContext);
                foreach (var domainEventsToDispatch in domainEventsToDispatches)
                {
                    domainEventsToDispatch.ClearDomainEvents();
                }
            }

            return Task.CompletedTask;
        }

        public InterceptionResult CreatingSavepoint(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result) => result;

        public void CreatedSavepoint(DbTransaction transaction, TransactionEventData eventData)
        {
        }

        public ValueTask<InterceptionResult> CreatingSavepointAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = new()) => ValueTask.FromResult(result);

        public Task CreatedSavepointAsync(DbTransaction transaction, TransactionEventData eventData, CancellationToken cancellationToken = new()) => Task.CompletedTask;

        public InterceptionResult RollingBackToSavepoint(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result) => result;

        public void RolledBackToSavepoint(DbTransaction transaction, TransactionEventData eventData)
        {
        }

        public ValueTask<InterceptionResult> RollingBackToSavepointAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = new()) => ValueTask.FromResult(result);

        public Task RolledBackToSavepointAsync(DbTransaction transaction, TransactionEventData eventData, CancellationToken cancellationToken = new()) => Task.CompletedTask;

        public InterceptionResult ReleasingSavepoint(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result) => result;

        public void ReleasedSavepoint(DbTransaction transaction, TransactionEventData eventData)
        {
        }

        public ValueTask<InterceptionResult> ReleasingSavepointAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = new()) => ValueTask.FromResult(result);

        public Task ReleasedSavepointAsync(DbTransaction transaction, TransactionEventData eventData, CancellationToken cancellationToken = new()) => Task.CompletedTask;

        public void TransactionFailed(DbTransaction transaction, TransactionErrorEventData eventData)
        {
        }

        public Task TransactionFailedAsync(DbTransaction transaction, TransactionErrorEventData eventData, CancellationToken cancellationToken = new()) => Task.CompletedTask;
    }
}