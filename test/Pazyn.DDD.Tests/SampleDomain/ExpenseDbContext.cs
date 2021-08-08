using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Pazyn.DDD.Tests.SampleDomain
{
    public class ExpenseDbContext : DomainDbContext
    {
        public ExpenseDbContext(DbContextOptions options, IPublisher publisher) : base(options, publisher)
        {
        }

        public DbSet<Expense> Expenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Expense>()
                .Property(x => x.Id)
                .HasValueGenerator<TemporaryIntValueGenerator>()
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Expense>()
                .Property(x => x.Number)
                .IsRequired();

            modelBuilder.Entity<ExpenseType>()
                .HasData(
                    ExpenseType.Hobby,
                    ExpenseType.Food,
                    ExpenseType.Bills);
        }

        protected override void PreAttachEntities() =>
            AttachRange(ExpenseType.Hobby, ExpenseType.Food, ExpenseType.Bills);
    }
}