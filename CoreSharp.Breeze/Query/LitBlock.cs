using System;
using System.Linq.Expressions;

namespace CoreSharp.Breeze.Query {
  public class LitBlock : BaseBlock {

    private object _initialValue;
    private object _coercedValue;
    private DataType _dataType;

    // TODO: doesn't yet handle case where value is an array - i.e. rhs of in clause.
    public LitBlock(object value, DataType dataType) {
      _initialValue = value;
      _dataType = dataType;
      _coercedValue = DataType.CoerceData(value, dataType);
    }

    public object GetValue() {
      return _coercedValue;
    }

    public override DataType DataType {
      get { return _dataType; }
    }

    public override Expression ToExpression(Expression inExpr) {
      return Expression.Constant(_coercedValue);
    }

  }
}