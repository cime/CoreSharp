using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using CoreSharp.Tests.Validation.Models;
using CoreSharp.Validation;
using CoreSharp.Common.Tests;
using Xunit;
using SimpleInjector;

namespace CoreSharp.Tests.Validation
{
    public class ValidationTests : IClassFixture<Bootstrapper>
    {
        private Bootstrapper _baseTest;

        public ValidationTests(Bootstrapper baseTest)
        {
            _baseTest = baseTest;
            _baseTest.BeforeInitialization(container =>
            {
                container.RegisterPackages();
                container.RegisterValidatorsFromAssemblyOf<LifecycleTests>();
            });

            _baseTest.Initialize();
        }

        [Fact]
        public void ValidateWithDomainValidator()
        {
            TestModelDomainValidator.ValidateCount = 0;
            TestModelDomainValidator.CanValidateCount = 0;
            TestModelDomainValidator.ValidatBeforeValidationCount = 0;
            TestModelAsyncDomainValidator.ValidateCount = 0;
            TestModelAsyncDomainValidator.CanValidateCount = 0;
            TestModelAsyncDomainValidator.ValidatBeforeValidationCount = 0;
            var validator = _baseTest.Container.GetInstance<IValidator<TestModel>>();

            for (var i = 0; i < 5; i++)
            {
                var model = new TestModel();
                var valResult = validator.Validate(model);
                Assert.True(valResult.IsValid);
                Assert.Equal(i + 1, TestModelDomainValidator.ValidateCount);
                Assert.Equal(i + 1, TestModelDomainValidator.CanValidateCount);
                Assert.Equal(i + 1, TestModelDomainValidator.ValidatBeforeValidationCount);
                Assert.Equal(i + 1, TestModelAsyncDomainValidator.ValidateCount);
                Assert.Equal(i + 1, TestModelAsyncDomainValidator.CanValidateCount);
                Assert.Equal(i + 1, TestModelAsyncDomainValidator.ValidatBeforeValidationCount);
            }
        }

        [Fact]
        public async Task ValidateWithAsyncDomainValidator()
        {
            TestModelDomainValidator.ValidateCount = 0;
            TestModelDomainValidator.CanValidateCount = 0;
            TestModelDomainValidator.ValidatBeforeValidationCount = 0;
            TestModelAsyncDomainValidator.ValidateCount = 0;
            TestModelAsyncDomainValidator.CanValidateCount = 0;
            TestModelAsyncDomainValidator.ValidatBeforeValidationCount = 0;
            var validator = _baseTest.Container.GetInstance<IValidator<TestModel>>();

            for (var i = 0; i < 5; i++)
            {
                var model = new TestModel();
                var valResult = await validator.ValidateAsync(model);
                Assert.True(valResult.IsValid);
                Assert.Equal(i + 1, TestModelDomainValidator.ValidateCount);
                Assert.Equal(i + 1, TestModelDomainValidator.CanValidateCount);
                Assert.Equal(i + 1, TestModelDomainValidator.ValidatBeforeValidationCount);
                Assert.Equal(i + 1, TestModelAsyncDomainValidator.ValidateCount);
                Assert.Equal(i + 1, TestModelAsyncDomainValidator.CanValidateCount);
                Assert.Equal(i + 1, TestModelAsyncDomainValidator.ValidatBeforeValidationCount);
            }
        }

        [Fact]
        public async Task ValidateNestedModelWithDomainValidator()
        {
            var valFuns = new List<Func<IValidator<Parent>, Parent, Task<ValidationResult>>>
            {
                (v, m) => v.ValidateAsync(m),
                (v, m) => v.ValidateAsync((object) m),
                (v, m) => v.ValidateAsync(new ValidationContext(m)),
                (v, m) => v.ValidateAsync(new ValidationContext<Parent>(m)),
                (v, m) => v.ValidateAsync(m, ruleSet: ValidationRuleSet.Default),
                (v, m) => Task.FromResult(v.Validate(m)),
                (v, m) => Task.FromResult(v.Validate((object) m)),
                (v, m) => Task.FromResult(v.Validate(new ValidationContext(m))),
                (v, m) => Task.FromResult(v.Validate(new ValidationContext<Parent>(m))),
                (v, m) => Task.FromResult(v.Validate(m, ruleSet: ValidationRuleSet.Default))
            };

            foreach (var valFun in valFuns)
            {
                for (var i = 0; i < 5; i++)
                {
                    ParentDomainValidator.Clear();
                    ParentChildDomainValidator.Clear();
                    ParentChild2DomainValidator.Clear();
                    ChildSubChildDomainValidator.Clear();
                    ParentSubChildDomainValidator.Clear();
                    SubChildDomainValidator.Clear();

                    var validator = _baseTest.Container.GetInstance<IValidator<Parent>>();
                    var model = new Parent
                    {
                        Children = new List<Child>
                        {
                            new Child
                            {
                                Name = "Child",
                                Children = new List<SubChild>
                                {
                                    new SubChild {Name = "SubChild1"},
                                    new SubChild()
                                }
                            },
                            new Child
                            {
                                Children = new List<SubChild>
                                {
                                    new SubChild {Name = "SubChild1"}
                                }
                            }
                        }
                    };
                    var result = await valFun(validator, model);

                    Assert.True(result.IsValid);

                    Assert.Single(ParentDomainValidator.Instances);
                    Assert.Single(ParentDomainValidator.ValidateModels);
                    Assert.Equal(model, ParentDomainValidator.ValidateModels[0].Item1);
                    Assert.Single(ParentDomainValidator.CanValidateModels);
                    Assert.Equal(model, ParentDomainValidator.CanValidateModels[0].Item1);
                    Assert.Single(ParentDomainValidator.BeforeValidationModels);
                    Assert.Equal(model, ParentDomainValidator.BeforeValidationModels[0].Item1);

                    Assert.Single(ParentChildDomainValidator.Instances);
                    Assert.Equal(2, ParentChildDomainValidator.ValidateModels.Count);
                    Assert.Equal(model.Children[0], ParentChildDomainValidator.ValidateModels[0].Item1);
                    Assert.Equal(model.Children[1], ParentChildDomainValidator.ValidateModels[1].Item1);
                    Assert.Equal(2, ParentChildDomainValidator.CanValidateModels.Count);
                    Assert.Equal(model.Children[0], ParentChildDomainValidator.CanValidateModels[0].Item1);
                    Assert.Equal(model.Children[1], ParentChildDomainValidator.CanValidateModels[1].Item1);
                    Assert.Single(ParentChildDomainValidator.BeforeValidationModels);
                    Assert.Equal(model, ParentChildDomainValidator.BeforeValidationModels[0].Item1);

                    Assert.Single(ParentChild2DomainValidator.Instances);
                    Assert.Equal(2, ParentChild2DomainValidator.ValidateModels.Count);
                    Assert.Equal(model.Children[0], ParentChild2DomainValidator.ValidateModels[0].Item1);
                    Assert.Equal(model.Children[1], ParentChild2DomainValidator.ValidateModels[1].Item1);
                    Assert.Equal(2, ParentChild2DomainValidator.CanValidateModels.Count);
                    Assert.Equal(model.Children[0], ParentChild2DomainValidator.CanValidateModels[0].Item1);
                    Assert.Equal(model.Children[1], ParentChild2DomainValidator.CanValidateModels[1].Item1);
                    Assert.Single(ParentChild2DomainValidator.BeforeValidationModels);
                    Assert.Equal(model, ParentChild2DomainValidator.BeforeValidationModels[0].Item1);

                    Assert.Single(ParentSubChildDomainValidator.Instances);
                    Assert.Equal(3, ParentSubChildDomainValidator.ValidateModels.Count);
                    Assert.Equal(model.Children[0].Children[0], ParentSubChildDomainValidator.ValidateModels[0].Item1);
                    Assert.Equal(model.Children[0].Children[1], ParentSubChildDomainValidator.ValidateModels[1].Item1);
                    Assert.Equal(model.Children[1].Children[0], ParentSubChildDomainValidator.ValidateModels[2].Item1);
                    Assert.Equal(3, ParentSubChildDomainValidator.CanValidateModels.Count);
                    Assert.Equal(model.Children[0].Children[0], ParentSubChildDomainValidator.CanValidateModels[0].Item1);
                    Assert.Equal(model.Children[0].Children[1], ParentSubChildDomainValidator.CanValidateModels[1].Item1);
                    Assert.Equal(model.Children[1].Children[0], ParentSubChildDomainValidator.CanValidateModels[2].Item1);
                    Assert.Single(ParentSubChildDomainValidator.BeforeValidationModels);
                    Assert.Equal(model, ParentSubChildDomainValidator.BeforeValidationModels[0].Item1);

                    Assert.Equal(2, ChildSubChildDomainValidator.Instances.Count);
                    Assert.Equal(3, ChildSubChildDomainValidator.ValidateModels.Count);
                    Assert.Equal(model.Children[0].Children[0], ChildSubChildDomainValidator.ValidateModels[0].Item1);
                    Assert.Equal(model.Children[0].Children[1], ChildSubChildDomainValidator.ValidateModels[1].Item1);
                    Assert.Equal(model.Children[1].Children[0], ChildSubChildDomainValidator.ValidateModels[2].Item1);
                    Assert.Equal(ChildSubChildDomainValidator.Instances[0], ChildSubChildDomainValidator.ValidateModels[0].Item2);
                    Assert.Equal(ChildSubChildDomainValidator.Instances[0], ChildSubChildDomainValidator.ValidateModels[1].Item2);
                    Assert.Equal(ChildSubChildDomainValidator.Instances[1], ChildSubChildDomainValidator.ValidateModels[2].Item2);
                    Assert.Equal(3, ChildSubChildDomainValidator.CanValidateModels.Count);
                    Assert.Equal(model.Children[0].Children[0], ChildSubChildDomainValidator.CanValidateModels[0].Item1);
                    Assert.Equal(model.Children[0].Children[1], ChildSubChildDomainValidator.CanValidateModels[1].Item1);
                    Assert.Equal(model.Children[1].Children[0], ChildSubChildDomainValidator.CanValidateModels[2].Item1);
                    Assert.Equal(ChildSubChildDomainValidator.Instances[0], ChildSubChildDomainValidator.CanValidateModels[0].Item2);
                    Assert.Equal(ChildSubChildDomainValidator.Instances[0], ChildSubChildDomainValidator.CanValidateModels[1].Item2);
                    Assert.Equal(ChildSubChildDomainValidator.Instances[1], ChildSubChildDomainValidator.CanValidateModels[2].Item2);
                    Assert.Equal(2, ChildSubChildDomainValidator.BeforeValidationModels.Count);
                    Assert.Equal(model.Children[0], ChildSubChildDomainValidator.BeforeValidationModels[0].Item1);
                    Assert.Equal(model.Children[1], ChildSubChildDomainValidator.BeforeValidationModels[1].Item1);
                    Assert.Equal(ChildSubChildDomainValidator.Instances[0], ChildSubChildDomainValidator.BeforeValidationModels[0].Item2);
                    Assert.Equal(ChildSubChildDomainValidator.Instances[1], ChildSubChildDomainValidator.BeforeValidationModels[1].Item2);

                    Assert.Equal(3, SubChildDomainValidator.Instances.Count);
                    Assert.Equal(3, SubChildDomainValidator.ValidateModels.Count);
                    Assert.Equal(model.Children[0].Children[0], SubChildDomainValidator.ValidateModels[0].Item1);
                    Assert.Equal(model.Children[0].Children[1], SubChildDomainValidator.ValidateModels[1].Item1);
                    Assert.Equal(model.Children[1].Children[0], SubChildDomainValidator.ValidateModels[2].Item1);
                    Assert.Equal(SubChildDomainValidator.Instances[0], SubChildDomainValidator.ValidateModels[0].Item2);
                    Assert.Equal(SubChildDomainValidator.Instances[1], SubChildDomainValidator.ValidateModels[1].Item2);
                    Assert.Equal(SubChildDomainValidator.Instances[2], SubChildDomainValidator.ValidateModels[2].Item2);
                    Assert.Equal(3, SubChildDomainValidator.CanValidateModels.Count);
                    Assert.Equal(model.Children[0].Children[0], SubChildDomainValidator.CanValidateModels[0].Item1);
                    Assert.Equal(model.Children[0].Children[1], SubChildDomainValidator.CanValidateModels[1].Item1);
                    Assert.Equal(model.Children[1].Children[0], SubChildDomainValidator.CanValidateModels[2].Item1);
                    Assert.Equal(SubChildDomainValidator.Instances[0], SubChildDomainValidator.CanValidateModels[0].Item2);
                    Assert.Equal(SubChildDomainValidator.Instances[1], SubChildDomainValidator.CanValidateModels[1].Item2);
                    Assert.Equal(SubChildDomainValidator.Instances[2], SubChildDomainValidator.CanValidateModels[2].Item2);
                    Assert.Equal(3, SubChildDomainValidator.BeforeValidationModels.Count);
                    Assert.Equal(model.Children[0].Children[0], SubChildDomainValidator.BeforeValidationModels[0].Item1);
                    Assert.Equal(model.Children[0].Children[1], SubChildDomainValidator.BeforeValidationModels[1].Item1);
                    Assert.Equal(model.Children[1].Children[0], SubChildDomainValidator.BeforeValidationModels[2].Item1);
                    Assert.Equal(SubChildDomainValidator.Instances[0], SubChildDomainValidator.BeforeValidationModels[0].Item2);
                    Assert.Equal(SubChildDomainValidator.Instances[1], SubChildDomainValidator.BeforeValidationModels[1].Item2);
                    Assert.Equal(SubChildDomainValidator.Instances[2], SubChildDomainValidator.BeforeValidationModels[2].Item2);
                }
            }
        }

        [Fact]
        public async Task ValidateNestedModelWithAsyncDomainValidator()
        {
            var valFuns = new List<Func<IValidator<AsyncParent>, AsyncParent, Task<ValidationResult>>>
            {
                (v, m) => v.ValidateAsync(m),
                (v, m) => v.ValidateAsync((object) m),
                (v, m) => v.ValidateAsync(new ValidationContext(m)),
                (v, m) => v.ValidateAsync(new ValidationContext<AsyncParent>(m)),
                (v, m) => v.ValidateAsync(m, ruleSet: ValidationRuleSet.Default),
                (v, m) => Task.FromResult(v.Validate(m)),
                (v, m) => Task.FromResult(v.Validate((object) m)),
                (v, m) => Task.FromResult(v.Validate(new ValidationContext(m))),
                (v, m) => Task.FromResult(v.Validate(new ValidationContext<AsyncParent>(m))),
                (v, m) => Task.FromResult(v.Validate(m, ruleSet: ValidationRuleSet.Default))
            };

            foreach (var valFun in valFuns)
            {
                for (var i = 0; i < 5; i++)
                {
                    AsyncParentDomainValidator.Clear();
                    AsyncParentChildDomainValidator.Clear();
                    AsyncParentChild2DomainValidator.Clear();
                    AsyncChildSubChildDomainValidator.Clear();
                    AsyncParentSubChildDomainValidator.Clear();
                    AsyncSubChildDomainValidator.Clear();

                    var validator = _baseTest.Container.GetInstance<IValidator<AsyncParent>>();
                    var model = new AsyncParent
                    {
                        Children = new List<AsyncChild>
                        {
                            new AsyncChild
                            {
                                Name = "Child",
                                Children = new List<AsyncSubChild>
                                {
                                    new AsyncSubChild {Name = "SubChild1"},
                                    new AsyncSubChild()
                                }
                            },
                            new AsyncChild
                            {
                                Children = new List<AsyncSubChild>
                                {
                                    new AsyncSubChild {Name = "SubChild1"}
                                }
                            }
                        }
                    };
                    var result = await valFun(validator, model);

                    Assert.True(result.IsValid);

                    Assert.Single(AsyncParentDomainValidator.Instances);
                    Assert.Single(AsyncParentDomainValidator.ValidateModels);
                    Assert.Equal(model, AsyncParentDomainValidator.ValidateModels[0].Item1);
                    Assert.Single(AsyncParentDomainValidator.CanValidateModels);
                    Assert.Equal(model, AsyncParentDomainValidator.CanValidateModels[0].Item1);
                    Assert.Single(AsyncParentDomainValidator.BeforeValidationModels);
                    Assert.Equal(model, AsyncParentDomainValidator.BeforeValidationModels[0].Item1);

                    Assert.Single(AsyncParentChildDomainValidator.Instances);
                    Assert.Equal(2, AsyncParentChildDomainValidator.ValidateModels.Count);
                    Assert.Equal(model.Children[0], AsyncParentChildDomainValidator.ValidateModels[0].Item1);
                    Assert.Equal(model.Children[1], AsyncParentChildDomainValidator.ValidateModels[1].Item1);
                    Assert.Equal(2, AsyncParentChildDomainValidator.CanValidateModels.Count);
                    Assert.Equal(model.Children[0], AsyncParentChildDomainValidator.CanValidateModels[0].Item1);
                    Assert.Equal(model.Children[1], AsyncParentChildDomainValidator.CanValidateModels[1].Item1);
                    Assert.Single(AsyncParentChildDomainValidator.BeforeValidationModels);
                    Assert.Equal(model, AsyncParentChildDomainValidator.BeforeValidationModels[0].Item1);

                    Assert.Single(AsyncParentChild2DomainValidator.Instances);
                    Assert.Equal(2, AsyncParentChild2DomainValidator.ValidateModels.Count);
                    Assert.Equal(model.Children[0], AsyncParentChild2DomainValidator.ValidateModels[0].Item1);
                    Assert.Equal(model.Children[1], AsyncParentChild2DomainValidator.ValidateModels[1].Item1);
                    Assert.Equal(2, AsyncParentChild2DomainValidator.CanValidateModels.Count);
                    Assert.Equal(model.Children[0], AsyncParentChild2DomainValidator.CanValidateModels[0].Item1);
                    Assert.Equal(model.Children[1], AsyncParentChild2DomainValidator.CanValidateModels[1].Item1);
                    Assert.Single(AsyncParentChild2DomainValidator.BeforeValidationModels);
                    Assert.Equal(model, AsyncParentChild2DomainValidator.BeforeValidationModels[0].Item1);

                    Assert.Single(AsyncParentSubChildDomainValidator.Instances);
                    Assert.Equal(3, AsyncParentSubChildDomainValidator.ValidateModels.Count);
                    Assert.Equal(model.Children[0].Children[0], AsyncParentSubChildDomainValidator.ValidateModels[0].Item1);
                    Assert.Equal(model.Children[0].Children[1], AsyncParentSubChildDomainValidator.ValidateModels[1].Item1);
                    Assert.Equal(model.Children[1].Children[0], AsyncParentSubChildDomainValidator.ValidateModels[2].Item1);
                    Assert.Equal(3, AsyncParentSubChildDomainValidator.CanValidateModels.Count);
                    Assert.Equal(model.Children[0].Children[0], AsyncParentSubChildDomainValidator.CanValidateModels[0].Item1);
                    Assert.Equal(model.Children[0].Children[1], AsyncParentSubChildDomainValidator.CanValidateModels[1].Item1);
                    Assert.Equal(model.Children[1].Children[0], AsyncParentSubChildDomainValidator.CanValidateModels[2].Item1);
                    Assert.Single(AsyncParentSubChildDomainValidator.BeforeValidationModels);
                    Assert.Equal(model, AsyncParentSubChildDomainValidator.BeforeValidationModels[0].Item1);

                    Assert.Equal(2, AsyncChildSubChildDomainValidator.Instances.Count);
                    Assert.Equal(3, AsyncChildSubChildDomainValidator.ValidateModels.Count);
                    Assert.Equal(model.Children[0].Children[0], AsyncChildSubChildDomainValidator.ValidateModels[0].Item1);
                    Assert.Equal(model.Children[0].Children[1], AsyncChildSubChildDomainValidator.ValidateModels[1].Item1);
                    Assert.Equal(model.Children[1].Children[0], AsyncChildSubChildDomainValidator.ValidateModels[2].Item1);
                    Assert.Equal(AsyncChildSubChildDomainValidator.Instances[0], AsyncChildSubChildDomainValidator.ValidateModels[0].Item2);
                    Assert.Equal(AsyncChildSubChildDomainValidator.Instances[0], AsyncChildSubChildDomainValidator.ValidateModels[1].Item2);
                    Assert.Equal(AsyncChildSubChildDomainValidator.Instances[1], AsyncChildSubChildDomainValidator.ValidateModels[2].Item2);
                    Assert.Equal(3, AsyncChildSubChildDomainValidator.CanValidateModels.Count);
                    Assert.Equal(model.Children[0].Children[0], AsyncChildSubChildDomainValidator.CanValidateModels[0].Item1);
                    Assert.Equal(model.Children[0].Children[1], AsyncChildSubChildDomainValidator.CanValidateModels[1].Item1);
                    Assert.Equal(model.Children[1].Children[0], AsyncChildSubChildDomainValidator.CanValidateModels[2].Item1);
                    Assert.Equal(AsyncChildSubChildDomainValidator.Instances[0], AsyncChildSubChildDomainValidator.CanValidateModels[0].Item2);
                    Assert.Equal(AsyncChildSubChildDomainValidator.Instances[0], AsyncChildSubChildDomainValidator.CanValidateModels[1].Item2);
                    Assert.Equal(AsyncChildSubChildDomainValidator.Instances[1], AsyncChildSubChildDomainValidator.CanValidateModels[2].Item2);
                    Assert.Equal(2, AsyncChildSubChildDomainValidator.BeforeValidationModels.Count);
                    Assert.Equal(model.Children[0], AsyncChildSubChildDomainValidator.BeforeValidationModels[0].Item1);
                    Assert.Equal(model.Children[1], AsyncChildSubChildDomainValidator.BeforeValidationModels[1].Item1);
                    Assert.Equal(AsyncChildSubChildDomainValidator.Instances[0], AsyncChildSubChildDomainValidator.BeforeValidationModels[0].Item2);
                    Assert.Equal(AsyncChildSubChildDomainValidator.Instances[1], AsyncChildSubChildDomainValidator.BeforeValidationModels[1].Item2);

                    Assert.Equal(3, AsyncSubChildDomainValidator.Instances.Count);
                    Assert.Equal(3, AsyncSubChildDomainValidator.ValidateModels.Count);
                    Assert.Equal(model.Children[0].Children[0], AsyncSubChildDomainValidator.ValidateModels[0].Item1);
                    Assert.Equal(model.Children[0].Children[1], AsyncSubChildDomainValidator.ValidateModels[1].Item1);
                    Assert.Equal(model.Children[1].Children[0], AsyncSubChildDomainValidator.ValidateModels[2].Item1);
                    Assert.Equal(AsyncSubChildDomainValidator.Instances[0], AsyncSubChildDomainValidator.ValidateModels[0].Item2);
                    Assert.Equal(AsyncSubChildDomainValidator.Instances[1], AsyncSubChildDomainValidator.ValidateModels[1].Item2);
                    Assert.Equal(AsyncSubChildDomainValidator.Instances[2], AsyncSubChildDomainValidator.ValidateModels[2].Item2);
                    Assert.Equal(3, AsyncSubChildDomainValidator.CanValidateModels.Count);
                    Assert.Equal(model.Children[0].Children[0], AsyncSubChildDomainValidator.CanValidateModels[0].Item1);
                    Assert.Equal(model.Children[0].Children[1], AsyncSubChildDomainValidator.CanValidateModels[1].Item1);
                    Assert.Equal(model.Children[1].Children[0], AsyncSubChildDomainValidator.CanValidateModels[2].Item1);
                    Assert.Equal(AsyncSubChildDomainValidator.Instances[0], AsyncSubChildDomainValidator.CanValidateModels[0].Item2);
                    Assert.Equal(AsyncSubChildDomainValidator.Instances[1], AsyncSubChildDomainValidator.CanValidateModels[1].Item2);
                    Assert.Equal(AsyncSubChildDomainValidator.Instances[2], AsyncSubChildDomainValidator.CanValidateModels[2].Item2);
                    Assert.Equal(3, AsyncSubChildDomainValidator.BeforeValidationModels.Count);
                    Assert.Equal(model.Children[0].Children[0], AsyncSubChildDomainValidator.BeforeValidationModels[0].Item1);
                    Assert.Equal(model.Children[0].Children[1], AsyncSubChildDomainValidator.BeforeValidationModels[1].Item1);
                    Assert.Equal(model.Children[1].Children[0], AsyncSubChildDomainValidator.BeforeValidationModels[2].Item1);
                    Assert.Equal(AsyncSubChildDomainValidator.Instances[0], AsyncSubChildDomainValidator.BeforeValidationModels[0].Item2);
                    Assert.Equal(AsyncSubChildDomainValidator.Instances[1], AsyncSubChildDomainValidator.BeforeValidationModels[1].Item2);
                    Assert.Equal(AsyncSubChildDomainValidator.Instances[2], AsyncSubChildDomainValidator.BeforeValidationModels[2].Item2);
                }
            }
        }

        [Fact]
        public void GenericChildDomainValidatorShouldWork()
        {
            var child1 = new ConcreteGenericChildChild();
            var child2 = new ConcreteGenericChildChild2();
            var child3 = new ConcreteGenericChildChild {Name = "Test"};
            var child4 = new ConcreteGenericChildChild2();
            var model = new GenericChildParent
            {
                Children = new List<GenericChildChild>
                {
                    child1,
                    child2,
                    child3
                },
                Relation = child4
            };
            var validator = _baseTest.Container.GetInstance<IValidator<GenericChildParent>>();

            var valResult = validator.Validate(model);
            Assert.False(valResult.IsValid);
            Assert.Equal(3, valResult.Errors.Count);
            Assert.Equal("Name should not be empty", valResult.Errors[0].ErrorMessage);
            Assert.Equal("Children[0]", valResult.Errors[0].PropertyName);
            Assert.Equal(child1, valResult.Errors[0].AttemptedValue);
            Assert.Equal("Name should not be empty", valResult.Errors[1].ErrorMessage);
            Assert.Equal("Children[1]", valResult.Errors[1].PropertyName);
            Assert.Equal(child2, valResult.Errors[1].AttemptedValue);
            Assert.Equal("Name should not be empty", valResult.Errors[2].ErrorMessage);
            Assert.Equal("Relation", valResult.Errors[2].PropertyName);
            Assert.Equal(child4, valResult.Errors[2].AttemptedValue);

            var childValidator = _baseTest.Container.GetInstance<IValidator<GenericChildChild>>();
            valResult = childValidator.Validate(child1);
            Assert.True(valResult.IsValid);
        }

        [Fact]
        public void GenericRootChildDomainValidatorShouldWork()
        {
            var child1 = new ConcreteGenericRootChildChild();
            var child2 = new ConcreteGenericRootChildChild2();
            var child3 = new ConcreteGenericRootChildChild { Name = "Test" };
            var child4 = new ConcreteGenericRootChildChild2();
            var model = new GenericRootChildParent
            {
                Children = new List<GenericRootChildChild>
                {
                    child1,
                    child2,
                    child3
                },
                Relation = child4
            };
            var validator = _baseTest.Container.GetInstance<IValidator<GenericRootChildParent>>();

            var valResult = validator.Validate(model);
            Assert.False(valResult.IsValid);
            Assert.Equal(3, valResult.Errors.Count);
            Assert.Equal("Name should not be empty", valResult.Errors[0].ErrorMessage);
            Assert.Equal("Children[0]", valResult.Errors[0].PropertyName);
            Assert.Equal(child1, valResult.Errors[0].AttemptedValue);
            Assert.Equal("Name should not be empty", valResult.Errors[1].ErrorMessage);
            Assert.Equal("Children[1]", valResult.Errors[1].PropertyName);
            Assert.Equal(child2, valResult.Errors[1].AttemptedValue);
            Assert.Equal("Name should not be empty", valResult.Errors[2].ErrorMessage);
            Assert.Equal("Relation", valResult.Errors[2].PropertyName);
            Assert.Equal(child4, valResult.Errors[2].AttemptedValue);

            var childValidator = _baseTest.Container.GetInstance<IValidator<GenericRootChildChild>>();
            valResult = childValidator.Validate(child1);
            Assert.True(valResult.IsValid);
        }

        [Fact]
        public void GenericRootDomainValidatorShouldWork()
        {
            var child1 = new GenericRootModel();
            var child2 = new GenericRootModel();
            var child3 = new GenericRootModel { Name = "Test" };
            var child4 = new GenericRootModel();
            var model = new GenericRootModel
            {
                Children = new List<GenericRootModel>
                {
                    child1,
                    child2,
                    child3
                },
                Relation = child4
            };
            var validator = _baseTest.Container.GetInstance<IValidator<GenericRootModel>>();

            var valResult = validator.Validate(model);
            Assert.False(valResult.IsValid);
            Assert.Equal(4, valResult.Errors.Count);
            Assert.Equal("Name should not be empty", valResult.Errors[0].ErrorMessage);
            Assert.Equal("Children[0]", valResult.Errors[0].PropertyName);
            Assert.Equal(child1, valResult.Errors[0].AttemptedValue);
            Assert.Equal("Name should not be empty", valResult.Errors[1].ErrorMessage);
            Assert.Equal("Children[1]", valResult.Errors[1].PropertyName);
            Assert.Equal(child2, valResult.Errors[1].AttemptedValue);
            Assert.Equal("Name should not be empty", valResult.Errors[2].ErrorMessage);
            Assert.Equal("Relation", valResult.Errors[2].PropertyName);
            Assert.Equal(child4, valResult.Errors[2].AttemptedValue);
            Assert.Equal("Name should not be empty", valResult.Errors[3].ErrorMessage);
            Assert.Equal("", valResult.Errors[3].PropertyName);
            Assert.Equal(model, valResult.Errors[3].AttemptedValue);

            var childValidator = _baseTest.Container.GetInstance<IValidator<GenericRootModel>>();
            valResult = childValidator.Validate(child1);
            Assert.False(valResult.IsValid);
        }
    }
}
