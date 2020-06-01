using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping;

namespace CoreSharp.NHibernate.PostgreSQL.Conventions
{
    [Priority(Priority.Low)]
    public class PostgreSqlHiLoIdConvention : IClassConvention, IIdConvention
    {
        private readonly global::NHibernate.Cfg.Configuration _configuration;
        private readonly INamingStrategy _namingStrategy;
        internal const string NextHiValueColumnName = "next_hi_value";
        internal const string TableColumnName = "entity";
        internal static readonly string HiLoIdentityTableName = "public.hi_lo_identity"; // TODO: configurable schema
        private static readonly Type[] ValidTypes = new [] { typeof(int), typeof(long), typeof(uint), typeof(ulong) };
        private static readonly HashSet<string> ValidDialects = new HashSet<string>
            {
                typeof (PostgreSQL81Dialect).FullName,
                typeof (PostgreSQL82Dialect).FullName,
                typeof (PostgreSQL83Dialect).FullName,
                typeof (PostgreSQLDialect).FullName
            };
        private static readonly ConcurrentDictionary<Type, IClassInstance> ClassInstances = new ConcurrentDictionary<Type, IClassInstance>();
        private static readonly ConcurrentDictionary<Type, string> FullNames = new ConcurrentDictionary<Type, string>();
        private readonly string _maxLo = "1000";

        public PostgreSqlHiLoIdConvention(global::NHibernate.Cfg.Configuration configuration)
        {
            _configuration = configuration;
            _namingStrategy = configuration.NamingStrategy;

            var maxLo = configuration.GetProperty("hilo_generator.max_lo");

            if (!string.IsNullOrEmpty(maxLo))
            {
                _maxLo = maxLo;
            }
        }

        public void Apply(IClassInstance instance)
        {
            ClassInstances[instance.EntityType] = instance;
        }

        public void Apply(IIdentityInstance instance)
        {
            if (instance.Property == null || !ValidTypes.Contains(instance.Property.PropertyType))
            {
                return;
            }

            var maxLo = _maxLo;
            var hiLoAttribute = instance.EntityType.GetCustomAttribute<HiLoAttribute>();

            if (hiLoAttribute != null && hiLoAttribute.Size > 0)
            {
                maxLo = hiLoAttribute.Size.ToString();
            }

            var classInstance = ClassInstances.ContainsKey(instance.EntityType) ? ClassInstances[instance.EntityType] : null;
            var fullName = GetFullName(classInstance);
            FullNames[instance.EntityType] = fullName;

            instance.GeneratedBy.HiLo(HiLoIdentityTableName, NextHiValueColumnName, maxLo, builder =>
                builder.AddParam("where", $"{TableColumnName} = '{fullName}'"));
        }

        public string GetFullName(IClassInstance classInstance)
        {
            return string.Join(".", new [] { _namingStrategy.TableName(((IClassInspector)classInstance).Schema ?? "public"), _namingStrategy.TableName(classInstance.TableName.TrimStart('`').TrimEnd('`')) }
                .Where(x => !string.IsNullOrEmpty(x)));
        }

        public static void SchemaCreate(global::NHibernate.Cfg.Configuration config)
        {
            var script = new StringBuilder();
            script.AppendFormat("DELETE FROM {0};", HiLoIdentityTableName);
            script.AppendLine();
            script.AppendFormat("ALTER TABLE {0} ADD COLUMN {1} VARCHAR(128) NOT NULL;", HiLoIdentityTableName, TableColumnName);
            script.AppendLine();
            script.AppendFormat("CREATE INDEX ix_{0}_{1} ON {0} ({1});", HiLoIdentityTableName.Replace("public.", ""), TableColumnName);
            script.AppendLine();
            script.AppendFormat("CREATE UNIQUE INDEX ux_{0}_{1} ON {0} (entity);", HiLoIdentityTableName.Replace("public.", ""), TableColumnName);
            script.AppendLine();

            var fullNames = FullNames.Values.ToList();

            foreach (var entityName in fullNames)
            {
                script.AppendFormat("INSERT INTO {0} ({1}, {2}) VALUES ('{3}', 0);", HiLoIdentityTableName, TableColumnName, NextHiValueColumnName, entityName);
                script.AppendLine();
            }

            config.AddAuxiliaryDatabaseObject(new SimpleAuxiliaryDatabaseObject(script.ToString(), null, ValidDialects));
        }
    }
}
