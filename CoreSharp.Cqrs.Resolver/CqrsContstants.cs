using System;
using System.Collections.Generic;
using CoreSharp.Cqrs.Command;
using CoreSharp.Cqrs.Query;

namespace CoreSharp.Cqrs.Resolver
{
    internal static class CqrsContstants
    {
        public static List<Type> CqrsInterfaces = new List<Type> {
            typeof(IAsyncQuery<>),
            typeof(IQuery<>),
            typeof(IAsyncCommand<>),
            typeof(ICommand<>),
            typeof(IAsyncCommand),
            typeof(ICommand)
        };

        public static List<Type> CqrsAsyncInterfaces = new List<Type> {
            typeof(IAsyncQuery<>),
            typeof(IAsyncCommand<>),
            typeof(IAsyncCommand),
        };

        public static List<Type> CqrsQueryInterfaces = new List<Type> {
            typeof(IAsyncQuery<>),
            typeof(IQuery<>),
        };

        public static List<Type> CqrsCommandInterfaces = new List<Type> {
            typeof(IAsyncCommand<>),
            typeof(ICommand<>),
            typeof(IAsyncCommand),
            typeof(ICommand)
        };
    }
}
