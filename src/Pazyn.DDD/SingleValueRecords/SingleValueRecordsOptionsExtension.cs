using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace Pazyn.DDD.SingleValueRecords
{
    public class SingleValueRecordsOptionsExtension : IDbContextOptionsExtension
    {
        private ValueConverter[] ValueConverters { get; }

        public SingleValueRecordsOptionsExtension(ValueConverter[] valueConverters)
        {
            ValueConverters = valueConverters;
        }

        public void ApplyServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            new EntityFrameworkRelationalServicesBuilder(services)
                .TryAddProviderSpecificServices(x => x
                    .TryAddSingletonEnumerable<IRelationalTypeMappingSourcePlugin>(new SingleValueRecordsRelationalTypeMappingSourcePlugin(ValueConverters))
                    .TryAddSingletonEnumerable<IMemberTranslatorPlugin, SingleValueRecordsMemberTranslatorPlugin>(sp => ActivatorUtilities.CreateInstance<SingleValueRecordsMemberTranslatorPlugin>(sp, ValueConverters as Object))
                );
        }

        public void Validate(IDbContextOptions options)
        {
        }

        public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

        private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            public ExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }

            public override Boolean IsDatabaseProvider => false;
            public override Int64 GetServiceProviderHashCode() => 0;

            public override void PopulateDebugInfo(IDictionary<String, String> debugInfo) =>
                debugInfo[$"Pazyn: {nameof(SingleValueRecordsOptionsExtension)}"] = "1";

            public override String LogFragment => $"using {nameof(SingleValueRecordsOptionsExtension)}";
        }
    }
}