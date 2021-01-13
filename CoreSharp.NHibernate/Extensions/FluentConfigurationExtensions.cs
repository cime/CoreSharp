using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using CoreSharp.DataAccess;
using CoreSharp.NHibernate.Conventions;
using CoreSharp.NHibernate.EventListeners;
using CoreSharp.NHibernate.Interceptors;
using FluentNHibernate.Cfg;
using NHibernate.Connection;
using NHibernate.Event;
using NHibernate.Tool.hbm2ddl;
using SimpleInjector;

namespace CoreSharp.NHibernate.Extensions
{
    public static class FluentConfigurationExtensions
    {
        public static FluentConfiguration AppendEventListeners<TUser>(this FluentConfiguration fluentConfiguration, Container container)
            where TUser : IUser
        {
            return fluentConfiguration.ExposeConfiguration(cfg =>
            {
                cfg.SetInterceptor(new NHibernateInterceptor());

                cfg.EventListeners.PreUpdateEventListeners =
                    cfg.EventListeners.PreUpdateEventListeners.Prepend(container
                        .GetInstance<VersionedEntityPreUpdateEventListener<TUser>>());
                cfg.EventListeners.DeleteEventListeners =
                    cfg.EventListeners.DeleteEventListeners.Prepend(container.GetInstance<DeleteEntityEventListener>());
                cfg.EventListeners.SaveOrUpdateEventListeners = new ISaveOrUpdateEventListener[]
                    {container.GetInstance<VersionedEntitySaveOrUpdateEventListener<TUser>>()};
                cfg.EventListeners.SaveEventListeners = new ISaveOrUpdateEventListener[]
                    {container.GetInstance<VersionedEntitySaveOrUpdateEventListener<TUser>>()};
                cfg.EventListeners.UpdateEventListeners = new ISaveOrUpdateEventListener[]
                    {container.GetInstance<VersionedEntitySaveOrUpdateEventListener<TUser>>()};

                cfg.EventListeners.PostInsertEventListeners =
                    cfg.EventListeners.PostInsertEventListeners.Append(container
                        .GetInstance<PostEntityActionEventListener>()).ToArray();
                cfg.EventListeners.PostUpdateEventListeners =
                    cfg.EventListeners.PostUpdateEventListeners.Append(container
                        .GetInstance<PostEntityActionEventListener>()).ToArray();
                cfg.EventListeners.PostDeleteEventListeners =
                    cfg.EventListeners.PostDeleteEventListeners.Append(container
                        .GetInstance<PostEntityActionEventListener>()).ToArray();
            });
        }

        public static FluentConfiguration AddConventions(this FluentConfiguration fluentConfiguration, IDatabaseTypeAccessor databaseTypeAccessor)
        {
            var cfg = (global::NHibernate.Cfg.Configuration)fluentConfiguration.GetMemberValue("cfg");

            return fluentConfiguration.Mappings(m =>
            {
                foreach (var persistenceModel in m.AutoMappings)
                {
                    persistenceModel.Conventions.AddFromAssemblyOf<NotNullConvention>();
                    persistenceModel.Conventions.Add(typeof(FormulaAttributeConvention), new FormulaAttributeConvention(cfg, databaseTypeAccessor));
                }
            });
        }

        public static FluentConfiguration CreateSchema(this FluentConfiguration fluentConfiguration, bool create = true)
        {
            if (!create)
            {
                return fluentConfiguration;
            }

            return fluentConfiguration.ExposeConfiguration(cfg =>
            {
                var schemaExport = new SchemaExport(cfg);
                schemaExport.Drop(true, true);
                schemaExport.Create(true, true);
            });
        }

        public static FluentConfiguration UpdateSchema(this FluentConfiguration fluentConfiguration, bool update = true)
        {
            if (!update)
            {
                return fluentConfiguration;
            }

            return fluentConfiguration.ExposeConfiguration(cfg =>
            {
                new SchemaUpdate(cfg).Execute(true, true);
            });
        }

        public static FluentConfiguration SetDefaultProperties(this FluentConfiguration fluentConfiguration)
        {
            return fluentConfiguration.ExposeConfiguration(cfg =>
            {
                cfg.SetProperty(global::NHibernate.Cfg.Environment.BatchSize, "100");
                //cfg.SetProperty(global::NHibernate.Cfg.Environment.BatchStrategy, typeof(NonBatchingBatcherFactory).FullName);
                cfg.SetProperty(global::NHibernate.Cfg.Environment.Hbm2ddlKeyWords, "auto-quote");
                //cfg.SetProperty(global::NHibernate.Cfg.Environment.DefaultBatchFetchSize, "20");
                cfg.SetProperty(global::NHibernate.Cfg.Environment.CacheDefaultExpiration, "86400");
            });
        }

        public static FluentConfiguration ExportMappings(this FluentConfiguration fluentConfiguration, string path, bool export = true)
        {
            if (!export)
            {
                return fluentConfiguration;
            }

            return fluentConfiguration.Mappings(m =>
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                m.AutoMappings.ExportTo(path);
            });
        }

        public static FluentConfiguration ExposeDbCommand(this FluentConfiguration fluentConfiguration, Action<IDbCommand, global::NHibernate.Cfg.Configuration> action)
        {
            fluentConfiguration.ExposeConfiguration(cfg =>
            {
                DbConnection connection = null;
                IConnectionProvider provider = null;
                IDbCommand command = null;

                try
                {
                    provider = GetConnectionProvider(cfg);
                    connection = provider.GetConnection();
                    command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;

                    action(command, cfg);
                }
                finally
                {
                    if (command != null)
                    {
                        command.Dispose();
                    }

                    if (connection != null)
                    {
                        provider.CloseConnection(connection);
                        provider.Dispose();
                    }
                }
            });

            return fluentConfiguration;
        }

        private static IConnectionProvider GetConnectionProvider(global::NHibernate.Cfg.Configuration config)
        {
            var settings = new Dictionary<string, string>();
            var dialect = global::NHibernate.Dialect.Dialect.GetDialect(config.Properties);
            foreach (var pair in dialect.DefaultProperties)
            {
                settings[pair.Key] = pair.Value;
            }

            if (config.Properties != null)
            {
                foreach (var pair2 in config.Properties)
                {
                    settings[pair2.Key] = pair2.Value;
                }
            }

            return ConnectionProviderFactory.NewConnectionProvider(settings);
        }

        private static T[] Prepend<T>(this IEnumerable<T> collection, T instance)
        {
            return new T[] { instance }.Union(collection).ToArray();
        }
    }
}
