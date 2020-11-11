using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Pazyn.DDD.SingleValueRecords
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder UseSingleValueRecords(this DbContextOptionsBuilder optionsBuilder, params ValueConverter[] valueConverters)
        {
            var extension = optionsBuilder.Options.FindExtension<SingleValueRecordsOptionsExtension>() ?? new SingleValueRecordsOptionsExtension(valueConverters);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            return optionsBuilder;
        }
    }
}