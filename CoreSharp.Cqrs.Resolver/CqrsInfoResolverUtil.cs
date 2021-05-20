using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreSharp.Common.Attributes;

namespace CoreSharp.Cqrs.Resolver
{

    public static class CqrsInfoResolverUtil
    {

        public static IEnumerable<CqrsInfo> GetCqrsDefinitions(Assembly assembly)
        {

            var items = assembly.GetTypes().Where(x => x.IsCqrsExposed());
            var infos = items.Select(x =>
            {
                var serviceName = string.Join("_", x.FullName.Split('.').Take(x.FullName.Split('.').Length - 1));
                var methodName = x.IsQuery() ? CqrsExposeUtil.GetQueryKey(x) : CqrsExposeUtil.GetCommandKey(x);
                var formatter = CqrsExposeUtil.GetFormatter(x);
                return new CqrsInfo(x, serviceName, methodName, formatter, x.IsQuery(), x.IsCommand(), x.IsCqrsAsync(), x.IsAuthorize(), x.GetResultType(), x.GetPermissions());
            }).ToList();
            return infos;
        }

        private static bool IsCqrs(this Type type)
        {
            return CqrsContstants.CqrsInterfaces.Any(ifx => type.Implements(ifx)) && type.IsPublic && !type.IsInterface;
        }

        private static bool IsCqrsExposed(this Type type)
        {
            return type.IsCqrs() && type.GetCustomAttributes<ExposeAttribute>().Any();
        }

        private static bool IsCqrsAsync(this Type type)
        {
            return CqrsContstants.CqrsAsyncInterfaces.Any(ifx => type.Implements(ifx));
        }

        private static bool IsQuery(this Type type)
        {
            return CqrsContstants.CqrsQueryInterfaces.Any(ifx => type.Implements(ifx));
        }

        private static bool IsCommand(this Type type)
        {
            return CqrsContstants.CqrsCommandInterfaces.Any(ifx => type.Implements(ifx));
        }

        private static Type GetResultType(this Type type)
        {
            var rsp = type.GetAllInterfaces().FirstOrDefault(x => x.IsGenericType && CqrsContstants.CqrsInterfaces.Contains(x.GetGenericTypeDefinition()))
                ?.GetGenericArguments().First();
            return rsp;
        }

        private static bool Implements(this Type type, Type interfaceType)
        {
            return type.GetAllInterfaces().Any(x => (x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType) || x == interfaceType);
        }

        private static IEnumerable<string> GetPermissions(this Type type)
        {
            var permissions = type.GetCustomAttributes<AuthorizeAttribute>()
                ?.SelectMany(y => y.Permissions ?? new string[0]).Distinct().ToList() ?? new List<string>();
            return permissions;
        }

        private static bool IsAuthorize(this Type type)
        {
            var count = type.GetCustomAttributes<AuthorizeAttribute>()?.Count() ?? 0;
            return count > 0;
        }

    }
}
