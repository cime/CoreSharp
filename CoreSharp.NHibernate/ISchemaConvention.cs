using System.Data;
using FluentNHibernate.Conventions;
using NHibernate.Dialect;

namespace CoreSharp.NHibernate
{
    public interface ISchemaConvention : IConvention
    {
        bool CanApply(Dialect dialect);

        void ApplyBeforeExecutingQuery(global::NHibernate.Cfg.Configuration config, IDbConnection connection, IDbCommand dbCommand);

        void ApplyAfterExecutingQuery(global::NHibernate.Cfg.Configuration config, IDbConnection connection, IDbCommand dbCommand);
    }
}
