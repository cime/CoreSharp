using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CoreSharp.Breeze.Tests.Entities
{
    public interface ICompositeKey
    {
        IEnumerable<object> GetKeyValues();
    }

    public class CompositeKey<TType, TCol1, TCol2, TCol3, TCol4> : CompositeKey<TType, TCol1, TCol2, TCol3>
    {
        public CompositeKey(TType entity, Expression<Func<TType, TCol1>> column1Expression, Expression<Func<TType, TCol2>> column2Expression,
            Expression<Func<TType, TCol3>> column3Expression, Expression<Func<TType, TCol4>> column4Expression)
            : base(entity, column1Expression, column2Expression, column3Expression)
        {
            Column4 = column4Expression.Compile();
            AddColumn(type => Column4(type), typeof(TCol4));
        }

        public Func<TType, TCol4> Column4 { get; }
    }

    public class CompositeKey<TType, TCol1, TCol2, TCol3> : CompositeKey<TType, TCol1, TCol2>
    {
        public CompositeKey(TType entity, Expression<Func<TType, TCol1>> column1Expression, Expression<Func<TType, TCol2>> column2Expression,
            Expression<Func<TType, TCol3>> column3Expression)
            : base(entity, column1Expression, column2Expression)
        {
            Column3 = column3Expression.Compile();
            AddColumn(type => Column3(type), typeof(TCol3));
        }

        public Func<TType, TCol3> Column3 { get; }
    }

    public class CompositeKey<TType, TCol1, TCol2> : ICompositeKey
    {
        private readonly Dictionary<Func<TType, object>, Type> _columnDict = new Dictionary<Func<TType, object>, Type>();

        public CompositeKey(TType entity, Expression<Func<TType, TCol1>> column1Expression, Expression<Func<TType, TCol2>> column2Expression)
        {
            Entity = entity;
            Column1 = column1Expression.Compile();
            Column2 = column2Expression.Compile();

            AddColumn(type => Column1(type), typeof(TCol1));
            AddColumn(type => Column2(type), typeof(TCol2));
        }

        public TType Entity { get; }

        public Func<TType, TCol1> Column1 { get; }

        public Func<TType, TCol2> Column2 { get; }

        protected void AddColumn(Func<TType, object> colFunc, Type colType)
        {
            _columnDict.Add(colFunc, colType);
        }

        public IEnumerable<object> GetKeyValues()
        {
            return _columnDict.Select(o => o.Key(Entity));
        }

        public static bool operator ==(CompositeKey<TType, TCol1, TCol2> c1, CompositeKey<TType, TCol1, TCol2> c2)
        {
            return !(c1 == null) && c1.Equals(c2);
        }

        public static bool operator !=(CompositeKey<TType, TCol1, TCol2> c1, CompositeKey<TType, TCol1, TCol2> c2)
        {
            if (c1 == null)
            {
                return true;
            }

            return !c1.Equals(c2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            var item = (ICompositeKey)obj;

            var itemKeys = item.GetKeyValues();
            var keys = GetKeyValues();

            using var itemEnumerator = itemKeys.GetEnumerator();
            using var enumerator = keys.GetEnumerator();
            while (itemEnumerator.MoveNext() && enumerator.MoveNext())
            {
                if (!Equals(itemEnumerator.Current, enumerator.Current))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = GetType().GetHashCode() * 31;
                foreach (var val in GetKeyValues())
                {
                    hashCode ^= val.GetHashCode();
                }

                return hashCode;
            }
        }
    }
}
