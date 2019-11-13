using System;
using System.Collections;
using System.Reflection;

namespace CoreSharp.NHibernate.Profiler
{
    public class SessionFactoryStatisticsSource
    {
        private readonly object _sessionFactory;
        private readonly FieldInfo _sessionFactoryName;
        private readonly PropertyInfo _statisticsProperty;
        private readonly PropertyInfo[] _statsProperties;
        private int _lastStatisticsHash;

        public SessionFactoryStatisticsSource(PropertyInfo statisticsProperty, FieldInfo sessionFactoryName,
            object sessionFactory, PropertyInfo[] statsProperties)
        {
            _statisticsProperty = statisticsProperty;
            _statsProperties = statsProperties;
            _sessionFactoryName = sessionFactoryName;
            _sessionFactory = sessionFactory;
        }

        public SessionFactoryStats GetStatistics()
        {
            var stats = _statisticsProperty.GetValue(_sessionFactory, new object[0]);
            var str = (string) _sessionFactoryName.GetValue(_sessionFactory) ?? "unnamed";
            var sessionFactoryStats = new SessionFactoryStats
            {
                Name = str,
                Statistics = StatsToDictionary(stats)
            };
            
            var hashCode = sessionFactoryStats.GetHashCode();
            
            if (_lastStatisticsHash == hashCode)
            {
                return null;
            }
            
            _lastStatisticsHash = hashCode;
            
            return sessionFactoryStats;
        }

        private Hashtable StatsToDictionary(object stats)
        {
            if (stats == null) return new Hashtable();
            var hashtable = new Hashtable();
            for (var index = 0; index < _statsProperties.Length; ++index)
            {
                var statsProperty = _statsProperties[index];
                if (!(statsProperty == null)) {
                    if (statsProperty.CanRead)
                    {
                        try
                        {
                            hashtable[statsProperty.Name] = statsProperty.GetValue(stats, new object[0]);
                        }
                        catch (Exception ex)
                        {
                            _statsProperties[index] = null;
                        }
                    }
                }
            }

            return hashtable;
        }
    }
}
