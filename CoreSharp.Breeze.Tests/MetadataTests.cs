using System.Linq;
using Breeze.NHibernate;
using Breeze.NHibernate.Metadata;
using CoreSharp.Breeze.Tests.Entities;
using CoreSharp.Common.Tests;
using Xunit;

namespace CoreSharp.Breeze.Tests
{
    public class MetadataTests : BaseBreezeTest
    {
        public MetadataTests(Bootstrapper bootstrapper) : base(bootstrapper)
        {
        }

        [Fact]
        public void TestFluentValidators()
        {
            var container = CreateTestContainer();
            var metadata = container.GetInstance<BreezeMetadataBuilder>()
                .WithClientModelAssemblies(GetEntityAssemblies())
                .Build();

            var type = (EntityType)metadata.StructuralTypes.First(o => o.Type == typeof(CompositeOrder));
            var properties = type.DataProperties.ToDictionary(o => o.NameOnServer);

            var validator = Assert.Single(properties[nameof(CompositeOrder.Status)].Validators);
            Assert.NotNull(validator);
            Assert.Equal("fvNotEmpty", validator.Name);
            Assert.True(validator.ContainsKey("errorMessageId"));

            var validators = properties[nameof(CompositeOrder.Number)].Validators;
            Assert.Equal(2, validators.Count);
            Assert.Equal(true, Assert.Single(validators.Where(o => o.Name == "fvNotNull"))?.ContainsKey("errorMessageId"));
            Assert.Single(validators.Where(o => o.Name == "integer"));

            validators = properties[nameof(CompositeOrder.Year)].Validators;
            Assert.Equal(2, validators.Count);
            Assert.Equal(true, Assert.Single(validators.Where(o => o.Name == "fvNotNull"))?.ContainsKey("errorMessageId"));
            Assert.Single(validators.Where(o => o.Name == "int32"));

            validators = properties[nameof(CompositeOrder.TotalPrice)].Validators;
            Assert.Equal(2, validators.Count);
            Assert.Equal(true, Assert.Single(validators.Where(o => o.Name == "fvNotNull"))?.ContainsKey("errorMessageId"));
            Assert.Single(validators.Where(o => o.Name == "number"));

            type = (EntityType)metadata.StructuralTypes.First(o => o.Type == typeof(CompositeOrderRow));
            properties = type.DataProperties.ToDictionary(o => o.NameOnServer);

            validator = Assert.Single(properties[$"{nameof(CompositeOrderRow.CompositeOrder)}{nameof(CompositeOrder.Status)}"].Validators);
            Assert.NotNull(validator);
            Assert.Equal("fvNotEmpty", validator.Name);

            validators = properties[$"{nameof(CompositeOrderRow.CompositeOrder)}{nameof(CompositeOrder.Number)}"].Validators;
            Assert.Equal(2, validators.Count);
            Assert.Equal(true, Assert.Single(validators.Where(o => o.Name == "fvNotEmpty"))?.ContainsKey("errorMessageId"));
            Assert.Single(validators.Where(o => o.Name == "integer"));

            type = (EntityType)metadata.StructuralTypes.First(o => o.Type == typeof(Order));
            properties = type.DataProperties.ToDictionary(o => o.NameOnServer);

            validator = Assert.Single(properties[nameof(Order.CreatedDate)].Validators);
            Assert.NotNull(validator);
            Assert.Equal("date", validator.Name);

            validator = Assert.Single(properties[nameof(Order.LastModifiedDate)].Validators);
            Assert.NotNull(validator);
            Assert.Equal("date", validator.Name);

            validators = properties[nameof(Order.Active)].Validators;
            Assert.Equal(2, validators.Count);
            Assert.Equal(true, Assert.Single(validators.Where(o => o.Name == "fvNotNull"))?.ContainsKey("errorMessageId"));
            Assert.Single(validators.Where(o => o.Name == "bool"));

            validators = properties[nameof(Order.Name)].Validators;
            Assert.Equal(2, validators.Count);
            Assert.Equal(true, Assert.Single(validators.Where(o => o.Name == "fvNotEmpty"))?.ContainsKey("errorMessageId"));
            validator = Assert.Single(validators.Where(o => o.Name == "fvLength"));
            Assert.NotNull(validator);
            Assert.True(validator.ContainsKey("errorMessageId"));
            Assert.Equal(0, validator["min"]);
            Assert.Equal(20, validator["max"]);

            type = (EntityType)metadata.StructuralTypes.First(o => o.Type == typeof(OrderProduct));
            properties = type.DataProperties.ToDictionary(o => o.NameOnServer);

            validators = properties[$"{nameof(OrderProduct.Order)}{nameof(Order.Id)}"].Validators;
            Assert.Equal(2, validators.Count);
            Assert.Equal(true, Assert.Single(validators.Where(o => o.Name == "fvNotEmpty"))?.ContainsKey("errorMessageId"));
            Assert.Single(validators.Where(o => o.Name == "integer"));

            validators = properties[$"{nameof(OrderProduct.Product)}{nameof(Product.Id)}"].Validators;
            Assert.Equal(2, validators.Count);
            Assert.Equal(true, Assert.Single(validators.Where(o => o.Name == "fvNotEmpty"))?.ContainsKey("errorMessageId"));
            Assert.Single(validators.Where(o => o.Name == "integer"));

            type = (EntityType)metadata.StructuralTypes.First(o => o.Type == typeof(ClientOrder));
            properties = type.DataProperties.ToDictionary(o => o.NameOnServer);

            validators = properties[nameof(ClientOrder.Id)].Validators;
            Assert.Equal(2, validators.Count);
            Assert.Equal(true, Assert.Single(validators.Where(o => o.Name == "fvNotNull"))?.ContainsKey("errorMessageId"));
            Assert.Single(validators.Where(o => o.Name == "integer"));

            validators = properties[nameof(ClientOrder.CreatedDate)].Validators;
            Assert.Equal(2, validators.Count);
            Assert.Equal(true, Assert.Single(validators.Where(o => o.Name == "fvNotNull"))?.ContainsKey("errorMessageId"));
            Assert.Single(validators.Where(o => o.Name == "date"));

            validator = Assert.Single(properties[$"{nameof(ClientOrder.MasterOrder)}{nameof(Order.Id)}"].Validators);
            Assert.NotNull(validator);
            Assert.Equal("integer", validator.Name);

            validator = Assert.Single(properties[$"{nameof(ClientOrder.MasterCompositeOrder)}{nameof(CompositeOrder.Number)}"].Validators);
            Assert.NotNull(validator);
            Assert.Equal("integer", validator.Name);

            type = (EntityType)metadata.StructuralTypes.First(o => o.Type == typeof(ClientOrderRow));
            properties = type.DataProperties.ToDictionary(o => o.NameOnServer);

            validators = properties[$"{nameof(ClientOrderRow.ClientOrder)}{nameof(ClientOrder.Id)}"].Validators;
            Assert.Equal(2, validators.Count);
            Assert.Equal(true, Assert.Single(validators.Where(o => o.Name == "fvNotEmpty"))?.ContainsKey("errorMessageId"));
            Assert.Single(validators.Where(o => o.Name == "integer"));

            validator = Assert.Single(properties[$"{nameof(ClientOrderRow.Product)}{nameof(Product.Id)}"].Validators);
            Assert.NotNull(validator);
            Assert.Equal("integer", validator.Name);
        }
    }
}
