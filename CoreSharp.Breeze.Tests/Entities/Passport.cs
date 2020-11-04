using System;
using CoreSharp.NHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace CoreSharp.Breeze.Tests.Entities
{
    public class Passport : VersionedEntity
    {
        public Passport()
        {
            CreatedDate = DateTime.UtcNow;
        }

        public virtual long Number { get; set; }

        public virtual DateTime ExpirationDate { get; set; }

        public virtual string Country { get; set; }

        public virtual Person Owner { get; set; }
    }

    public class PassportMapping : IAutoMappingOverride<Passport>
    {
        public void Override(AutoMapping<Passport> mapping)
        {
            mapping.HasOne(o => o.Owner).Constrained().ForeignKey("none").PropertyRef(o => o.Passport);
        }
    }
}
