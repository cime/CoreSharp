using System;
using System.Linq;
using System.Reflection;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace CoreSharp.NHibernate.Alternations
{
    public class AutomappingOverridesAlternation : IAutoMappingAlteration
    {
        private readonly Assembly assembly;

        /// <summary>
        /// Constructor for AutoMappingOverrideAlteration.
        /// </summary>
        /// <param name="overrideAssembly">Assembly to load overrides from.</param>
        public AutomappingOverridesAlternation(Assembly overrideAssembly)
        {
            assembly = overrideAssembly;
        }

        /// <summary>
        /// Alter the model
        /// </summary>
        /// <remarks>
        /// Finds all types in the assembly (passed in the constructor) that implement IAutoMappingOverride&lt;T&gt;, then
        /// creates an AutoMapping&lt;T&gt; and applies the override to it.
        /// </remarks>
        /// <param name="model">AutoPersistenceModel instance to alter</param>
        public void Alter(CustomAutoPersistenceModel model)
        {
            // find all types deriving from IAutoMappingOverride<T>
            var types = from type in assembly.GetExportedTypes()
                where !type.IsAbstract
                let entity = (from interfaceType in type.GetInterfaces()
                    where interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IAutoMappingOverride<>)
                    select interfaceType.GetGenericArguments()[0]).FirstOrDefault()
                where entity != null
                select type;

            foreach (var type in types)
            {
                model.Override(type);
            }
        }

        public void Alter(AutoPersistenceModel model)
        {
            if (model is CustomAutoPersistenceModel customAutoPersistenceModel)
            {
                Alter(customAutoPersistenceModel);
            }
            else
            {
                throw new InvalidOperationException("Instance must be of type CustomAutoPersistenceModel");
            }
        }
    }
}
