using System;
using System.Collections.Generic;
using System.Text;
using CoreSharp.NHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace CoreSharp.Breeze.Tests.Entities
{
    public class IdentityCard : VersionedEntity
    {
        public IdentityCard()
        {
            CreatedDate = DateTime.UtcNow;
        }

        public virtual string Code { get; set; }

        public virtual Person Owner { get; set; }

        public virtual void SetOwner(Person owner)
        {
            Id = owner.Id;
            Owner = owner;
        }
    }

    public class IdentityCard5Mapping : IAutoMappingOverride<IdentityCard>
    {
        public void Override(AutoMapping<IdentityCard> mapping)
        {
            mapping.Id(o => o.Id).GeneratedBy.Foreign("Owner");
            mapping.HasOne(o => o.Owner).Constrained();
        }
    }
}
