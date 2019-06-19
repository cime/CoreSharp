using System;
using System.Linq;
using System.Reflection;

namespace CoreSharp.Breeze.Query {
  public static class EntityQueryExtensions {
    private static readonly MethodInfo IncludeMethod;

    static EntityQueryExtensions()
    {
      IncludeMethod = typeof(global::NHibernate.Linq.LinqExtensions)
          .GetMethods().First(m => m.Name == "Include" && !m.IsGenericMethod && m.GetParameters().Length == 2);
    }

    public static IQueryable ApplyWhere(this EntityQuery eq, IQueryable queryable, Type eleType) {
      if (eq.WherePredicate != null) {
        queryable = QueryBuilder.ApplyWhere(queryable, eleType, eq.WherePredicate);
      }
      return queryable;
    }

    public static IQueryable ApplyOrderBy(this EntityQuery eq, IQueryable queryable, Type eleType) {
      if (eq.OrderByClause != null) {
        queryable = QueryBuilder.ApplyOrderBy(queryable, eleType, eq.OrderByClause);
      }
      return queryable;
    }

    public static IQueryable ApplySelect(this EntityQuery eq, IQueryable queryable, Type eleType) {
      if (eq.SelectClause != null) {
        queryable = QueryBuilder.ApplySelect(queryable, eleType, eq.SelectClause);
      }
      return queryable;
    }

    public static IQueryable ApplySkip(this EntityQuery eq, IQueryable queryable, Type eleType) {
      if (eq.SkipCount.HasValue) {
        queryable = QueryBuilder.ApplySkip(queryable, eleType, eq.SkipCount.Value);
      }
      return queryable;
    }

    public static IQueryable ApplyTake(this EntityQuery eq, IQueryable queryable, Type eleType) {
      if (eq.TakeCount.HasValue) {
        queryable = QueryBuilder.ApplyTake(queryable, eleType, eq.TakeCount.Value);
      }
      return queryable;
    }

    public static IQueryable ApplyExpand(this EntityQuery eq, IQueryable queryable, Type eleType) {
      if (eq.ExpandClause != null && IncludeMethod != null)
      {
        var expands = eq.ExpandClause.PropertyPaths.Select(x => x.Replace("/", ".")).Where(x => !eq.ExpandClause.PropertyPaths.Any(e => e.StartsWith($"{x}."))).ToList();

        foreach (var expand in expands)
        {
          queryable = (IQueryable)IncludeMethod.Invoke(null, new object[] { queryable, expand });
        }
      }
      return queryable;
    }
  }
}
