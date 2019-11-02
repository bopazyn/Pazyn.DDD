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
        private class DomainEventsToDispatch
        {
            public INotification[] DomainEvents { get; set; }
            public Action ClearDomainEvents { get; set; }
        }

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

        public override async Task<Int32> SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            var mediator = this.GetService<IMediator>();
            var domainEventsToDispatches = FindDomainEvents().ToArray();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            foreach (var domainEventsToDispatch in domainEventsToDispatches)
            {
                if (mediator != null)
                {
                    foreach (var domainEvent in domainEventsToDispatch.DomainEvents)
                    {
                        await mediator.Publish(domainEvent, cancellationToken);
                    }
                }

                domainEventsToDispatch.ClearDomainEvents();
            }

            return result;
        }

        private IEnumerable<DomainEventsToDispatch> FindDomainEvents()
        {
            yield return FindDomainEvents<Byte>();
            yield return FindDomainEvents<Int16>();
            yield return FindDomainEvents<UInt16>();
            yield return FindDomainEvents<Int32>();
            yield return FindDomainEvents<UInt32>();
            yield return FindDomainEvents<Int64>();
            yield return FindDomainEvents<UInt64>();
            yield return FindDomainEvents<Guid>();
        }

        private DomainEventsToDispatch FindDomainEvents<T>() where T : struct
        {
            var domainEntities = ChangeTracker
                .Entries<AggregateRoot<T>>()
                .Where(x => x.Entity.DomainEvents.Any())
                .ToArray();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToArray();

            return new DomainEventsToDispatch
            {
                DomainEvents = domainEvents,
                ClearDomainEvents = () =>
                {
                    foreach (var entity in domainEntities)
                    {
                        entity.Entity.ClearDomainEvents();
                    }
                }
            };
        }
    }
}