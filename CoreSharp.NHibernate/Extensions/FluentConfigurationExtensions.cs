using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreSharp.DataAccess;
using CoreSharp.NHibernate.Conventions.Mssql;
using CoreSharp.NHibernate.EventListeners;
using CoreSharp.NHibernate.Interceptors;
using FluentNHibernate.Cfg;
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
            });
        }

        public static FluentConfiguration CreateHiLoSchema(this FluentConfiguration fluentConfiguration)
        {
            return fluentConfiguration.ExposeConfiguration(cfg =>
            {
                MssqlHiLoIdConvention.SchemaCreate(cfg);
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
                new SchemaExport(cfg).Execute(true, true, false);
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

        public static FluentConfiguration ExportMappings(this FluentConfiguration fluentConfiguration, string path)
        {
            return fluentConfiguration.Mappings(m =>
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    Directory.CreateDirectory(path);

                    m.AutoMappings.ExportTo(path);
                }
            });
        }

        private static T[] Prepend<T>(this IEnumerable<T> collection, T instance)
        {
            return new T[] { instance }.Union(collection).ToArray();
        }
    }
}
