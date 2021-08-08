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
                    .TryAddSingletonEnumerable<IMemberTranslatorPlugin, SingleValueRecordsMemberTranslatorPlugin>(sp => ActivatorUtilities.CreateInstance<SingleValueRecordsMemberTranslatorPlugin>(sp, ValueConverters as object))
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

            public override bool IsDatabaseProvider => false;
            public override long GetServiceProviderHashCode() => 0;

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) =>
                debugInfo[$"Pazyn: {nameof(SingleValueRecordsOptionsExtension)}"] = "1";

            public override string LogFragment => $"using {nameof(SingleValueRecordsOptionsExtension)}";
        }
    }
}