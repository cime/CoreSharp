using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreSharp.Breeze.Query {

  // local to this package
  class FnBlockToken {
    private StringBuilder _sb;
    private int _nextIx;
    private List<FnBlockToken> _fnArgs;

    private FnBlockToken() {
      _sb = new StringBuilder();
    }

    public static FnBlock ToExpression(string source, Type entityType) {
      var token = ParseToken(source, 0);
      return (FnBlock)token.ToExpression(entityType, null);
    }

    private BaseBlock ToExpression(Type entityType, DataType returnDataType) {
      var text = _sb.ToString();
      if (this._fnArgs == null) {

        if (PropertySignature.IsProperty(entityType, text)) {
          // TODO: we could check that the PropBlock dataType is compatible with the returnDataType
          return new PropBlock(text, entityType);
        } else {
          return new LitBlock(text, returnDataType);
        }

      } else {
        var fnName = text;
        var argTypes = FnBlock.GetArgTypes(fnName);
        if (argTypes.Count != _fnArgs.Count) {
          throw new Exception("Incorrect number of arguments to '" + fnName
                  + "' function; was expecting " + argTypes.Count);
        }
        var exprs = _fnArgs.Select((token, ix) => token.ToExpression(entityType, argTypes[ix])).ToList();
        // TODO: we could check that the FnBlock dataType is compatible with the returnDataType
        return new FnBlock(text, exprs);
      }

    }

    private static FnBlockToken ParseToken(string source, int ix) {
      ix = SkipWhitespace(source, ix);
      var token = CollectQuotedToken(source, ix);
      if (token != null) {
        return token;
      }
      token = new FnBlockToken();
      var badChars = "'\"";

      while (ix < source.Length) {
        var c = source[ix];

        if (c == '(') {
          ix++;
          ParseFnArgs(token, source, ix);
          return token;
        }

        if (c == ',' || c == ')') {
          token._nextIx = ix;
          return token;
        }

        if (badChars.IndexOf(c) >= 0) {
          throw new Exception("Unable to parse Fn name - encountered: " + c);
        }
        token._sb.Append(c);
        ix++;
      }
      token._nextIx = ix;
      return token;

    }

    private static void ParseFnArgs(FnBlockToken token, string source, int ix) {
      token._fnArgs = new List<FnBlockToken>();

      while (ix < source.Length) {
        var argToken = ParseToken(source, ix);
        ix = argToken._nextIx;
        if (argToken._sb.Length != 0) {
          token._fnArgs.Add(argToken);
        }
        var c = source[ix];
        ix++;
        if (c == ')') break;
      }
      token._nextIx = ix;
      return;

    }

    private static int SkipWhitespace(string source, int ix) {
      while (ix < source.Length) {
        var c = source[ix];
        if (c == ' ') {
          ix++;
        } else {
          return ix;
        }
      }
      return ix;
    }

    private static FnBlockToken CollectQuotedToken(string source, int ix) {
      var c = source[ix];
      if (c != '\'' && c != '"') return null;
      var token = new FnBlockToken();
      var quoteChar = c;
      ix++;
      while (ix < source.Length) {
        c = source[ix];
        ix++;
        if (c == quoteChar) {
          token._nextIx = ix;
          return token;
        } else {
          token._sb.Append(c);
        }
      }
      throw new Exception("Quoted token was not terminated");
    }
  }

}