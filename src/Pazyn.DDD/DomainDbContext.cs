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
        private IDbContextTransaction _currentTransaction;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.AddInterceptors(new DomainEventsInterceptor());
        }

        public bool HasActiveTransaction => _currentTransaction != null;

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                return null;
            }

            _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
            return _currentTransaction;
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            if (transaction != _currentTransaction)
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
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransaction()
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync();
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        private bool _areEntitiesPreAttached;

        protected virtual void PreAttachEntities()
        {
        }

        public void EnsureEntitiesAreAttached()
        {
            if (_areEntitiesPreAttached)
            {
                return;
            }

            _areEntitiesPreAttached = true;
            PreAttachEntities();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
            throw new NotSupportedException("Use async version of this method");
    }
}