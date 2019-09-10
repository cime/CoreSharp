using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Events;
using FluentValidation;
using FluentValidation.Results;
using NHibernate;
using SimpleInjector;

namespace CoreSharp.Breeze.Validation
{
    public abstract class ValidatedBreezeContext : BreezeContext
    {
        private readonly SimpleInjector.Container _container;

        public ValidatedBreezeContext(
            ISession session,
            IBreezeConfig breezeConfig,
            IBreezeConfigurator breezeConfigurator,
            IEventPublisher eventPublisher,
            Container container) : base(breezeConfig, session, breezeConfigurator, eventPublisher)
        {
            _container = container;
        }


        public override List<EntityInfo> BeforeSaveEntityGraph(List<EntityInfo> entitiesToPersist)
        {
            ValidateEntities(entitiesToPersist);

            return base.BeforeSaveEntityGraph(entitiesToPersist);
        }

        public override Task<List<EntityInfo>> BeforeSaveEntityGraphAsync(List<EntityInfo> entitiesToPersist)
        {
            ValidateEntities(entitiesToPersist);

            return Task.FromResult(base.BeforeSaveEntityGraph(entitiesToPersist));
        }

        private void ValidateEntities(List<EntityInfo> entitiesToPersist)
        {
            var validationResults = new Dictionary<EntityInfo, ValidationResult>();

            foreach (var entityInfo in entitiesToPersist)
            {
                var validatorType = typeof(IValidator<>).MakeGenericType(entityInfo.Entity.GetType());
                var validator = ((IServiceProvider)_container).GetService(validatorType);

                if (validator != null)
                {
                    var validateMethod = typeof(DefaultValidatorExtensions).GetMethods().Single(x => x.Name == "Validate" && x.GetParameters().All(y => new[] { "validator", "instance", "selector", "ruleSet" }.Contains(y.Name)));
                    validateMethod = validateMethod.MakeGenericMethod(entityInfo.Entity.GetType());

                    var validationResult = (ValidationResult)validateMethod.Invoke(null, new[] { validator, entityInfo.Entity, null, $"{BreezeValidationRuleSet.BreezeDefault},Breeze{Enum.GetName(typeof(EntityState), entityInfo.EntityState)}" });

                    if (!validationResult.IsValid)
                    {
                        validationResults[entityInfo] = validationResult;
                    }
                }
            }

            if (validationResults.Any())
            {
                throw new BreezeValidationException(validationResults.Where(x => !x.Value.IsValid).ToDictionary(kv => kv.Key, kv => kv.Value));
            }
        }

        protected override bool HandleSaveException(Exception e, SaveWorkState saveWorkState)
        {

            if (e is BreezeValidationException)
            {
                var ex = (BreezeValidationException)e;

                //TODO: handle fluent validation exceptions
            }

            return base.HandleSaveException(e, saveWorkState);
        }
    }
}
