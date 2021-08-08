using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Pazyn.DDD.SingleValueRecords
{
    public class SingleValueRecordsMemberTranslatorPlugin : IMemberTranslatorPlugin
    {
        private class ValueObjectsMemberTranslator : IMemberTranslator
        {
            private ISqlExpressionFactory SqlExpressionFactory { get; }

            private Type ClrType { get; }

            public ValueObjectsMemberTranslator(ISqlExpressionFactory sqlExpressionFactory, Type clrType)
            {
                SqlExpressionFactory = sqlExpressionFactory;
                ClrType = clrType;
            }

            public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType, IDiagnosticsLogger<DbLoggerCategory.Query> logger) =>
                instance.Type != ClrType
                    ? null
                    : SqlExpressionFactory.Convert(instance, returnType);
        }

        public SingleValueRecordsMemberTranslatorPlugin(ISqlExpressionFactory sqlExpressionFactory, ValueConverter[] valueConverters) =>
            _translators = valueConverters
                .Select(x => new ValueObjectsMemberTranslator(sqlExpressionFactory, x.ModelClrType))
                .Cast<IMemberTranslator>()
                .ToArray();

        private readonly IMemberTranslator[] _translators;
        public IEnumerable<IMemberTranslator> Translators => _translators;
    }
}