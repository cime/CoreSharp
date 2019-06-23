using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SimpleInjector;

namespace CoreSharp.Cqrs.Command
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private delegate void InvokeHandle(object obj, ICommand command);
        private delegate T InvokeHandle<out T>(object obj, object command);
        private delegate Task InvokeHandleAsync(object obj, object command, CancellationToken token);
        private delegate Task<T> InvokeHandleAsync<T>(object obj, object command, CancellationToken token);

        private readonly Container _container;
        private static readonly ConcurrentDictionary<Type, object> CommandHandlerInvokers = new ConcurrentDictionary<Type, object>();

        public CommandDispatcher(Container container)
        {
            _container = container;
        }

        public virtual void Dispatch(ICommand command)
        {
            var cmdType = command.GetType();
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(cmdType);
            var handler = _container.GetInstance(handlerType);
            var handle = (InvokeHandle)CommandHandlerInvokers.GetOrAdd(handlerType, t =>
                CreateHandlerInvoker<InvokeHandle>(
                    cmdType, handlerType, "Handle"));
            handle(handler, command);
        }

        public TResult Dispatch<TResult>(ICommand<TResult> command)
        {
            var cmdType = command.GetType();
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(cmdType, typeof(TResult));
            var handler = _container.GetInstance(handlerType);
            var handle = (InvokeHandle<TResult>)CommandHandlerInvokers.GetOrAdd(handlerType, t =>
               CreateHandlerInvoker<InvokeHandle<TResult>>(
                   cmdType, handlerType, "Handle"));
            return handle(handler, command);
        }

        public virtual Task DispatchAsync(IAsyncCommand command)
        {
            return DispatchAsync(command, CancellationToken.None);
        }

        public Task<TResult> DispatchAsync<TResult>(IAsyncCommand<TResult> command)
        {
            return DispatchAsync(command, CancellationToken.None);
        }

        public Task DispatchAsync(IAsyncCommand command, CancellationToken cancellationToken)
        {
            var cmdType = command.GetType();
            var handlerType = typeof(IAsyncCommandHandler<>).MakeGenericType(cmdType);
            var handler = _container.GetInstance(handlerType);
            var handle = (InvokeHandleAsync)CommandHandlerInvokers.GetOrAdd(handlerType, t =>
                CreateHandlerInvoker<InvokeHandleAsync>(
                    cmdType, handlerType, "HandleAsync",
                    Expression.Parameter(typeof(CancellationToken), "token")));
            return handle(handler, command, cancellationToken);
        }

        public virtual Task<TResult> DispatchAsync<TResult>(IAsyncCommand<TResult> command, CancellationToken cancellationToken)
        {
            var cmdType = command.GetType();
            var handlerType = typeof(IAsyncCommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
            var handler = _container.GetInstance(handlerType);
            var handle = (InvokeHandleAsync<TResult>)CommandHandlerInvokers.GetOrAdd(handlerType, t =>
               CreateHandlerInvoker<InvokeHandleAsync<TResult>>(
                   cmdType, handlerType, "HandleAsync",
                   Expression.Parameter(typeof(CancellationToken), "token")));
            return handle(handler, command, cancellationToken);
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
