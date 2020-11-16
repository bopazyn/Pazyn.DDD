# Pazyn.DDD

Library contains set of base classes to work with with Entit Framework's DbContext:
- [`Entity`](./blob/master/src/Pazyn.DDD/Entity.cs)
- [`AggreagteRoot`](./blob/master/src/Pazyn.DDD/AggregateRoot.cs)

## Example

```
public class Expense : AggregateRoot<Int32>
{
    private Expense()
    {
    }

    public Expense(ExpenseNumber number, ExpenseType type)
    {
        Number = number;
        Type = type;
    }

    public ExpenseNumber Number { get; private set; }
    public ExpenseType Type { get; private set; }
}

public record ExpenseNumber
{
    public String String { get; init; }
    public ExpenseNumber(String value) => Value = value;
}

public class ExpenseType : Entity<Int32>
{
    public static readonly ExpenseType Hobby = new ExpenseType(1, nameof(Hobby));
    public static readonly ExpenseType Food = new ExpenseType(2, nameof(Food));
    public static readonly ExpenseType Bills = new ExpenseType(3, nameof(Bills));

    private ExpenseType()
    {
    }

    private ExpenseType(Int32 id, String name) : this()
    {
        Id = id;
        Name = name;
    }

    public String Name { get; private set; }
}

public class ExpenseDbContext : DomainDbContext
{
    public DbSet<Expense> Expenses { get; set; }

    public ExpenseDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExpenseType>()
            .HasData(
                ExpenseType.Hobby,
                ExpenseType.Food,
                ExpenseType.Bills);
    }

    protected override void PreAttachEntities()
    {
        AttachRange(ExpenseType.Hobby, ExpenseType.Food, ExpenseType.Bills);
    }
}

public void ConfigureServices(IServiceCollection services)
{
    services
        .AddMediatR(typeof(DomainDbContextSqlServerTests).Assembly)
        .AddDbContext<ExpenseDbContext>(builder =>
            builder.UseSqlServer("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=PazynDDDTest;Integrated Security=True")
                .EnableSensitiveDataLogging()
                .UseSingleValueRecords(
                    new ValueConverter<ExpenseNumber, String>(y => y.Value, y => new ExpenseNumber(y)))
                .UseLoggerFactory(xUnitLogger.ToLoggerFactory()), ServiceLifetime.Transient)
}

public async Task Adding(ExpenseDbContext expenseDbContext)
{
    expenseDbContext.EnsureEntitiesAreAttached();
    expenseDbContext.Expenses.AddRange(
        new Expense(new ExpenseNumber("1"), ExpenseType.Hobby),
        new Expense(new ExpenseNumber("2"), ExpenseType.Food),
        new Expense(new ExpenseNumber("3"), ExpenseType.Food));

    await expenseDbContext.SaveChangesAsync();
}

public async Task Query(ExpenseDbContext expenseDbContext)
{
    Assert.Equal(2, await expenseDbContext.Expenses.CountAsync(x => x.Type == ExpenseType.Food));

    Assert.Equal(1, await expenseDbContext.Expenses.CountAsync(x => x.Number == new ExpenseNumber("1")));
    Assert.Equal(1, await expenseDbContext.Expenses.CountAsync(x => x.Number.Value == "2"));
    Assert.Equal(1, await expenseDbContext.Expenses.CountAsync(x => x.Number.Value.Contains("3")));
}
```

## Acknowledgements

The work was inpired by:
- [Vladimir Khorikov](https://github.com/vkhorikov) and his [blog](https://enterprisecraftsmanship.com/)
- [Andrew Lock](https://github.com/andrewlock) and his [blog](https://andrewlock.net/)
