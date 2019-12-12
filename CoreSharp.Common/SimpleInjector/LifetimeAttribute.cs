using System;
using System.Diagnostics;
using CoreSharp.Common.Exceptions;

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

        public Lifestyle Lifestyle
        {
            get
            {
                switch (Lifetime)
                {
                    case Lifetime.Singleton:
                        return Lifestyle.Singleton;
                    case Lifetime.Scoped:
                        return Lifestyle.Scoped;
                    case Lifetime.Transient:
                        return Lifestyle.Transient;
                    default:
                        throw new CoreSharpException($"Invalid {nameof(Lifetime)} value: {Lifetime}");
                }
            }
        }
    }
}
