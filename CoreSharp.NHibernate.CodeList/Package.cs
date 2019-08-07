using SimpleInjector;

namespace CoreSharp.NHibernate.CodeList
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            container.RegisterEventHandlersFromAssemblyOf<Package>();
        }
    }
}
