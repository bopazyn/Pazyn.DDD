using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Pazyn.DDD.SingleValueRecords
{
    public class SingleValueRecordsRelationalTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
    {
        public class SingleValueRecordsRelationalTypeMapping : RelationalTypeMapping
        {
            private SingleValueRecordsRelationalTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters)
            {
            }

            protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters) => new SingleValueRecordsRelationalTypeMapping(parameters);

            public static SingleValueRecordsRelationalTypeMapping Create(ValueConverter valueConverter)
            {
                var coreTypeMappingParameters = new CoreTypeMappingParameters(valueConverter.ProviderClrType, valueConverter, valueGeneratorFactory: valueConverter.MappingHints?.ValueGeneratorFactory);
                RelationalTypeMappingParameters relationalTypeMappingParameters = default;

                if (valueConverter.ProviderClrType == typeof(String))
                {
                    relationalTypeMappingParameters = new RelationalTypeMappingParameters(coreTypeMappingParameters, "nvarchar(max)", StoreTypePostfix.None, System.Data.DbType.String);
                }

                if (valueConverter.ProviderClrType == typeof(Double))
                {
                    relationalTypeMappingParameters = new RelationalTypeMappingParameters(coreTypeMappingParameters, "float", StoreTypePostfix.None, System.Data.DbType.Double);
                }

                if (valueConverter.ProviderClrType == typeof(Decimal))
                {
                    relationalTypeMappingParameters = new RelationalTypeMappingParameters(coreTypeMappingParameters, "decimal", StoreTypePostfix.PrecisionAndScale, System.Data.DbType.Decimal, precision: 18, scale: 2);
                }

                if (valueConverter.ProviderClrType == typeof(Int32))
                {
                    relationalTypeMappingParameters = new RelationalTypeMappingParameters(coreTypeMappingParameters, "int", StoreTypePostfix.None, System.Data.DbType.Int32);
                }

                if (valueConverter.ProviderClrType == typeof(Int64))
                {
                    relationalTypeMappingParameters = new RelationalTypeMappingParameters(coreTypeMappingParameters, "bigint", StoreTypePostfix.None, System.Data.DbType.Int64);
                }

                return new SingleValueRecordsRelationalTypeMapping(relationalTypeMappingParameters);
            }
        }

        private ConcurrentDictionary<Type, SingleValueRecordsRelationalTypeMapping> Converters { get; }

        public SingleValueRecordsRelationalTypeMappingSourcePlugin(IEnumerable<ValueConverter> valueConverters)
        {
            Converters = new ConcurrentDictionary<Type, SingleValueRecordsRelationalTypeMapping>(
                valueConverters.Select(SingleValueRecordsRelationalTypeMapping.Create)
                    .ToDictionary(x => x.ClrType, x => x)
                    .AsEnumerable());
        }

        public RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo) =>
            Converters.TryGetValue(mappingInfo.ClrType, out var mapping)
                ? mapping
                : null;
    }
}