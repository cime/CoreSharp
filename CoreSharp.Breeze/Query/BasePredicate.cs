using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CoreSharp.Breeze.Query {
  /**
   * Represents a single where clause.
   * @author IdeaBlade
   *
   */
  public abstract class BasePredicate {

    protected Operator _op;

    public Operator Operator {
      get { return _op; }
    }

    public BasePredicate(Operator op) {
      _op = op;
    }

    public abstract void Validate(Type entityType);

    public LambdaExpression ToLambda(Type entityType) {
      var paramExpr = Expression.Parameter(entityType, "ent");
      var expr = ToExpression(paramExpr);
      return Expression.Lambda(expr, paramExpr);
    }

    public static List<BasePredicate> PredicatesFromMap(IDictionary<string, object> map) {
      return map.Keys.Select(k => PredicateFromKeyValue(k, map[k])).ToList();
    }

    public static BasePredicate PredicateFromMap(IDictionary<string, object> sourceMap) {
      if (sourceMap == null) return null;
      var preds = PredicatesFromMap(sourceMap);
      return CreateCompoundPredicate(preds);
    }

    private static BasePredicate PredicateFromKeyValue(string key, object value) {
      var op = Operator.FromString(key);
      if (op != null) {
        if (op.OpType == OperatorType.AndOr) {
          var preds2 = PredicatesFromObject(value);
          return new AndOrPredicate(op, preds2);
        } else if (op.OpType == OperatorType.Unary) {
          var pred = PredicateFromObject(value);
          return new UnaryPredicate(op, pred);
        } else {
          throw new Exception("Invalid operator in context: " + key);
        }
      }

      if (value == null || TypeFns.IsPredefinedType(value.GetType())) {
        return new BinaryPredicate(BinaryOperator.Equals, key, value);
      } else if (value is IDictionary<string, object> && ((IDictionary<string, object>)value).ContainsKey("value")) {
        return new BinaryPredicate(BinaryOperator.Equals, key, value);
      }

      if (!(value is Dictionary<string, object>)) {
        throw new Exception("Unable to resolve value associated with key:" + key);
      }

      var preds = new List<BasePredicate>();
      var map = (Dictionary<string, object>)value;


      foreach (var subKey in map.Keys) {

        var subOp = Operator.FromString(subKey);
        var subVal = map[subKey];
        BasePredicate pred;
        if (subOp != null) {
          if (subOp.OpType == OperatorType.AnyAll) {
            var subPred = PredicateFromObject(subVal);
            pred = new AnyAllPredicate(subOp, key, subPred);
          } else if (subOp.OpType == OperatorType.Binary) {
            pred = new BinaryPredicate(subOp, key, subVal);
          } else {
            throw new Exception("Unable to resolve OperatorType for key: " + subKey);
          }
          // next line old check was for null not 'ContainsKey'
        } else if (subVal is IDictionary<string, object> && ((IDictionary<string, object>)subVal).ContainsKey("value")) {
          pred = new BinaryPredicate(BinaryOperator.Equals, key, subVal);
        } else {
          throw new Exception("Unable to resolve BasePredicate after: " + key);
        }
        preds.Add(pred);
      }
      return CreateCompoundPredicate(preds);
    }


    private static BasePredicate PredicateFromObject(object source) {
      var preds = PredicatesFromObject(source);
      return CreateCompoundPredicate(preds);
      //		if (preds.size() > 1) {
      //			throw new RuntimeException("BasePredicateFromObject: should only contain a single item");
      //		} else {
      //		    return preds.get(0);
      //		}
    }

    private static List<BasePredicate> PredicatesFromObject(object source) {
      var preds = new List<BasePredicate>();
      if (source is IDictionary<string, object>) {
        preds = PredicatesFromMap((IDictionary<string, object>)source);
      } else if (source is IList) {
        foreach (var item in (IList)source) {
          var pred = PredicateFromObject(item);
          preds.Add(pred);
        }
      }
      return preds;
    }

    private static BasePredicate CreateCompoundPredicate(List<BasePredicate> preds) {
      if (preds.Count > 1) {
        return new AndOrPredicate(Operator.And, preds);
      } else {
        return preds[0];
      }
    }

    public abstract Expression ToExpression(ParameterExpression paramExpr);

  }
}
