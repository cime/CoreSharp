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
    public class DateTimeAddGenerator : BaseHqlGeneratorForMethod
    {
        public DateTimeAddGenerator()
        {
            SupportedMethods = new[] {
                ReflectHelper.GetMethodDefinition<DateTime>(d => d.AddSeconds((double)0)),
                ReflectHelper.GetMethodDefinition<DateTime>(d => d.AddMinutes((double)0)),
                ReflectHelper.GetMethodDefinition<DateTime>(d => d.AddHours((double)0)),
                ReflectHelper.GetMethodDefinition<DateTime>(d => d.AddDays((double)0)),
                ReflectHelper.GetMethodDefinition<DateTime>(d => d.AddMonths((int)0)),
                ReflectHelper.GetMethodDefinition<DateTime>(d => d.AddYears((int)0)),
            };
        }

        public override HqlTreeNode BuildHql(MethodInfo method, Expression targetObject, ReadOnlyCollection<Expression> arguments, HqlTreeBuilder treeBuilder, IHqlExpressionVisitor visitor)
        {
            return treeBuilder.MethodCall(method.Name, visitor.Visit(targetObject).AsExpression(), visitor.Visit(arguments[0]).AsExpression());
        }
    }
}
