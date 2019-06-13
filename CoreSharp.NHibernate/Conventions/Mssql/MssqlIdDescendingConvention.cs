using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using NHibernate.Dialect;
using CoreSharp.NHibernate.Configuration;

namespace CoreSharp.NHibernate.Conventions.Mssql
{
    public class MssqlIdDescendingConvention : ISchemaConvention
    {
        private readonly ConventionsConfiguration _configuration;
        private readonly HashSet<string> _validDialects = new HashSet<string>
            {
                typeof (MsSql2012Dialect).FullName,
                typeof (MsSql2008Dialect).FullName,
                typeof (MsSql2005Dialect).FullName
            };

        public MssqlIdDescendingConvention(ConventionsConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool CanApply(Dialect dialect)
        {
            return _configuration.IdDescending && _validDialects.Contains(dialect.GetType().FullName);
        }

        public void ApplyBeforeExecutingQuery(global::NHibernate.Cfg.Configuration config, IDbConnection connection, IDbCommand dbCommand)
        {
            var match = Regex.Match(dbCommand.CommandText, @"create[\s]+table[\s]+([\[\]\w]+)[\s\w\(,\)[\[\]]+((?:primary\s+key\s+)\(([^\)]+)\))");
            if (!match.Success) return;
            var tableName = match.Groups[1].Value.TrimEnd(']').TrimStart('[');
            var pkConstraintOld = match.Groups[2].Value;
            var columns = match.Groups[3].Value.Split(',').Select(o => string.Format("{0} DESC", o.Trim())).ToList();
            var pkConstraintNew = string.Format("CONSTRAINT {0} PRIMARY KEY ({1})", GetPrimaryKeyName(tableName), string.Join(", ", columns));
            dbCommand.CommandText = dbCommand.CommandText.Replace(pkConstraintOld, pkConstraintNew);
        }

        private static string GetPrimaryKeyName(string tableName)
        {
            return string.Format("PK_{0}", tableName);
        }

        public void ApplyAfterExecutingQuery(global::NHibernate.Cfg.Configuration config, IDbConnection connection, IDbCommand dbCommand)
        {
        }
    }
}
