using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Pazyn.DDD
{
    public class DomainDbContext : DbContext
    {
        public DomainDbContext(DbContextOptions options) : base(options)
        {
        }

        private IDbContextTransaction currentTransaction;

        public Boolean HasActiveTransaction => currentTransaction != null;

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (currentTransaction != null)
            {
                return null;
            }

            currentTransaction = await Database.BeginTransactionAsync(cancellationToken);

            return currentTransaction;
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            if (transaction != currentTransaction)
            {
                throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");
            }

            try
            {
                await SaveChangesAsync();
                transaction.Commit();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (currentTransaction != null)
                {
                    currentTransaction.Dispose();
                    currentTransaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                currentTransaction?.Rollback();
            }
            finally
            {
                if (currentTransaction != null)
                {
                    currentTransaction.Dispose();
                    currentTransaction = null;
                }
            }
        }


        private Boolean areEntitiesPreAttached;

        protected virtual void PreAttachEntities()
        {
        }

        public void EnsureEntitiesAreAttached()
        {
            if (areEntitiesPreAttached)
            {
                return;
            }
            areEntitiesPreAttached = true;
            PreAttachEntities();
        }

        public override DbQuery<TQuery> Query<TQuery>()
        {
            EnsureEntitiesAreAttached();
            return base.Query<TQuery>();
        }

        public override Int32 SaveChanges(Boolean acceptAllChangesOnSuccess) =>
            throw new NotSupportedException("Use async version of this method");

        public override async Task<Int32> SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var mediator = this.GetService<IMediator>();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            await Task.WhenAll(DispatchDomainEventsAsync(mediator, cancellationToken));
            return result;
        }

        private IEnumerable<Task> DispatchDomainEventsAsync(IMediator mediator, CancellationToken cancellationToken)
        {
            yield return DispatchDomainEventsAsync<Byte>(mediator, cancellationToken);
            yield return DispatchDomainEventsAsync<Int16>(mediator, cancellationToken);
            yield return DispatchDomainEventsAsync<UInt16>(mediator, cancellationToken);
            yield return DispatchDomainEventsAsync<Int32>(mediator, cancellationToken);
            yield return DispatchDomainEventsAsync<UInt32>(mediator, cancellationToken);
            yield return DispatchDomainEventsAsync<Int64>(mediator, cancellationToken);
            yield return DispatchDomainEventsAsync<UInt64>(mediator, cancellationToken);
            yield return DispatchDomainEventsAsync<Guid>(mediator, cancellationToken);
        }

        private Task DispatchDomainEventsAsync<T>(IMediator mediator, CancellationToken cancellationToken) where T : struct
        {
            var domainEntities = ChangeTracker
                .Entries<AggregateRoot<T>>()
                .Where(x => x.Entity.DomainEvents.Any())
                .ToArray();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToArray();

            foreach (var entity in domainEntities)
            {
                entity.Entity.ClearDomainEvents();
            }

            return Task.WhenAll(domainEvents.Select(x => mediator.Publish(x, cancellationToken)));
        }
    }
}