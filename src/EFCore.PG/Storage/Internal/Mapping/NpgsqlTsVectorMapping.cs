﻿using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlTsVectorTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTsVectorTypeMapping() : base("tsvector", typeof(NpgsqlTsVector), NpgsqlDbType.TsVector) { }

        protected NpgsqlTsVectorTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.TsVector) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlTsVectorTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            Check.NotNull(value, nameof(value));
            var vector = (NpgsqlTsVector)value;
            var builder = new StringBuilder();
            builder.Append("TSVECTOR  ");
            var indexOfFirstQuote = builder.Length - 1;
            builder.Append(vector);
            builder.Replace("'", "''");
            builder[indexOfFirstQuote] = '\'';
            builder.Append("'");
            return builder.ToString();
        }
    }
}
