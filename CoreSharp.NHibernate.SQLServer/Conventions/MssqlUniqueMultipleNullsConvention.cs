using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CoreSharp.NHibernate.Configuration;
using NHibernate.Dialect;

namespace CoreSharp.NHibernate.SQLServer.Conventions
{
    /// <summary>
    /// Create a unique constraint that allows multiple NULL values. Applies to Mssql 2008 and above
    /// This only apply to nullable unique columns
    /// http://stackoverflow.com/questions/767657/how-do-i-create-unique-constraint-that-also-allows-nulls-in-sql-server
    /// </summary>
    public class MssqlUniqueMultipleNullsConvention : ISchemaConvention
    {
        private readonly ConventionsConfiguration _configuration;

        private readonly HashSet<string> _validDialects = new HashSet<string>
            {
                typeof (MsSql2012Dialect).FullName,
                typeof (MsSql2008Dialect).FullName
            };

        public MssqlUniqueMultipleNullsConvention(ConventionsConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool CanApply(Dialect dialect)
        {
            return _configuration.UniqueWithMultipleNulls && _validDialects.Contains(dialect.GetType().FullName);
        }

        public void ApplyBeforeExecutingQuery(global::NHibernate.Cfg.Configuration config, IDbConnection connection, IDbCommand dbCommand)
        {
            var tableMatch = Regex.Match(dbCommand.CommandText, @"create\s+table\s+([\[\]\w_]+)");
            if(!tableMatch.Success) return;
            var tableName = tableMatch.Groups[1].Value.TrimStart('[').TrimEnd(']');
            var matches = Regex.Matches(dbCommand.CommandText,
                                        @"(([\[\]\w_]+)\s+([\w\(\)]+)\s+(not null|null) unique)|(unique\s+\(([^\)]+))\)");
            if (matches.Count == 0) return;
            var script = new StringBuilder();
            script.AppendLine();
            foreach (var match in matches.Cast<Match>().Where(match => match.Success))
            {
                string uniqueKeySql;
                if (string.IsNullOrEmpty(match.Groups[2].Value)) //named unique key
                {
                    var columns = match.Groups[6].Value.Split(',').Select(o => o.Trim()).ToList();
                    uniqueKeySql =
                        $"CONSTRAINT {GetUniqueKeyName(tableName, columns)} UNIQUE ({string.Join(", ", columns)})";
                    dbCommand.CommandText = dbCommand.CommandText.Replace(match.Groups[0].Value, uniqueKeySql);

                }
                else
                {
                    var column = match.Groups[2].Value;
                    uniqueKeySql = match.Groups[0].Value.Replace("unique", "");
                    dbCommand.CommandText = dbCommand.CommandText.Replace(match.Groups[0].Value, uniqueKeySql);

                    if (match.Groups[4].Value == "null") //create filtered unique index
                    {
                        script.AppendFormat("CREATE UNIQUE NONCLUSTERED INDEX {0} ON {1}({2}) WHERE {2} IS NOT NULL",
                                            GetUniqueKeyName(tableName, column), tableName, column);
                        script.AppendLine();
                    }
                    else
                    {
                        dbCommand.CommandText = dbCommand.CommandText.Remove(dbCommand.CommandText.LastIndexOf(')'), 1);
                        dbCommand.CommandText +=
                            $",CONSTRAINT {GetUniqueKeyName(tableName, column)} UNIQUE ({column}))";
                    }

                }
            }
            dbCommand.CommandText += script.ToString();
        }

        private static string GetUniqueKeyName(string tableName, string columnName)
        {
            return GetUniqueKeyName(tableName, new List<string> {columnName});
        }

        private static string GetUniqueKeyName(string tableName, IEnumerable<string> columnNames)
        {
            return $"UQ_{tableName}_{string.Join("_", columnNames.Select(o => o.TrimEnd(']').TrimStart('[')))}";
        }

        public void ApplyAfterExecutingQuery(global::NHibernate.Cfg.Configuration config, IDbConnection connection, IDbCommand dbCommand)
        {
        }
    }
}
