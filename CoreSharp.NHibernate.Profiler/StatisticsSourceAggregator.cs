using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace CoreSharp.NHibernate.Profiler
{
    public class StatisticsSourceAggregator
    {
        private readonly IDictionary<object, SessionFactoryStatisticsSource> _statSources = new Dictionary<object, SessionFactoryStatisticsSource>();

        private readonly PropertyInfo _statisticsProperty;
        private readonly FieldInfo _sessionFactoryName;
        private readonly PropertyInfo[] _statsProperties;
        private readonly IDictionary _instances;
        
        public StatisticsSourceAggregator()
        {
            _instances = (IDictionary)typeof(global::NHibernate.Impl.SessionFactoryObjectFactory)?.GetField("Instances", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);

            if (_instances != null)
            {
                foreach (var instance in _instances.Values)
                {
                    var settings = instance.GetType().GetProperty("Settings")?.GetValue(instance, new object[0]);
                    var property = settings?.GetType().GetProperty("IsStatisticsEnabled");

                    if (property != null)
                    {
                        property.SetValue(settings, true, null);
                    }
                }
            }
            
            var type = typeof(global::NHibernate.Impl.SessionFactoryImpl);
            _statisticsProperty = type.GetProperty("Statistics");
            _sessionFactoryName = type.GetField("name", BindingFlags.Instance | BindingFlags.NonPublic);
            _statsProperties = typeof(global::NHibernate.Stat.StatisticsImpl).Assembly.GetType("NHibernate.Stat.StatisticsImpl").GetProperties();
        }
        
        public void RefreshSessionFactoryStatisticsSources()
        {
            if (_statisticsProperty == null || _sessionFactoryName == null || _statsProperties == null)
            {
                return;
            }
            
            var arrayList = new ArrayList(_instances.Values);
            foreach (var obj in arrayList)
            {
                if (!_statSources.ContainsKey(obj))
                {
                    var statisticsSource = new SessionFactoryStatisticsSource(_statisticsProperty, _sessionFactoryName, obj, _statsProperties);
                    _statSources.Add(obj, statisticsSource);
                }
            }
            foreach (var key in new List<object>(_statSources.Keys))
            {
                if (!arrayList.Contains(key))
                {
                    _statSources.Remove(key);
                }
            }
        }

        public SessionFactoryStats[] GetStatistics()
        {
            RefreshSessionFactoryStatisticsSources();
            var sessionFactoryStatsList = new List<SessionFactoryStats>();
            
            foreach (var statisticsSource in _statSources.Values)
            {
                var statistics = statisticsSource.GetStatistics();
                if (statistics != null)
                {
                    sessionFactoryStatsList.Add(statistics);
                }
            }
            
            return sessionFactoryStatsList.ToArray();
        }
    }
}