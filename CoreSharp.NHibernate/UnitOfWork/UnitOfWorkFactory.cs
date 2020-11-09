using System.Data;
using CoreSharp.DataAccess.UnitOfWork;
using SimpleInjector;

namespace CoreSharp.NHibernate.UnitOfWork
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly Container _container;

        public UnitOfWorkFactory(Container container)
        {
            _container = container;
        }

        public IUnitOfWork Create(IsolationLevel isolationLevel)
        {
            return new UnitOfWork(_container, isolationLevel);
        }
    }
}
