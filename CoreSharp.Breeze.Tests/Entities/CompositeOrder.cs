using System;
using System.Collections.Generic;
using System.Text;
using CoreSharp.DataAccess;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace CoreSharp.Breeze.Tests.Entities
{
    public class CompositeOrder : CompositeEntity<CompositeOrder, long, int, string>
    {
        public virtual int Year { get; set; }

        public virtual long Number { get; set; }

        public virtual string Status { get; set; }

        public virtual decimal TotalPrice { get; set; }

        public virtual ISet<CompositeOrderRow> CompositeOrderRows { get; set; } = new HashSet<CompositeOrderRow>();

        protected override CompositeKey<CompositeOrder, long, int, string> CreateCompositeKey()
        {
            return new CompositeKey<CompositeOrder, long, int, string>(this, o => o.Number, o => o.Year, o => o.Status);
        }
    }

    public class CompositeOrderOverride : IAutoMappingOverride<CompositeOrder>
    {
        public void Override(AutoMapping<CompositeOrder> mapping)
        {
            mapping.CompositeId()
                .KeyProperty(x => x.Year)
                .KeyProperty(x => x.Number)
                .KeyProperty(x => x.Status);

            var rows = mapping.HasMany(o => o.CompositeOrderRows).Inverse();
            rows.KeyColumns.Add($"{nameof(CompositeOrder)}{nameof(CompositeOrder.Year)}");
            rows.KeyColumns.Add($"{nameof(CompositeOrder)}{nameof(CompositeOrder.Number)}");
            rows.KeyColumns.Add($"{nameof(CompositeOrder)}{nameof(CompositeOrder.Status)}");
        }
    }
}
