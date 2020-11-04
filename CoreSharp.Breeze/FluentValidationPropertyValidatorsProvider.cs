using System;
using System.Collections.Generic;
using System.Linq;
using Breeze.NHibernate;
using Breeze.NHibernate.Metadata;
using CoreSharp.Common.Extensions;
using CoreSharp.Validation;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;

namespace CoreSharp.Breeze
{
    public class FluentValidationPropertyValidatorsProvider : DefaultPropertyValidatorsProvider
    {
        private readonly IValidatorFactory _validatorFactory;
        private readonly IEntityMetadataProvider _entityMetadataProvider;
        private readonly IClientModelMetadataProvider _clientModelMetadataProvider;
        private readonly Dictionary<Type, ILookup<string, PropertyRule>> _typePropertyValidators = new Dictionary<Type, ILookup<string, PropertyRule>>();

        public FluentValidationPropertyValidatorsProvider(
            IValidatorFactory validatorFactory,
            IEntityMetadataProvider entityMetadataProvider,
            IClientModelMetadataProvider clientModelMetadataProvider)
        {
            _validatorFactory = validatorFactory;
            _entityMetadataProvider = entityMetadataProvider;
            _clientModelMetadataProvider = clientModelMetadataProvider;
        }

        public override IEnumerable<Validator> GetValidators(DataProperty dataProperty, Type type)
        {
            Validator validator;
            var addedValidators = new HashSet<string>();
            if (!dataProperty.IsNullable)
            {
                ModelMetadata modelMetadata = null;
                if (_entityMetadataProvider.IsEntityType(type))
                {
                    modelMetadata = _entityMetadataProvider.GetMetadata(type);
                }
                else if (_clientModelMetadataProvider.IsClientModel(type))
                {
                    modelMetadata = _clientModelMetadataProvider.GetMetadata(type);
                }

                // Do not permit default values for foreign key properties
                if (modelMetadata?.ForeignKeyPropertyNames.Contains(dataProperty.NameOnServer) == true || dataProperty.DataType == DataType.String)
                {
                    addedValidators.Add("fvNotEmpty");
                    validator = new Validator("fvNotEmpty");
                    validator.MergeLeft(FluentValidators.GetParameters(new NotEmptyValidator(null)));
                    yield return validator;
                }
                else
                {
                    addedValidators.Add("fvNotNull");
                    validator = new Validator("fvNotNull");
                    validator.MergeLeft(FluentValidators.GetParameters(new NotNullValidator()));
                    yield return validator;
                }
            }

            if (dataProperty.MaxLength.HasValue)
            {
                addedValidators.Add("fvLength");
                validator = new Validator("fvLength");
                validator.MergeLeft(FluentValidators.GetParameters(new LengthValidator(0, dataProperty.MaxLength.Value)));
                yield return validator;
            }

            if (dataProperty.DataType.HasValue && TryGetDataTypeValidator(dataProperty.DataType.Value, out validator))
            {
                yield return validator;
            }

            if (!_typePropertyValidators.TryGetValue(type, out var propertyValidators))
            {
                if (!(_validatorFactory.GetValidator(type) is IEnumerable<IValidationRule> validationRules))
                {
                    yield break;
                }

                propertyValidators = validationRules.OfType<PropertyRule>().Where(IsRuleSupported).ToLookup(o => o.PropertyName);
                _typePropertyValidators.Add(type, propertyValidators);
            }

            // Add fluent validations
            foreach (var propertyRule in propertyValidators[dataProperty.NameOnServer])
            {
                var currentValidator = propertyRule.CurrentValidator;
                var name = FluentValidators.GetName(currentValidator);
                if (string.IsNullOrEmpty(name) || addedValidators.Contains(name))
                {
                    continue;
                }

                validator = new Validator(name);
                validator.MergeLeft(FluentValidators.GetParameters(currentValidator));
                yield return validator;
            }
        }

        private static bool IsRuleSupported(PropertyRule rule)
        {
            if (rule.RuleSets != null && !ValidationRuleSet.InsertUpdate.Intersect(rule.RuleSets).Any())
            {
                return false;
            }

            var options = rule.CurrentValidator.Options;
            return !string.IsNullOrEmpty(rule.PropertyName) &&
                   rule.Condition == null &&
                   rule.AsyncCondition == null &&
                   options.Condition == null &&
                   options.AsyncCondition == null;
        }
    }
}
