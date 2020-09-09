using Microsoft.EntityFrameworkCore;

namespace Pazyn.DDD.Tests.SampleDomain
{
    public class ExpenseDbContext : DomainDbContext
    {
        public ExpenseDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Expense> Expenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Expense>()
                .Property(x => x.Id)
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