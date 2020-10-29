using System.Data;
using NpgsqlTypes;

namespace CoreSharp.NHibernate.PostgreSQL.Types
{
    public class StringArrayType : ArrayType<string>
    {
        protected override NpgSqlType GetNpgSqlType()
        {
            return new NpgSqlType(
                DbType.Object,
                NpgsqlDbType.Array | NpgsqlDbType.Text,
                "text[]"
            );
        }
    }
}
