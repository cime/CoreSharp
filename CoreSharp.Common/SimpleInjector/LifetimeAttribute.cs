using System;

// ReSharper disable once CheckNamespace
namespace SimpleInjector
{
    /// <summary>
    /// Defines a life time for a type. Currently only supported for <see cref="IEventHandler{TEvent}"/>, <see cref="IAsyncEventHandler{TEvent}"></see>
    /// <see cref="ICommandHandler{TCommand}"/> and <see cref="IAsyncCommandHandler{TCommand}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class LifetimeAttribute : Attribute
    {
        public Lifetime Lifetime { get; }

        public LifetimeAttribute(Lifetime lifetime)
        {
            Lifetime = lifetime;
        }
    }
}
