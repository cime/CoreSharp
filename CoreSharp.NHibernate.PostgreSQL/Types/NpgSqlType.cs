using System;
using System.Data;
using NHibernate.SqlTypes;
using NpgsqlTypes;

namespace CoreSharp.NHibernate.PostgreSQL.Types
{
    public class NpgSqlType : SqlType
    {
        public string TypeName { get; }
        public NpgsqlDbType NpgDbType { get; }

        public NpgSqlType(DbType dbType, NpgsqlDbType npgDbType, string typeName)
            : base(dbType)
        {
            NpgDbType = npgDbType;
            TypeName = typeName;
        }

        public NpgSqlType(DbType dbType, NpgsqlDbType npgDbType, int length, string typeName)
            : base(dbType, length)
        {
            NpgDbType = npgDbType;
            TypeName = typeName;
        }

        public NpgSqlType(DbType dbType, NpgsqlDbType npgDbType, byte precision, byte scale, string typeName)
            : base(dbType, precision, scale)
        {
            NpgDbType = npgDbType;
            TypeName = typeName;
        }
    }
}
