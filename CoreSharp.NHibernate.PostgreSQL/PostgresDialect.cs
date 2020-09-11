using CoreSharp.NHibernate.PostgreSQL.Types;
using NHibernate.Dialect;
using NHibernate.SqlTypes;

namespace CoreSharp.NHibernate.PostgreSQL
{
    public class PostgresDialect : PostgreSQL83Dialect
    {
        public override string GetTypeName(SqlType sqlType)
        {
            if (sqlType is NpgSqlType npgSqlType)
            {
                return npgSqlType.TypeName;
            }

            return base.GetTypeName(sqlType);
        }
    }
}
