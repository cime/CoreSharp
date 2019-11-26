using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreSharp.Common.Exceptions;
using CoreSharp.Cqrs.Events;
using CoreSharp.DataAccess;
using CoreSharp.NHibernate.CodeList.Attributes;
using CoreSharp.NHibernate.Events;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.ClassBased;
using NHibernate.Cfg;
using NHibernate.Dialect;

namespace CoreSharp.NHibernate.CodeList.EventHandlers
{
    public class CodeListsMappingsBuiltEventHandler : IEventHandler<MappingsBuiltEvent>
    {
        // TODO: configurable...COALESCE works everywhere but is slower than ISNULL which is available only for sql server
        // TODO: COALESCE executes both expression and replacement - try with CASE
        private static readonly string _localizeFormula =
            "(SELECT COALESCE((SELECT curr.{0} FROM {1} curr WHERE curr.{5} = {7} AND curr.{6} = :{3}.{4})," +
            "(SELECT def.{0} FROM {1} def WHERE def.{5} = {7} AND def.{6} = :{3}.{2})))";

        public INamingStrategy NamingStrategy { get; private set; }

        public Dialect Dialect { get; private set; }

        public void Apply(ClassMappingBase classMapBase, Lazy<Dictionary<Type, ClassMapping>> lazyTypeMap)
        {
            var type = classMapBase.Type;

            var classMap = classMapBase as ClassMapping;
            if (classMap == null)
            {
                return;
            }

            if (typeof(ILocalizableCodeListTranslation).IsAssignableFrom(type))
            {
                return; // for localization table we set only the table name
            }

            foreach (var propMap in classMap.Properties)
            {
                var attr = propMap.Member.MemberInfo.GetCustomAttribute<FilterCurrentLanguageAttribute>(false);
                if (attr != null)
                {
                    var names =
                        classMap.Collections.FirstOrDefault(
                            o => typeof(ILocalizableCodeListTranslation).IsAssignableFrom(o.ChildType));
                    if (names == null)
                    {
                        throw new CoreSharpException("FilterCurrentLanguage must be applied on a type that implements ILocalizableCodeListTranslation");
                    }
                    if (!lazyTypeMap.Value.ContainsKey(names.ChildType))
                    {
                        throw new CoreSharpException($"Mapping for codelist {names.ChildType} was not found, failed to apply the formula for the FilterCurrentLanguage attribute");
                    }
                    // Here the table name is already altered
                    var childMapping = lazyTypeMap.Value[names.ChildType];
                    var childTableName = string.IsNullOrEmpty(childMapping.Schema) ? childMapping.TableName : $"{childMapping.Schema}.{childMapping.TableName}";
                    propMap.Set(o => o.Formula, Layer.UserSupplied,
                        string.Format(_localizeFormula,
                            attr.ColumnName ?? ConvertQuotes(GetColumnName(propMap.Name)),
                            childTableName,
                            attr.FallbackLanguageParameterName,
                            attr.FilterName,
                            attr.CurrentLanguageParameterName,
                            GetColumnName("CodeListId"),
                            ConvertQuotes(GetColumnName("LanguageId")),
                            ConvertQuotes(GetColumnName("Id"))));
                }
            }
        }

        protected string GetColumnName(string name)
        {
            return NamingStrategy.ColumnName(name);
        }

        protected virtual string ConvertQuotes(string name)
        {
            if (name.StartsWith("`") || name.EndsWith("`"))
            {
                return $"{Dialect.OpenQuote}{name.Trim('`')}{Dialect.CloseQuote}";
            }
            return name;
        }

        public void Handle(MappingsBuiltEvent e)
        {
            NamingStrategy = e.Configuration.NamingStrategy;
            Dialect = Dialect.GetDialect(e.Configuration.Properties);
            var lazyTypeMap = new Lazy<Dictionary<Type, ClassMapping>>(() =>
            {
                return e.Mappings.SelectMany(o => o.Classes).ToDictionary(o => o.Type);
            });

            var classes = e.Mappings.SelectMany(o => o.Classes);
            var codeListClasses = classes.Where(x => typeof(ICodeList).IsAssignableFrom(x.Type));
            var translationsClasses = classes.Where(x => typeof(ILocalizableCodeListTranslation).IsAssignableFrom(x.Type));

            foreach (var classMap in translationsClasses.Union(codeListClasses))
            {
                Apply(classMap, lazyTypeMap);
            }
        }
    }
}
