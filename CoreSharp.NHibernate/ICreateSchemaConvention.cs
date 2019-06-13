using System.Data;

namespace CoreSharp.NHibernate
{
    public interface ICreateSchemaConvention : ISchemaConvention
    {
        void ApplyBeforeSchemaCreate(global::NHibernate.Cfg.Configuration config, IDbConnection connection);

        void ApplyAfterSchemaCreate(global::NHibernate.Cfg.Configuration config, IDbConnection connection);
    }
}
