using System;
using System.Collections.Generic;
using System.Linq;
using CoreSharp.Cqrs.Resolver;

namespace CoreSharp.Cqrs.Mvc
{
    public static class CqrsConfigurationExtensions
    {

        public static string GetUrlPath(this CqrsControllerConfiguration options, CqrsInfo info)
        {
            var root = (info.IsQuery ? options.QueriesPath : options.CommandsPath).Replace("//", "/").TrimEnd('/').TrimStart('/');
            var uri = $"/{root}/{info.MethodName}";
            return uri;
        }

        public static IEnumerable<CqrsInfo> FilterCqrsInfoList(this CqrsConfiguration options, IEnumerable<CqrsInfo> cqrsInfos)
        {
            var infos = cqrsInfos.Where(x => FilterTypeName(x.ReqType, options.ExposeList)).ToList();
            return infos;
        }

        private static bool FilterTypeName(Type type, string[] patterns)
        {
            if (patterns == null || !patterns.Any())
            {
                return true;
            }

            // process patterns 
            var patternsInfo = patterns.Select(x =>
            {
                var negative = x.StartsWith("!");
                var range = x.EndsWith(".*");
                var pattern = x.TrimStart('!').TrimEnd(".*".ToCharArray());
                return Tuple.Create(pattern, negative, range);
            }).ToList();

            // if only negative patterns are present add default positive pattern
            if(patternsInfo.Count(x => x.Item2) == patterns.Count())
            {
                patternsInfo.Add(Tuple.Create("", false, false));
            }

            // order patterns by length
            patternsInfo = patternsInfo.OrderByDescending(x => x.Item1.Length).ToList();

            // get best math (explicit before range)
            var patternMatch = patternsInfo.FirstOrDefault(x => !x.Item3 && type.FullName.Equals(x.Item1)) 
                ?? patternsInfo.FirstOrDefault(x => x.Item1 == "" || type.FullName.StartsWith(x.Item1));

            var status = patternMatch != null && !patternMatch.Item2;
            return status;

        }

    }
}
