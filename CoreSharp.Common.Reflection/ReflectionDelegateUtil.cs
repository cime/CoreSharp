using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CoreSharp.Common.Reflection
{
    public static class ReflectionDelegateUtil
    {

        public static IDictionary<string, Func<object, object>> CreatePropertiesGenericGetters<TDoc>()
        {
            var createMethodInfo = typeof(ReflectionDelegateUtil).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(x => x.Name == nameof(CreateGetterGenericDelegate));

            var delegates = typeof(TDoc).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead)
                .ToDictionary(x => x.Name, x => createMethodInfo.MakeGenericMethod(typeof(TDoc), x.PropertyType).Invoke(null, new object[] { x }) as Func<object, object>);
            return delegates;
        }

        public static Func<TDoc, TProp> CreateGetterDelegate<TDoc, TProp>(PropertyInfo prop)
        {
            var dlgt = Delegate.CreateDelegate(typeof(Func<TDoc, TProp>), null, prop.GetGetMethod());
            return (Func<TDoc, TProp>)dlgt;
        }

        public static Func<object, object> CreateGetterGenericDelegate<TDoc, TProp>(PropertyInfo prop)
        {
            var dlgt = CreateGetterDelegate<TDoc, TProp>(prop);
            return new Func<object, object>(doc => {
                var d = (TDoc) doc;
                if(d != null)
                {
                    return dlgt(d);
                }
                return null;
            });
        }

        public static IDictionary<string, Action<object, object>> CreatePropertiesGenericSetters<TDoc>()
        {
            var createMethodInfo = typeof(ReflectionDelegateUtil).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(x => x.Name == nameof(CreateSetterGenericDelegate));

            var delegates = typeof(TDoc).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite)
                .ToDictionary(x => x.Name, x => createMethodInfo.MakeGenericMethod(typeof(TDoc), x.PropertyType).Invoke(null, new object[] { x }) as Action<object, object>);
            return delegates;
        }

        public static Action<TDoc, TProp> CreateSetterDelegate<TDoc, TProp>(PropertyInfo prop)
        {
            var dlgt = Delegate.CreateDelegate(typeof(Action<TDoc, TProp>), null, prop.GetSetMethod());
            return (Action<TDoc, TProp>)dlgt;
        }

        public static Action<object, object> CreateSetterGenericDelegate<TDoc, TProp>(PropertyInfo prop)
        {
            var dlgt = CreateSetterDelegate<TDoc, TProp>(prop);
            return new Action<object, object>((doc, val) => {
                dlgt((TDoc) doc, (TProp) val);
            });
        }

    }
}
