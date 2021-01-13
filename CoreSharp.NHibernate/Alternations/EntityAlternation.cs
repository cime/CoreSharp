using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace CoreSharp.NHibernate.Alternations
{
    public class EntityAlternation : IAutoMappingAlteration
    {
        public void Alter(AutoPersistenceModel model)
        {
            model.IgnoreBase(typeof(Entity<>));
            model.IgnoreBase(typeof(Entity));
            model.IgnoreBase(typeof(VersionedEntity<>));
            model.IgnoreBase(typeof(VersionedEntity));
            model.IgnoreBase(typeof(VersionedEntityWithUser<>));
            model.IgnoreBase(typeof(VersionedEntityWithUser<,>));
            model.IgnoreBase(typeof(Document<,,,>));
            model.IgnoreBase(typeof(DocumentVersion<,,,>));
        }
    }
}
