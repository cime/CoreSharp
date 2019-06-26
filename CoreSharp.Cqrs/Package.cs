using CoreSharp.Cqrs.Command;
using CoreSharp.Cqrs.Events;
using CoreSharp.Cqrs.Query;
using SimpleInjector;

namespace CoreSharp.Cqrs
{
    public class Package : IPackage
    {
        /// <summary>
        /// Registers IQueryProcessor, ICommandDispatcher, EventAggregator, IEventPublisher and IEventSubscriber
        /// </summary>
        /// <param name="container">SimpleInjector container</param>
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
