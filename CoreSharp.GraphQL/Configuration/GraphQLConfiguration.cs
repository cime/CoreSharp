using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSharp.GraphQL.Configuration
{
    public class GraphQLConfiguration : IGraphQLConfiguration
    {
        protected Dictionary<Type, ITypeConfiguration> ModelsConfiguration = new Dictionary<Type, ITypeConfiguration>();
        private bool _isLocked = false;

        public static event Action<ITypeConfiguration> ModelConfigurationCreated;

        internal static void OnModelConfigurationCreated(ITypeConfiguration obj)
        {
            var handler = ModelConfigurationCreated;
            handler?.Invoke(obj);
        }

        public ITypeConfiguration GetModelConfiguration(Type modelType)
        {
            if (!_isLocked)
            {
                _isLocked = true;
            }
            var mergedConfig = (TypeConfiguration)Activator.CreateInstance(typeof(TypeConfiguration<>).MakeGenericType(modelType));
            //For interfaces we want to match only interfaces that are assignable from modelType
            var modelConfigs = modelType.IsInterface
                ? ModelsConfiguration.Where(o => o.Key.IsInterface && o.Key.IsAssignableFrom(modelType)).ToList()
                : ModelsConfiguration.Where(o => o.Key.IsAssignableFrom(modelType)).ToList();
            //subclasses have higher priority
            modelConfigs.Sort((pair, valuePair) => pair.Key.IsAssignableFrom(valuePair.Key) ? -1 : 1);
            foreach (var modelConfig in modelConfigs.Select(o => o.Value))
            {
                mergedConfig.ModelType = modelConfig.ModelType;
                mergedConfig.FieldName = modelConfig.FieldName;
                foreach (var data in modelConfig.Data)
                {
                    mergedConfig.Data[data.Key] = data.Value;
                }
                foreach (var pair in modelConfig.MemberConfigurations)
                {
                    var mergedMember = mergedConfig.MemberConfigurations.ContainsKey(pair.Key)
                        ? mergedConfig.MemberConfigurations[pair.Key]
                        : null;

                    mergedMember = MergeMemberConfiguration(pair.Value, mergedMember);
                    mergedConfig.MemberConfigurations[pair.Key] = mergedMember;
                }
            }
            return mergedConfig;
        }

        public ITypeConfiguration GetModelConfiguration<TModel>() where TModel : class
        {
            return GetModelConfiguration(typeof (TModel));
        }

        public ITypeConfiguration<TModel> Configure<TModel>()
        {
            ThrowIfLocked();
            var type = typeof(TModel);
            if (ModelsConfiguration.ContainsKey(type))
                return (ITypeConfiguration<TModel>)ModelsConfiguration[type];
            var modelConfig = new TypeConfiguration<TModel>();
            ModelsConfiguration[type] = modelConfig;
            OnModelConfigurationCreated(modelConfig);
            return modelConfig;
        }

        public ITypeConfiguration Configure(Type type)
        {
            ThrowIfLocked();
            if (ModelsConfiguration.ContainsKey(type))
            {
                return ModelsConfiguration[type];
            }

            var modelConfig = (TypeConfiguration)Activator.CreateInstance(typeof(TypeConfiguration<>).MakeGenericType(type));
            ModelsConfiguration[type] = modelConfig;
            OnModelConfigurationCreated(modelConfig);

            return modelConfig;
        }

        private IFieldConfiguration MergeMemberConfiguration(IFieldConfiguration member, IFieldConfiguration mergedMember = null)
        {
            mergedMember = mergedMember ?? new FieldConfiguration(member.FieldMemberInfo);
            foreach (var data in member.Data)
            {
                mergedMember.Data[data.Key] = data.Value;
            }

            mergedMember.Ignored = member.Ignored ?? mergedMember.Ignored;

            mergedMember.Input = member.Input ?? mergedMember.Input;
            mergedMember.Output = member.Output ?? mergedMember.Output;

            return mergedMember;
        }

        private void ThrowIfLocked()
        {
            if (_isLocked)
            {
                throw new InvalidOperationException("Cannot configure model as breeze configurator is locked. " +
                                                    "Assure that all configurations for models are done at startup of the application");
            }
        }
    }
}
