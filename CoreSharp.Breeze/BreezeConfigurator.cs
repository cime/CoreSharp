using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSharp.Breeze
{
    public class BreezeConfigurator : IBreezeConfigurator
    {
        protected Dictionary<Type, IModelConfiguration> ModelsConfiguration = new Dictionary<Type, IModelConfiguration>();
        private bool _isLocked = false;

        #region ModelConfigurationCreated

        public static event Action<IModelConfiguration> ModelConfigurationCreated;

        internal static void OnModelConfigurationCreated(IModelConfiguration obj)
        {
            var handler = ModelConfigurationCreated;
            handler?.Invoke(obj);
        }

        #endregion

        #region SerializationMemberRuleCreated

        public static event Action<IMemberConfiguration> SerializationMemberRuleCreated;

        internal static void OnSerializationMemberRuleCreated(IMemberConfiguration obj)
        {
            var handler = SerializationMemberRuleCreated;
            handler?.Invoke(obj);
        }

        #endregion

        public IModelConfiguration GetModelConfiguration<TModel>() where TModel : class
        {
            return GetModelConfiguration(typeof (TModel));
        }

        public IModelConfiguration GetModelConfiguration(Type modelType)
        {
            if (!_isLocked)
            {
                _isLocked = true;
            }
            var mergedConfig = (ModelConfiguration)Activator.CreateInstance(typeof(ModelConfiguration<>).MakeGenericType(modelType));
            //For interfaces we want to match only interfaces that are assignable from modelType
            var modelConfigs = modelType.IsInterface
                ? ModelsConfiguration.Where(o => o.Key.IsInterface && o.Key.IsAssignableFrom(modelType)).ToList() 
                : ModelsConfiguration.Where(o => o.Key.IsAssignableFrom(modelType)).ToList();
            //subclasses have higher priority
            modelConfigs.Sort((pair, valuePair) => pair.Key.IsAssignableFrom(valuePair.Key) ? -1 : 1);
            foreach (var modelConfig in modelConfigs.Select(o => o.Value))
            {
                mergedConfig.ModelType = modelConfig.ModelType;
                mergedConfig.ResourceName = modelConfig.ResourceName;
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

        private IMemberConfiguration MergeMemberConfiguration(IMemberConfiguration member, IMemberConfiguration mergedMember = null)
        {
            mergedMember = mergedMember ?? new MemberConfiguration(member.MemberInfo);
            foreach (var data in member.Data)
            {
                mergedMember.Data[data.Key] = data.Value;
            }
            mergedMember.Converter = member.Converter ?? mergedMember.Converter;
            mergedMember.DefaultValueHandling = member.DefaultValueHandling ?? mergedMember.DefaultValueHandling;
            mergedMember.DefaultValue = member.DefaultValue ?? mergedMember.DefaultValue;
            mergedMember.SerializeFunc = member.SerializeFunc ?? mergedMember.SerializeFunc;
            mergedMember.ShouldSerializePredicate = member.ShouldSerializePredicate ?? mergedMember.ShouldSerializePredicate;
            mergedMember.ShouldDeserializePredicate = member.ShouldDeserializePredicate ?? mergedMember.ShouldDeserializePredicate;
            mergedMember.Ignored = member.Ignored ?? mergedMember.Ignored;
            if (mergedMember.Ignored.GetValueOrDefault())
            {

            }
            mergedMember.Writable = member.Writable ?? mergedMember.Writable;
            mergedMember.Readable = member.Readable ?? mergedMember.Readable;
            mergedMember.SerializedName = member.SerializedName ?? mergedMember.SerializedName;
            mergedMember.MemberType = member.MemberType ?? mergedMember.MemberType;
            mergedMember.Order = member.Order ?? mergedMember.Order;
            mergedMember.LazyLoad = member.LazyLoad ?? mergedMember.LazyLoad;
            return mergedMember;
        }

        public IModelConfiguration<TModel> Configure<TModel>()
        {
            ThrowIfLocked();
            var type = typeof(TModel);
            if (ModelsConfiguration.ContainsKey(type))
                return (IModelConfiguration<TModel>)ModelsConfiguration[type];
            var modelConfig = new ModelConfiguration<TModel>();
            ModelsConfiguration[type] = modelConfig;
            OnModelConfigurationCreated(modelConfig);
            return modelConfig;
        }

        public IModelConfiguration Configure(Type type)
        {
            ThrowIfLocked();
            if (ModelsConfiguration.ContainsKey(type))
                return ModelsConfiguration[type];
            var modelConfig = (ModelConfiguration)Activator.CreateInstance(typeof(ModelConfiguration<>).MakeGenericType(type));
            ModelsConfiguration[type] = modelConfig;
            OnModelConfigurationCreated(modelConfig);
            return modelConfig;
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
