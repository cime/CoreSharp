using System.Data;
using NpgsqlTypes;

namespace CoreSharp.NHibernate.PostgreSQL.Types
{
    public class FloatArrayType : ArrayType<float>
    {
        protected override NpgSqlType GetNpgSqlType() => new NpgSqlType(
            DbType.Object,
            NpgsqlDbType.Array | NpgsqlDbType.Real,
            "float4[]"
        );
    }
}
