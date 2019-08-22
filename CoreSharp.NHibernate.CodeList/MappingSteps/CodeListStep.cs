using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreSharp.DataAccess;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Steps;
using FluentNHibernate.Mapping;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.ClassBased;
using FluentNHibernate.MappingModel.Collections;
using FluentNHibernate.Utils;

namespace CoreSharp.NHibernate.CodeList.MappingSteps
{
    //Maps OneToMany for Names in ICodeList<>
    public class CodeListStep : IAutomappingStep
    {
        private readonly IAutomappingConfiguration _cfg;
        private readonly AutoKeyMapper _keys;

        public CodeListStep(IAutomappingConfiguration cfg)
        {
            _cfg = cfg;
            _keys = new AutoKeyMapper();
        }

        public bool ShouldMap(Member member)
        {
            if (!member.DeclaringType.IsGenericType || !member.DeclaringType.IsAssignableToGenericType(typeof(ILocalizableCodeList<,,>)))
            {
                return false;
            }

            var codeListType = member.DeclaringType.GetGenericArguments()[2];

            return
                member.Name == "Translations" &&
                typeof(ICodeList).IsAssignableFrom(codeListType) &&
                !typeof(INonLocalizableCodeList).IsAssignableFrom(codeListType) &&
                FluentNHibernate.Utils.Extensions.In(member.PropertyType.Namespace, "System.Collections.Generic", "Iesi.Collections.Generic") &&
                !member.PropertyType.HasInterface(typeof(IDictionary)) &&
                !member.PropertyType.ClosesInterface(typeof(IDictionary<,>)) &&
                !member.PropertyType.Closes(typeof(IDictionary<,>));
        }

        public void Map(ClassMappingBase classMap, Member member)
        {
            var collectionType = CollectionTypeResolver.Resolve(member);
            var mapping = CollectionMapping.For(collectionType);

            mapping.ContainingEntityType = classMap.Type;
            mapping.Member = member;
            mapping.Set(x => x.Name, Layer.Defaults, member.Name);
            mapping.Set(x => x.ChildType, Layer.Defaults, member.PropertyType.GetGenericArguments()[0]);

            SetDefaultAccess(member, mapping);
            SetRelationship(member, classMap, mapping);
            _keys.SetKey(member, classMap, mapping);
            var column = mapping.Key.Columns.FirstOrDefault();

            if (column != null)
            {
                column.Set(x => x.Name, Layer.UserSupplied, "CodeListId");
            }

            classMap.AddOrReplaceCollection(mapping);
        }

        void SetDefaultAccess(Member member, CollectionMapping mapping)
        {
            var resolvedAccess = MemberAccessResolver.Resolve(member);

            if (resolvedAccess != Access.Property && resolvedAccess != Access.Unset)
            {
                // if it's a property or unset then we'll just let NH deal with it, otherwise
                // set the access to be whatever we determined it might be
                mapping.Set(x => x.Access, Layer.Defaults, resolvedAccess.ToString());
            }

            if (member.IsProperty && !member.CanWrite)
            {
                mapping.Set(x => x.Access, Layer.Defaults, _cfg.GetAccessStrategyForReadOnlyProperty(member).ToString());
            }
        }

        static void SetRelationship(Member property, ClassMappingBase classMap, CollectionMapping mapping)
        {
            var relationship = new OneToManyMapping
            {
                ContainingEntityType = classMap.Type
            };

            relationship.Set(x => x.Class, Layer.Defaults, new TypeReference(property.PropertyType.GetGenericArguments()[0]));
            mapping.Set(x => x.Relationship, Layer.Defaults, relationship);
        }
    }
}
