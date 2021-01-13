namespace System.Collections.Generic
{
    public static class EnumeratorExtensions
    {

        public class Enumerable<T> : IEnumerable<T>
        {

            private readonly IEnumerator<T> _enumerator;

            internal Enumerable(IEnumerator<T> enumerator)
            {
                _enumerator = enumerator;
            }
            public IEnumerator<T> GetEnumerator()
            {
                return _enumerator;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _enumerator;
            }
        }

        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
        {
            var enumerable = new Enumerable<T>(enumerator);
            return enumerable;
        }
    }
}
