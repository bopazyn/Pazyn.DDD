using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Pazyn.DDD.SingleValue
{
    public static class ValueObjectsExtensions
    {
        public static DbContextOptionsBuilder UseValueObjectsSqlServer(this DbContextOptionsBuilder optionsBuilder, params ValueConverter[] valueConverters)
        {
            var extension = optionsBuilder.Options.FindExtension<ValueObjectsOptionsExtension>() ?? new ValueObjectsOptionsExtension(valueConverters);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            return optionsBuilder;
        }
    }
}