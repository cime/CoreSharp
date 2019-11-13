using System.Collections;

namespace CoreSharp.NHibernate.Profiler
{
    public class SessionFactoryStats
    {
        public string Name { get; set; }

        public Hashtable Statistics { get; set; }

        public bool Equals(SessionFactoryStats other)
        {
            if (other == null)
            {
                return false;
            }

            if (this == other)
            {
                return true;
            }

            if (Equals(other.Name, Name))
            {
                return Equals(other.Statistics, Statistics);
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (this == obj)
            {
                return true;
            }

            return Equals(obj as SessionFactoryStats);
        }

        public override int GetHashCode()
        {
            var num = Name.GetHashCode();

            foreach (DictionaryEntry statistic in Statistics)
            {
                num = num * 397 ^ statistic.Key.GetHashCode();
                var obj1 = statistic.Value;
                if (obj1 is IEnumerable enumerable)
                {
                    foreach (var obj2 in enumerable)
                    {
                        if (obj2 != null)
                            num = num * 397 ^ obj2.GetHashCode();
                    }
                }
                else if (obj1 != null)
                    num = num * 397 ^ obj1.GetHashCode();
            }
            return num;
        }
    }
}