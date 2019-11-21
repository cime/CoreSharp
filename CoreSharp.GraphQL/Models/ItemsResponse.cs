using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace CoreSharp.GraphQL.Models
{
    public class ItemsResponse<T>
        where T : class
    {
        private static readonly MethodInfo IncludeMethod = null;

        static ItemsResponse()
        {
            IncludeMethod = AppDomain.CurrentDomain.GetAssemblies()
                .Where(o => !o.IsDynamic)
                .Where(o => o.GetName().Name == "NHibernate.Extensions")
                .Select(o => o.GetTypes().First(t => t.FullName == "NHibernate.Linq.LinqExtensions"))
                .Select(o => o.GetMethods().First(m => m.Name == "Include" && !m.IsGenericMethod && m.GetParameters().Length == 2))
                .FirstOrDefault();
        }

        private readonly IQueryable<T> _queryable;
        private readonly Page _page;
        private readonly IList<Sort> _order;
        private long _count = -1;
        public IList<T> Items { get; }

        public ItemsResponse(ResolveFieldContext context, IQueryable<T> queryable, Page page, Sort sort)
            : this(context, queryable, page, new[] {sort})
        {

        }

        public ItemsResponse(ResolveFieldContext context, IQueryable<T> queryable, Page page, IList<Sort> order)
        {
            _queryable = queryable;
            _page = page;
            _order = order;

            var itemsField = context.FieldAst.SelectionSet.Selections.OfType<Field>().FirstOrDefault(x => x.Name == "items");
            if (itemsField != null)
            {
                var includes = itemsField.GetIncludes();

                if (IncludeMethod != null)
                {
                    foreach (var include in includes)
                    {
                        queryable = (IQueryable<T>)IncludeMethod.Invoke(null, new object[] { queryable, include });
                    }
                }
            }

            queryable = ApplyOrderBy(queryable, order);

            Items = queryable.Page(_page).ToList();
        }

        private IQueryable<T> ApplyOrderBy(IQueryable<T> queryable, IList<Sort> order)
        {
            if (order?.Any() == true)
            {
                var isOrdered = IsOrdered(queryable);
                foreach (var x in order)
                {
                    queryable = isOrdered ? ((IOrderedQueryable<T>) queryable).ThenBy($"{x.Field} {x.Direction}") : queryable.OrderBy($"{x.Field} {x.Direction}");

                    isOrdered = true;
                }
            }

            return queryable;
        }

        private static bool IsOrdered( IQueryable<T> source)
        {
            var ordered = source as IOrderedQueryable<T>;
            if (ordered != null)
            {
                var lastMethod = (source.Expression as MethodCallExpression)?.Method;

                if (lastMethod?.DeclaringType == typeof(Queryable))
                    switch (lastMethod.Name)
                    {
                        case nameof(Queryable.OrderBy):
                        case nameof(Queryable.OrderByDescending):
                        case nameof(Queryable.ThenBy):
                        case nameof(Queryable.ThenByDescending):
                            return true;
                    }
            }

            return false;
        }

        public bool HasNextPage
        {
            get
            {
                return _page != null && _page.Skip + _page.Take + 1 < Count;
            }
        }

        public bool HasPreviousPage
        {
            get
            {
                return _page != null && _page.Skip > 0;
            }
        }

        public long Count
        {
            get
            {
                return _count > -1 ? _count : _count = _queryable.Count();
            }
        }
    }
}
