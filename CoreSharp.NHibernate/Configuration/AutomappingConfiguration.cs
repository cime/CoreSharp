using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Steps;
using FluentNHibernate.Conventions;
using Microsoft.Extensions.Configuration;

namespace CoreSharp.NHibernate.Configuration
{
    public class AutomappingConfiguration : DefaultAutomappingConfiguration
    {
        private readonly IConfiguration _configuration;
        private readonly List<Assembly> _mappingStepsAssembiles;

        public AutomappingConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
            _mappingStepsAssembiles = new List<Assembly>();
        }

        public AutomappingConfiguration AddStepAssembly(Assembly assembly)
        {
            _mappingStepsAssembiles.Add(assembly);
            return this;
        }

        public AutomappingConfiguration AddStepAssemblies(IEnumerable<Assembly> assemblies)
        {
            _mappingStepsAssembiles.AddRange(assemblies);
            return this;
        }

        public override bool ShouldMap(Type type)
        {
            if (type.GetCustomAttribute<IncludeAttribute>() != null)
            {
                return true;
            }

            return base.ShouldMap(type) && typeof(IEntity).IsAssignableFrom(type) && type.GetCustomAttribute<IgnoreAttribute>(false) == null;
        }

        public override bool AbstractClassIsLayerSupertype(Type type)
        {
            return type.GetCustomAttribute<IncludeAttribute>() == null;
        }

        public override bool ShouldMap(Member member)
        {
            if (member.IsProperty)
            {
                var propInfo = (PropertyInfo)member.MemberInfo;
                if (propInfo.GetCustomAttribute<IgnoreAttribute>() != null)
                {
                    return false;
                }

                if (propInfo.GetCustomAttribute<IncludeAttribute>() != null)
                {
                    return true;
                }

                if (propInfo.PropertyType.Namespace == "GeoAPI.Geometries")
                {
                    return true;
                }

                var getMethod = propInfo.GetGetMethod(true);

                if (getMethod.IsFamilyOrAssembly && !getMethod.IsFamily && !getMethod.IsAssembly) //true for protected internal properties - .NET BUG?
                {
                    return member.CanWrite;
                }
            }

            return member.CanWrite && base.ShouldMap(member);
        }

        public override IEnumerable<IAutomappingStep> GetMappingSteps(AutoMapper mapper, IConventionFinder conventionFinder)
        {
            var steps = GetAdditionalMappingSteps();
            steps.AddRange(base.GetMappingSteps(mapper, conventionFinder));

            return steps;
        }

        private List<IAutomappingStep> GetAdditionalMappingSteps()
        {
            var stepTypes = _mappingStepsAssembiles.SelectMany(o => o.GetTypes().Where(t => typeof(IAutomappingStep).IsAssignableFrom(t)));
            var result = new List<IAutomappingStep>();

            foreach (var constuctor in stepTypes.Select(stepType => stepType.GetConstructor(new[] { typeof(IAutomappingConfiguration) })))
            {
                if (constuctor == null)
                {
                    throw new NullReferenceException("constuctor");
                }

                var mappingStep = (IAutomappingStep)constuctor.Invoke(new object[] { this });
                result.Add(mappingStep);
            }

            return result;
        }
    }
}
