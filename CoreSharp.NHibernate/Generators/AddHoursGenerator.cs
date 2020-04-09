using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Hql.Ast;
using NHibernate.Linq.Functions;
using NHibernate.Linq.Visitors;
using NHibernate.Util;

namespace CoreSharp.NHibernate.Generators
{
    public class AddHoursGenerator : BaseHqlGeneratorForMethod
    {
        public AddHoursGenerator()
        {
            SupportedMethods = new[] {
                ReflectHelper.GetMethodDefinition<DateTime>(d => d.AddHours((double)0))
            };
        }

        public override HqlTreeNode BuildHql(MethodInfo method, Expression targetObject, ReadOnlyCollection<Expression> arguments, HqlTreeBuilder treeBuilder, IHqlExpressionVisitor visitor)
        {
            return treeBuilder.MethodCall("AddHours", visitor.Visit(targetObject).AsExpression(), visitor.Visit(arguments[0]).AsExpression());
        }
    }
}
