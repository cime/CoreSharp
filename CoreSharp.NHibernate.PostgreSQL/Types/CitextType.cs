using System;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;
using Npgsql;
using NpgsqlTypes;

namespace CoreSharp.NHibernate.PostgreSQL.Types
{
    [Serializable]
    public class CitextType : AbstractStringType
    {
        public override string Name => "Citext";

        public CitextType() : base(new StringSqlType())
        {

        }

        public CitextType(SqlType sqlType) : base(sqlType)
        {
        }

        public override void Set(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            if (cmd.Parameters[index] is NpgsqlParameter param)
            {
                param.NpgsqlDbType = NpgsqlDbType.Citext;
            }

            base.Set(cmd, value, index, session);
        }
    }
}
