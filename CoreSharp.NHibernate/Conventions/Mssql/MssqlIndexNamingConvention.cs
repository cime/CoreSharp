using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using NHibernate.Dialect;
using CoreSharp.NHibernate.Configuration;

namespace CoreSharp.NHibernate.Conventions.Mssql
{
    public class MssqlIndexNamingConvention : ISchemaConvention
    {
        private readonly ConventionsConfiguration _configuration;

        private readonly HashSet<string> _validDialects = new HashSet<string>
            {
                typeof (MsSql2012Dialect).FullName,
                typeof (MsSql2008Dialect).FullName
            };

        public MssqlIndexNamingConvention(ConventionsConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool CanApply(Dialect dialect)
        {
            return _configuration.UniqueWithMultipleNulls && _validDialects.Contains(dialect.GetType().FullName);
        }

        public void ApplyBeforeExecutingQuery(global::NHibernate.Cfg.Configuration config, IDbConnection connection, IDbCommand dbCommand)
        {
            var indexMatch = Regex.Match(dbCommand.CommandText, @"create\s+index\s+([\w\d]+)\s+on\s+([\w\d\[\]]+)\s+\(([\w\d\s\[\],]+)\)");
            if (!indexMatch.Success)
            {
                return;
            }

            var tableName = indexMatch.Groups[2].Value.TrimStart('[').TrimEnd(']');
            var columns = indexMatch.Groups[3].Value.Split(',').Select(o => o.Trim()).ToList();
            var key = GetUniqueKeyName(tableName, columns);
            dbCommand.CommandText = dbCommand.CommandText.Replace(indexMatch.Groups[1].Value, key);
        }

        public void ApplyAfterExecutingQuery(global::NHibernate.Cfg.Configuration config, IDbConnection connection, IDbCommand dbCommand)
        {
        }

        private static string GetUniqueKeyName(string tableName, IEnumerable<string> columnNames)
        {
            return $"IX_{tableName}_{string.Join("_", columnNames.Select(o => o.TrimEnd(']').TrimStart('[')))}";
        }
    }
}
