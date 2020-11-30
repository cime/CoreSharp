using System.Diagnostics;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    public static class EnumerableExtensions
    {
        private static readonly Random Random = new Random();

#nullable disable
        public static T GetRandom<T>(this IEnumerable<T> sequence)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException();
            }

            var enumerable = sequence as IList<T> ?? sequence.ToList();
            if (!enumerable.Any())
            {
                throw new ArgumentException("The sequence is empty.");
            }

            //optimization for ICollection<T>
            var collection = sequence as ICollection<T>;
            if (collection != null)
            {
                return collection.ElementAt(Random.Next(collection.Count));
            }

            var count = 1;
            var selected = default(T);

            foreach (var element in enumerable)
            {
                if (Random.Next(count++) == 0)
                {
                    //Select the current element with 1/count probability
                    selected = element;
                }
            }

            return selected;
        }
#nullable enable

        [DebuggerStepThrough]
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (TSource source1 in source)
            {
                action(source1);
            }
        }

        [DebuggerStepThrough]
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<int, TSource> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var num = 0;

            foreach (TSource source1 in source)
            {
                action(num, source1);
                ++num;
            }
        }

        public static IEnumerable<IGrouping<TKey, TSource>> Duplicates<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.GroupBy(keySelector).Where(o => o.Count() > 1);
        }

        public static IEnumerable<Tuple<T1, T2>> CrossJoin<T1, T2>(this IEnumerable<T1> sequence1, IEnumerable<T2> sequence2)
        {
            return sequence1.SelectMany(t1 => sequence2.Select(t2 => Tuple.Create(t1, t2)));
        }

        public static IEnumerable<TCol> Except<TCol, TAdd>(this IEnumerable<TCol> collection, IEnumerable<TAdd> buffer, Func<TCol, TAdd, bool> comparer)
        {
            var except = collection.Where(c => !buffer.Any(b => comparer(c, b))).ToList();
            return except;
        }

        public static IEnumerable<TCol> Intersect<TCol, TAdd>(this IEnumerable<TCol> collection, IEnumerable<TAdd> buffer, Func<TCol, TAdd, bool> comparer)
        {
            var intersection = collection.Where(c => buffer.Any(b => comparer(c, b))).ToList();
            return intersection;
        }
    }
}
