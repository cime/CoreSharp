using System.Collections.Generic;
using System.Reflection;
using FluentNHibernate.Automapping;

// ReSharper disable once CheckNamespace
namespace NHibernate
{
    public static class AutoPersistenceModelModelExtensions
    {
        private static readonly MethodInfo AddFilterMethodInfo;

        static AutoPersistenceModelModelExtensions()
        {
            AddFilterMethodInfo = typeof(AutoPersistenceModel).GetMethod("AddFilter");
        }

        public static AutoPersistenceModel UseOverridesFromAssemblies(this AutoPersistenceModel model, IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                model.UseOverridesFromAssembly(assembly);
            }
            return model;
        }

        public static AutoPersistenceModel AddConventions(this AutoPersistenceModel model, IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                model.Conventions.AddAssembly(assembly);
            }
            return model;
        }

        public static AutoPersistenceModel AddFilters(this AutoPersistenceModel model, IEnumerable<System.Type> types)
        {
            foreach (var type in types)
            {
                AddFilterMethodInfo.MakeGenericMethod(type).Invoke(model, null);
            }
            return model;
        }
    }
}
