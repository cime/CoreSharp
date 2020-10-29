using Newtonsoft.Json;
using NHibernate.Engine;
using NHibernate.Type;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace CoreSharp.NHibernate.PostgreSQL.Types
{
[Serializable]
    public class JsonbType<TSerializable> : IUserType where TSerializable : class
    {
        private readonly Type _serializableClass;

        public JsonbType()
        {
            _serializableClass = typeof(TSerializable);
        }

        private static string Serialize(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch (Exception e)
            {
                throw new SerializationException("Could not serialize a serializable property: ", e);
            }
        }

        private object Deserialize(string dbValue)
        {
            try
            {
                return JsonConvert.DeserializeObject(dbValue, _serializableClass);
            }
            catch (Exception e)
            {
                throw new SerializationException("Could not deserialize a serializable property: ", e);
            }
        }

        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null | y == null)
            {
                return false;
            }

            if (IsDictionary(x) && IsDictionary(y))
            {
                return EqualDictionary(x, y);
            }

            return x.Equals(y);
        }

        private static bool EqualDictionary(object x, object y)
        {
            var a = x as IDictionary;
            var b = y as IDictionary;

            if (a.Count != b.Count) return false;

            foreach (var key in a.Keys)
            {
                if (!b.Contains(key)) return false;

                var va = a[key];
                var vb = b[key];

                if (!va.Equals(vb)) return false;
            }

            return true;
        }

        private static bool IsDictionary(object o)
        {
            return typeof(IDictionary).IsAssignableFrom(o.GetType());
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            if (names.Length != 1)
                throw new InvalidOperationException($"One column name expected but received {names.Length}");

            if (rs[names[0]] is string value && !string.IsNullOrWhiteSpace(value))
                return Deserialize(value);

            return null;
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            var parameter = cmd.Parameters[index];

            if (parameter is NpgsqlParameter)
                parameter.DbType = SqlTypes[0].DbType;

            if (value == null)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = value;
        }

        public object DeepCopy(object value)
        {
            return Deserialize(Serialize(value));
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            return (cached == null) ? null : Deserialize((string) cached);
        }

        public object Disassemble(object value)
        {
            return (value == null) ? null : Serialize(value);
        }

        public SqlType[] SqlTypes => new[] {new NpgSqlType(DbType.Object, NpgsqlDbType.Jsonb, "jsonb")};
        public Type ReturnedType => _serializableClass;
        public bool IsMutable => true;
    }
}
