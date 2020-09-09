using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Pazyn.DDD.SingleValue
{
    public class ValueObjectsRelationalTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
    {
        public class ValueObjectsRelationalTypeMapping : RelationalTypeMapping
        {
            private ValueObjectsRelationalTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters)
            {
            }

            protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters) => new ValueObjectsRelationalTypeMapping(parameters);

            public static ValueObjectsRelationalTypeMapping Create(ValueConverter valueConverter)
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

                return new ValueObjectsRelationalTypeMapping(relationalTypeMappingParameters);
            }
        }

        private ConcurrentDictionary<Type, ValueObjectsRelationalTypeMapping> Converters { get; }

        public ValueObjectsRelationalTypeMappingSourcePlugin(IEnumerable<ValueConverter> valueConverters)
        {
            Converters = new ConcurrentDictionary<Type, ValueObjectsRelationalTypeMapping>(
                valueConverters.Select(ValueObjectsRelationalTypeMapping.Create)
                    .ToDictionary(x => x.ClrType, x => x)
                    .AsEnumerable());
        }

        public RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo) =>
            Converters.TryGetValue(mappingInfo.ClrType, out var mapping)
                ? mapping
                : null;
    }
}