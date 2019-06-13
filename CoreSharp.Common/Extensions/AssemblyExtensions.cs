using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StandardSharp.Common.Extensions
{
    public static class AssemblyExtensions
    {
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
    }
}
