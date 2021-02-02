using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Pazyn.DDD
{
    public class DomainDbContext : DbContext
    {
        public DomainDbContext(DbContextOptions options, IPublisher publisher) : base(options)
        {
            Publisher = publisher;
        }

        internal IPublisher Publisher { get; }
        private IDbContextTransaction currentTransaction;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.AddInterceptors(new DomainEventsInterceptor());
        }

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

        public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
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
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransaction();
                throw;
            }
            finally
            {
                if (currentTransaction != null)
                {
                    await currentTransaction.DisposeAsync();
                    currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransaction()
        {
            try
            {
                if (currentTransaction != null)
                {
                    await currentTransaction.RollbackAsync();
                }
            }
            finally
            {
                if (currentTransaction != null)
                {
                    await currentTransaction.DisposeAsync();
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

        public override Int32 SaveChanges(Boolean acceptAllChangesOnSuccess) =>
            throw new NotSupportedException("Use async version of this method");
    }
}