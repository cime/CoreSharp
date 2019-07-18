using CoreSharp.Breeze.Internal;
using SimpleInjector;

namespace CoreSharp.Breeze
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            container.RegisterEventHandlersFromAssemblyOf<BreezeMetadataValidators>();
        }
    }
}
