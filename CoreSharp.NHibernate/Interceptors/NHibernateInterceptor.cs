using CoreSharp.DataAccess;
using NHibernate;
using NHibernate.Type;

namespace CoreSharp.NHibernate.Interceptors
{
    public class NHibernateInterceptor : EmptyInterceptor
    {
        public override bool? IsTransient(object entity)
        {
            if (entity is IEntity)
            {
                return ((IEntity)entity).IsTransient();
            }

            return base.IsTransient(entity);
        }

        public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            var entityState = entity as IEntityState;
            entityState?.SetTransient(false);

            return false;
        }

        public override bool OnLoad(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            var entityState = entity as IEntityState;
            entityState?.SetTransient(false);

            return false;
        }
    }
}
