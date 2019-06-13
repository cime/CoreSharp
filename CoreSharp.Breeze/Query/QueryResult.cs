using System.Collections;

namespace CoreSharp.Breeze.Query {
  public class QueryResult {

    public QueryResult(IEnumerable results, int? inlineCount = null) {
      Results = results;
      InlineCount = inlineCount;
    }

    public IEnumerable Results {
      get; private set;
    }

    public int? InlineCount {
      get; private set;
    }
  }

}
