using System.Data;
using NpgsqlTypes;

namespace CoreSharp.NHibernate.PostgreSQL.Types
{
    public class Int16ArrayType : ArrayType<short>
    {
        protected override NpgSqlType GetNpgSqlType() => new NpgSqlType(
            DbType.Object,
            NpgsqlDbType.Array | NpgsqlDbType.Smallint,
            "int2[]"
        );
    }
}
