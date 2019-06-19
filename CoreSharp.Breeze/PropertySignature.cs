using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CoreSharp.Breeze {

  public class PropertySignature {
    public PropertySignature(Type instanceType, string propertyPath) {
      InstanceType = instanceType;
      PropertyPath = propertyPath;
      Properties = GetProperties(InstanceType, PropertyPath).ToList();
    }

    public static bool IsProperty(Type instanceType, string propertyPath) {
      return GetProperties(instanceType, propertyPath, false).Any(pi => pi != null);
    }

    public Type InstanceType { get; private set; }
    public string PropertyPath { get; private set; }
    public List<PropertyInfo> Properties { get; private set; }

    public string Name {
      get { return Properties.Select(p => p.Name).ToAggregateString("_"); }
    }

    public Type ReturnType {
      get { return Properties.Last().PropertyType; }
    }

    // returns null for scalar properties
    public Type ElementType {
      get { return TypeFns.GetElementType(ReturnType); }

    }

    public bool IsDataProperty {
      get { return TypeFns.IsPredefinedType(ReturnType) || TypeFns.IsEnumType(ReturnType); }
    }

    public bool IsNavigationProperty {
      get { return !IsDataProperty; }
    }



    // returns an IEnumerable<PropertyInfo> with nulls if invalid and throwOnError = true
    public static IEnumerable<PropertyInfo> GetProperties(Type instanceType, string propertyPath, bool throwOnError = true) {
      var propertyNames = propertyPath.Split('.');

      var nextInstanceType = instanceType;
      foreach (var propertyName in propertyNames) {
        var property = GetProperty(nextInstanceType, propertyName, throwOnError);
        if (property != null) {
          yield return property;

          nextInstanceType = property.PropertyType;
        } else {
          break;
        }
      }
    }

    private static PropertyInfo GetProperty(Type instanceType, string propertyName, bool throwOnError = true) {
      var propertyInfo = (PropertyInfo)TypeFns.FindPropertyOrField(instanceType, propertyName,
        BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public);
      if (propertyInfo == null) {
        if (throwOnError) {
          var msg = string.Format("Unable to locate property '{0}' on type '{1}'.", propertyName, instanceType);
          throw new Exception(msg);
        } else {
          return null;
        }
      }
      return propertyInfo;
    }

    public Expression BuildMemberExpression(ParameterExpression parmExpr) {
      var memberExpr = BuildPropertyExpression(parmExpr, Properties.First());
      foreach (var property in Properties.Skip(1)) {
        memberExpr = BuildPropertyExpression(memberExpr, property);
      }
      return memberExpr;
    }

    public Expression BuildPropertyExpression(Expression baseExpr, PropertyInfo property) {
      return Expression.Property(baseExpr, property);
    }



  }


}
