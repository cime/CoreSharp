using System.Collections.Generic;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.ClassBased;
using FluentNHibernate.MappingModel.Collections;

namespace CoreSharp.NHibernate.Visitors
{
    internal interface INHibernateMappingVisitor
    {
        void Visit(IEnumerable<HibernateMapping> mappings);
        void VisitClass(ClassMapping classMapping);
        void VisitClassBase(ClassMappingBase classMapping);
        void VisitOneToOne(OneToOneMapping one);
        void VisitJoin(JoinMapping @join);
        void VisitAny(AnyMapping anyMapping);
        void VisitComponent(IComponentMapping componentMapping);
        void VisitSubclass(SubclassMapping subClass);
        void VisitCollection(CollectionMapping collection);
        void VisitReference(ManyToOneMapping reference);
    }
}
