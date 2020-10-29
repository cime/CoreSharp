using System.Data;
using NpgsqlTypes;

namespace CoreSharp.NHibernate.PostgreSQL.Types
{
    public class CitextArrayType : ArrayType<string>
    {
        protected override NpgSqlType GetNpgSqlType()
        {
            return new NpgSqlType(
                DbType.Object,
                NpgsqlDbType.Array | NpgsqlDbType.Text,
                "citext[]"
            );
        }
    }
}
