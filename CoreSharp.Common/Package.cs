using CoreSharp.Common.Command;
using CoreSharp.Common.Events;
using CoreSharp.Common.Query;
using SimpleInjector;

namespace CoreSharp.Common
{
    public class Package : IPackage
    {
        public void Register(Container container)
        {
            // Query system
            container.RegisterSingleton<IQueryProcessor, DefaultQueryProcessor>();

            // Command system
            container.RegisterSingleton<ICommandDispatcher, CommandDispatcher>();

            // Events
            var registration = Lifestyle.Singleton.CreateRegistration<EventAggregator>(container);
            container.AddRegistration(typeof(EventAggregator), registration);
            container.AddRegistration(typeof(IEventPublisher), registration);
            container.AddRegistration(typeof(IEventSubscriber), registration);
        }
    }
}
