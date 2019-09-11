using System.Text;
using NHibernate.Cfg;
using NHibernate.Util;

namespace CoreSharp.NHibernate.PostgreSQL
{
    public class PostgresSnakeCaseNamingStrategy : INamingStrategy
    {
        #region INamingStrategy Members

        /// <summary>
        /// Return the unqualified class name, mixed case converted to underscores
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public string ClassToTableName(string className)
        {
            return AddUnderscores(StringHelper.Unqualify(className));
        }

        /// <summary>
        /// Return the full property path with underscore separators, mixed case converted to underscores
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string PropertyToColumnName(string propertyName)
        {
            return AddUnderscores(StringHelper.Unqualify(propertyName));
        }

        /// <summary>
        /// Convert mixed case to underscores
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string TableName(string tableName)
        {
            return AddUnderscores(tableName);
        }

        /// <summary>
        /// Convert mixed case to underscores
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string ColumnName(string columnName)
        {
            return AddUnderscores(columnName);
        }

        /// <summary>
        /// Return the full property path prefixed by the unqualified class name, with underscore separators, mixed case converted to underscores
        /// </summary>
        /// <param name="className"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string PropertyToTableName(string className, string propertyName)
        {
            return AddUnderscores(StringHelper.Unqualify(propertyName));
        }

        public string LogicalColumnName(string columnName, string propertyName)
        {
            return StringHelper.IsNotEmpty(columnName) ? columnName : StringHelper.Unqualify(propertyName);
        }

        #endregion

        private string AddUnderscores(string name)
        {
            var chars = name.Replace('.', '_').ToCharArray();
            var buf = new StringBuilder(chars.Length);

            var prev = 'A';
            foreach (var c in chars)
            {
                if (c != '_' && char.IsUpper(c) && !char.IsUpper(prev) && prev != '`')
                {
                    buf.Append('_');
                }
                buf.Append(char.ToLowerInvariant(c));
                prev = c;
            }

            return buf.ToString();
        }
    }
}
