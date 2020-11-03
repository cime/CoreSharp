using System;
using NHibernate.Intercept;
using NHibernate.Proxy;

namespace CoreSharp.NHibernate.Extensions
{
    public static class ObjectExtensions
    {
        public static Type GetEntityType(this object entity, bool allowInitialization = true)
        {
            if (entity is INHibernateProxy nhProxy)
            {
                if (nhProxy.HibernateLazyInitializer.IsUninitialized && !allowInitialization)
                {
                    return nhProxy.HibernateLazyInitializer.PersistentClass;
                }

                // We have to initialize in case of a subclass to get the concrete type
                entity = nhProxy.HibernateLazyInitializer.GetImplementation();
            }

            switch (entity)
            {
                case IFieldInterceptorAccessor interceptorAccessor:
                    return interceptorAccessor.FieldInterceptor.MappedClass;
                default:
                    return entity.GetType();
            }
        }
    }
}
