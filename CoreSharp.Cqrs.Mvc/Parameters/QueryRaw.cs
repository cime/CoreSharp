using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSharp.Cqrs.Mvc.Parameters
{
    internal class QueryRaw : IDataRaw<Query>
    {
        public int? Skip { get; set; }

        public int? Take { get; set; }

        public bool? InlineCount { get; set; }

        public IEnumerable<string> OrderBy { get; set; }

        public Query MapToData()
        {

            var order = OrderBy?.Select(x => Tuple.Create(x?.Split(' ').FirstOrDefault(), x?.Split(' ').ElementAtOrDefault(1)?.ToLowerInvariant() == "desc"))
                .Where(x => !string.IsNullOrWhiteSpace(x.Item1)).ToList();

            var query = new Query
            {
                InlineCount = InlineCount,
                Skip = Skip,
                Take = Take,
                SortBy = order?.Select(x => x.Item1).ToList(),
                SortOrder = order?.Select(x => x.Item2).ToList()
            };
            return query;
        }
    }
}
