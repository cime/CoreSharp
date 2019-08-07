using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using CoreSharp.DataAccess;

namespace CoreSharp.NHibernate.CodeList.Extensions
{
    internal static class EntityExtensions
    {
        private static void AddIfNotExist<T>(ICollection<T> collection, T item)
        {
            if (!collection.Contains(item))
                collection.Add(item);
        }

        private static void RemoveIfExist<T>(ICollection<T> collection, T item)
        {
            if (collection.Contains(item))
                collection.Remove(item);
        }

        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var type = typeof(TSource);

            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda));

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            return propInfo;
        }

        public static void AddOneToMany<TOne, TMany>(this TOne one,
            Expression<Func<TOne, IEnumerable<TMany>>> manyListExpr, TMany many,
            Expression<Func<TMany, TOne>> oneExpr,
            Expression<Func<TOne, Action<TMany>>> removeManyExpr)
            where TOne : IEntity
            where TMany : IEntity
        {
            var enumerable = manyListExpr.Compile()(one);
            var collection = (ICollection<TMany>)enumerable;
            var itemOwner = oneExpr.Compile()(many);
            var entityOwnerPropInfo = GetPropertyInfo(many, oneExpr);

            if (collection.Contains(many)) return;
            if (itemOwner != null)
                removeManyExpr.Compile()(itemOwner)(many);
            entityOwnerPropInfo.SetValue(many, one);
            collection.Add(many);

            /*
            if (Locations.Contains(location)) return;
            if (location.User != null)
                location.User.RemoveLocation(location);
            location.User = this;
            Locations.Add(location);*/


        }

        public static void RemoveOneToMany<TOne, TMany>(this TOne one,
            Expression<Func<TOne, IEnumerable<TMany>>> manyListExpr, TMany many,
            Expression<Func<TMany, TOne>> entityExpr
            )
            where TOne : IEntity
            where TMany : IEntity
        {
            var enumerable = manyListExpr.Compile()(one);
            var collection = (ICollection<TMany>)enumerable;
            var entityOwnerPropInfo = GetPropertyInfo(many, entityExpr);

            if (!collection.Contains(many)) return;
            collection.Remove(many);
            entityOwnerPropInfo.SetValue(many, null);

            /*
            if (!Locations.Contains(location)) return;
            Locations.Remove(location);
            location.User = null;*/
        }

        public static void SetManyToOne<TMany, TOne>(this TMany many,
            Expression<Func<TMany, TOne>> oneExpr, TOne newOne,
            Expression<Func<TOne, Action<TMany>>> removeManyExpr,
            Expression<Func<TOne, IEnumerable<TMany>>> manyExp)
            where TOne : IEntity
            where TMany : IEntity
        {
            var one = oneExpr.Compile()(many);
            var onePropInfo = GetPropertyInfo(many, oneExpr);
            var manyList = (ICollection<TMany>)manyExp.Compile()(newOne);

            if (Equals(one, newOne)) return;
            if (one != null)
                removeManyExpr.Compile()(one)(many);
            onePropInfo.SetValue(many, newOne);
            AddIfNotExist(manyList, many);
            /*
            if (User == user) return;
            if(User != null)
                User.RemoveLocation(this);
            User = user;
            AddIfNotExist(user.Locations, this);*/
        }

        public static void UnsetManyToOne<TMany, TOne>(this TMany many,
            Expression<Func<TMany, TOne>> oneExpr,
            Expression<Func<TOne, IEnumerable<TMany>>> manyListExpr)
            where TOne : IEntity
            where TMany : IEntity
        {
            var one = oneExpr.Compile()(many);
            var onePropInfo = GetPropertyInfo(many, oneExpr);
            var manyList = (ICollection<TMany>)manyListExpr.Compile()(one);

            if (one == null) return;
            RemoveIfExist(manyList, many);
            onePropInfo.SetValue(many, null);
            /*
            if(User == null) return;
            RemoveIfExist(User.Locations, this);
            User = null;*/
        }

        public static void AddManyToMany<TMany, TMany2>(this TMany many,
            Expression<Func<TMany, IEnumerable<TMany2>>> many2ListExpr, TMany2 many2,
            Expression<Func<TMany2, IEnumerable<TMany>>> manyListExpr)
            where TMany : IEntity
            where TMany2 : IEntity
        {
            var many2List = (ICollection<TMany2>)many2ListExpr.Compile()(many);
            var manyList = (ICollection<TMany>)manyListExpr.Compile()(many2);

            AddIfNotExist(manyList, many);
            AddIfNotExist(many2List, many2);
            /*
            camera.Locations.AddIfNotExist(this);
            Cameras.AddIfNotExist(camera);*/
        }

        public static void RemoveManyToMany<TMany, TMany2>(this TMany many,
            Expression<Func<TMany, IEnumerable<TMany2>>> many2ListExpr, TMany2 many2,
            Expression<Func<TMany2, IEnumerable<TMany>>> manyListExpr)
            where TMany : IEntity
            where TMany2 : IEntity
        {
            var many2List = (ICollection<TMany2>)many2ListExpr.Compile()(many);
            var manyList = (ICollection<TMany>)manyListExpr.Compile()(many2);

            RemoveIfExist(manyList, many);
            RemoveIfExist(many2List, many2);
            /*
            RemoveIfExist(camera.Locations, this);
            RemoveIfExist(Cameras, camera);*/
        }

        public static void SetOneToOne<TOne, TOne2>(this TOne one,
            Expression<Func<TOne, TOne2>> one2Expr, TOne2 one2,
            Expression<Func<TOne2, TOne>> oneExpr)
            where TOne : IEntity
            where TOne2 : IEntity
        {
            var oneInOne2 = oneExpr.Compile()(one2);
            var oneInOne2PropInfo = GetPropertyInfo(one2, oneExpr);
            var currentOne2 = one2Expr.Compile()(one);
            var currentOne2PropInfo = GetPropertyInfo(one, one2Expr);

            if (oneInOne2 != null)
                oneInOne2.UnsetOneToOne(one2Expr, oneExpr);
            oneInOne2PropInfo.SetValue(one2, one);
            if (currentOne2 != null)
                currentOne2.UnsetOneToOne(oneExpr, one2Expr);
            currentOne2PropInfo.SetValue(one, one2);

            /*
             merryWith.MerriedWith = this;
             MerriedWith = merryWith;
             */


        }

        public static void UnsetOneToOne<TOne, TOne2>(this TOne one,
            Expression<Func<TOne, TOne2>> one2Expr,
            Expression<Func<TOne2, TOne>> oneExpr)
            where TOne : IEntity
            where TOne2 : IEntity
        {
            var one2 = one2Expr.Compile()(one);
            var one2PropInfo = GetPropertyInfo(one, one2Expr);
            var oneInOne2PropInfo = GetPropertyInfo(one2, oneExpr);

            if (one2 == null) return;
            oneInOne2PropInfo.SetValue(one2, null);
            one2PropInfo.SetValue(one, null);
            /*
            if (Dispozicija == null) return;
            Dispozicija.DispozicijaRow = null;
            Dispozicija = null;*/
        }
    }
}
