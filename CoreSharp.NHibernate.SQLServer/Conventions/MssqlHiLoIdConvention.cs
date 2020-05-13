﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using NHibernate.Dialect;
using NHibernate.Mapping;

namespace CoreSharp.NHibernate.SQLServer.Conventions
{
    public class MssqlHiLoIdConvention : IIdConvention
    {
        private const string NextHiValueColumnName = "NextHiValue";
        private const string TableColumnName = "Entity";
        private static readonly string HiLoIdentityTableName = "HiLoIdentity";
        private static readonly string MaxLo = "1000";
        private static readonly Type[] ValidTypes = new [] { typeof(int), typeof(long), typeof(uint), typeof(ulong) };
        private static readonly HashSet<string> ValidDialects = new HashSet<string>
            {
                typeof (MsSql2000Dialect).FullName,
                typeof (MsSql2005Dialect).FullName,
                typeof (MsSql2008Dialect).FullName,
                typeof (MsSql2012Dialect).FullName,
                "NHibernate.Spatial.Dialect.MsSql2008GeometryDialect",
                "NHibernate.Spatial.Dialect.MsSql2008GeographyDialect",
                "NHibernate.Spatial.Dialect.MsSql2012GeometryDialect",
                "NHibernate.Spatial.Dialect.MsSql2012GeographyDialect"
            };

        public void Apply(IIdentityInstance instance)
        {
            if (instance.Property == null || !ValidTypes.Contains(instance.Property.PropertyType))
            {
                return;
            }

            var maxLo = MaxLo;
            var hiLoAttribute = instance.EntityType.GetCustomAttribute<HiLoAttribute>();

            if (hiLoAttribute != null && hiLoAttribute.Size > 0)
            {
                maxLo = hiLoAttribute.Size.ToString();
            }

            instance.GeneratedBy.HiLo(HiLoIdentityTableName, NextHiValueColumnName, maxLo, builder =>
                builder.AddParam("WHERE", $"{TableColumnName} = '[{instance.EntityType.Name}]'"));
        }

        public static void SchemaCreate(global::NHibernate.Cfg.Configuration config)
        {
            var script = new StringBuilder();
            script.AppendFormat("DELETE FROM {0};", HiLoIdentityTableName);
            script.AppendLine();
            script.AppendFormat("ALTER TABLE {0} ADD {1} VARCHAR(128) NOT NULL;", HiLoIdentityTableName, TableColumnName);
            script.AppendLine();
            script.AppendFormat("CREATE NONCLUSTERED INDEX IX_{0}_{1} ON {0} (Entity DESC);", HiLoIdentityTableName, TableColumnName);
            script.AppendLine();
            script.AppendLine("GO");
            script.AppendLine();

            foreach (var entityName in config.ClassMappings.Select(m => m.MappedClass != null ? m.MappedClass.Name : m.Table.Name).Distinct())
            {
                script.AppendFormat("INSERT INTO [{0}] ({1}, {2}) VALUES ('[{3}]', 0);", HiLoIdentityTableName, TableColumnName, NextHiValueColumnName, entityName);
                script.AppendLine();
            }

            config.AddAuxiliaryDatabaseObject(new SimpleAuxiliaryDatabaseObject(script.ToString(), null, ValidDialects));
        }
    }
}
