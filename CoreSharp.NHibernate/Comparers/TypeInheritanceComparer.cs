using System;
using System.Collections.Generic;
using System.Reflection;

namespace CoreSharp.NHibernate.Comparers
{
    internal class TypeInheritanceComparer : IComparer<Type>
    {
        public int Compare(Type x, Type y)
        {
            if (x == y)
            {
                return 0;
            }
            if (x.GetTypeInfo().IsAssignableToGenericType(y))
            {
                return 1;
            }
            return -1;
        }
    }
}
