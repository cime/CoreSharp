using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SimpleInjector;

namespace CoreSharp.Cqrs.Events
{

    public class EventAggregator : IEventPublisher, IEventSubscriber
    {
        private delegate void InvokeHandle(object obj, IEvent @event);
        private delegate Task InvokeHandleAsync(object obj, IAsyncEvent @event, CancellationToken token);

        private readonly Container _container;
        private readonly ConcurrentDictionary<Type, object> _eventHandlerInvokers = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<object, DelegateEventHandler> _delegateEventHandlers =
            new ConcurrentDictionary<object, DelegateEventHandler>();

        public EventAggregator(Container container)
        {
            _container = container;
        }

        public virtual void Publish(IEvent e)
        {
            var eventType = e.GetType();
            var handlerType = typeof(IEventHandler<>).MakeGenericType(e.GetType());
            var handle = (InvokeHandle)_eventHandlerInvokers.GetOrAdd(handlerType, t => CreateHandlerInvoker<InvokeHandle>(eventType, handlerType, "Handle"));

            foreach (var obj in GetInstances(handlerType))
            {
                handle(obj, e);
            }
        }

        public virtual Task PublishAsync(IAsyncEvent e)
        {
            return PublishAsync(e, CancellationToken.None);
        }

        public virtual async Task PublishAsync(IAsyncEvent e, CancellationToken cancellationToken)
        {
            var eventType = e.GetType();
            var handlerType = typeof(IAsyncEventHandler<>).MakeGenericType(e.GetType());
            var handleAsync = (InvokeHandleAsync)_eventHandlerInvokers.GetOrAdd(handlerType, t => CreateHandlerInvoker<InvokeHandleAsync>(eventType, handlerType, "HandleAsync", Expression.Parameter(typeof(CancellationToken), "token")));

            foreach (var obj in GetInstances(handlerType))
            {
                await handleAsync(obj, e, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        public void Subscribe<TEvent>(EventHandler<TEvent> handlerDelegate, short priority = default(short)) where TEvent : IEvent
        {
            var handler = new DelegateEventHandler<TEvent>(handlerDelegate, priority);

            _delegateEventHandlers.AddOrUpdate(handlerDelegate, handler, (k, v) => handler);
        }

        public bool Unsubscribe<TEvent>(EventHandler<TEvent> handlerDelegate) where TEvent : IEvent
        {
            DelegateEventHandler handler;

            return _delegateEventHandlers.TryRemove(handlerDelegate, out handler);
        }

        public void Subscribe<TEvent>(AsyncEventHandler<TEvent> handlerDelegate, short priority = default(short)) where TEvent : IAsyncEvent
        {
            var handler = new AsyncDelegateEventHandler<TEvent>(handlerDelegate, priority);

            _delegateEventHandlers.AddOrUpdate(handlerDelegate, handler, (k, v) => handler);
        }

        public bool Unsubscribe<TEvent>(AsyncEventHandler<TEvent> handlerDelegate) where TEvent : IAsyncEvent
        {
            DelegateEventHandler handler;

            return _delegateEventHandlers.TryRemove(handlerDelegate, out handler);
        }

        private IEnumerable<object> GetInstances(Type t)
        {
            return (IEnumerable<object>)_container
                .TryGetAllInstances(t)
                .Select(o => new
                {
                    Instance = o,
                    Priority = o.GetType().GetPriority()
                })
                .Union(
                    _delegateEventHandlers.Values
                    .Where(t.IsInstanceOfType)
                    .Select(o => new
                    {
                        Instance = (object)o,
                        o.Priority
                    })
                )
                .OrderByDescending(o => o.Priority)
                .Select(o => o.Instance);
        }

        private static TResult CreateHandlerInvoker<TResult>(Type eventType, Type handlerType, string methodName, params ParameterExpression[] parameters)
        {
            var param1 = Expression.Parameter(typeof(object), "handler");
            var param2 = Expression.Parameter(typeof(object), "obj");
            var convertHandler = Expression.Convert(param1, handlerType);
            var convertCommand = Expression.Convert(param2, eventType);
            var callParams = new List<Expression>
            {
                convertCommand
            }.Concat(parameters);
            var lambdaParams = new List<ParameterExpression>
            {
                param1,
                param2
            }.Concat(parameters);
            var callMethod = Expression.Call(convertHandler, handlerType.GetMethod(methodName), callParams);

            return Expression.Lambda<TResult>(callMethod, lambdaParams).Compile();
        }
    }
}
