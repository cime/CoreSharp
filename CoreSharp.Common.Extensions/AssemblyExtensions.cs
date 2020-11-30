using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CoreSharp.Common.Extensions
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Get all assemblies that depends on <paramref name="analyzedAssembly"/>
        /// </summary>
        /// <param name="analyzedAssembly">Analyzed Assembly</param>
        /// <returns>List of assemblies that depends on <paramref name="analyzedAssembly"/></returns>
        public static IEnumerable<Assembly> GetDependentAssemblies(this Assembly analyzedAssembly)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic)
                .Where(a => GetNamesOfAssembliesReferencedBy(a)
                    .Contains(analyzedAssembly.GetName().Name));
        }

        private static IEnumerable<string> GetNamesOfAssembliesReferencedBy(Assembly assembly)
        {
            return assembly.GetReferencedAssemblies()
                .Select(assemblyName => new AssemblyName(assemblyName.FullName).Name);
        }

        public static IDictionary<Type, List<T>> GetTypesAttributes<T>(this Assembly assembly)
            where T : Attribute
        {
            var attrs = assembly.GetTypes().Where(t => t.GetCustomAttributes().Any(a => a.GetType() == typeof(T)))
                .ToDictionary(t => t, t => t.GetCustomAttributes().Where(a => a.GetType() == typeof(T)).Select(a => (T)a).ToList());
            return attrs;
        }

        public static IDictionary<Type, List<T>> GetTypesAttributes<T>(params Type[] typesDefAssemlies)
            where T : Attribute
        {
            var attrs = typesDefAssemlies.Distinct().SelectMany(t => t.Assembly.GetTypesAttributes<T>())
                .ToDictionary(p => p.Key, p => p.Value);
            return attrs;
        }

        public static IEnumerable<Assembly> ToAssemblies(this IEnumerable<string> assembliesNames)
        {

            var assemblies = new List<Assembly>();
            if (assembliesNames != null || assembliesNames.Any())
            {
                assemblies = assembliesNames
                    .Select(a => {
                        var assemlby = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == a);
                        if (assemlby != null)
                        {
                            return assemlby;
                        }
                        assemlby = Assembly.Load(a);
                        return assemlby;
                    })
                    .Where(x => x != null).ToList();
            }
            return assemblies;
        }

    }
}
