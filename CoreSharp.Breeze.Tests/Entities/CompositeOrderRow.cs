using CoreSharp.Common.Attributes;
using CoreSharp.NHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace CoreSharp.Breeze.Tests.Entities
{
    public class CompositeOrderRow : Entity
    {
        [NotNull]
        public virtual CompositeOrder CompositeOrder { get; set; }

        [NotNull]
        public virtual Product Product { get; set; }

        public virtual decimal Price { get; set; }

        public virtual int Quantity { get; set; }
    }

    public class CompositeOrderRowOverride : IAutoMappingOverride<CompositeOrderRow>
    {
        public void Override(AutoMapping<CompositeOrderRow> mapping)
        {
            mapping.References(o => o.CompositeOrder)
                .Columns(
                    $"{nameof(CompositeOrder)}{nameof(CompositeOrder.Year)}",
                    $"{nameof(CompositeOrder)}{nameof(CompositeOrder.Number)}",
                    $"{nameof(CompositeOrder)}{nameof(CompositeOrder.Status)}"
                );
        }
    }
}
