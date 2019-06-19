using GraphQL.Types;
using SimpleInjector;

namespace CoreSharp.GraphQL
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            container.Register(() => new CqrsSchema(container), Lifestyle.Singleton);
        }
    }
}
