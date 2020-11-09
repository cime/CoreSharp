using CoreSharp.DataAccess;
using CoreSharp.DataAccess.UnitOfWork;
using CoreSharp.NHibernate.Decorators;
using CoreSharp.NHibernate.DeepClone;
using CoreSharp.NHibernate.Store;
using CoreSharp.NHibernate.UnitOfWork;
using CoreSharp.NHibernate.Visitors;
using NHibernate;
using SimpleInjector;

namespace CoreSharp.NHibernate
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            container.Register<IDbStore, DbStore>(Lifestyle.Scoped);
            container.Register(typeof(IDbStore<>), typeof(DbStore<>), Lifestyle.Scoped);

            container.Register<IDeepCloner, DeepCloner>(Lifestyle.Scoped);

            container.RegisterSingleton<IMappingsValidator, MappingsValidator>();

            //container.RegisterDecorator<ISessionFactory, SessionFactoryDecorator>(Lifestyle.Singleton);
            container.RegisterDecorator<ISession, SessionDecorator>(Lifestyle.Scoped);

            container.RegisterSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
        }
    }
}
