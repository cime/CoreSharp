using System.Linq;

namespace CoreSharp.Analyzer.NHibernate.Extensions
{
    public static class StringExtensions
    {
        private static readonly string[] GeneratedFilesExtensions = new []{ ".g.cs", ".generated.cs", "*designer.cs", ".AssemblyInfo.cs" };
        
        public static bool IsGeneratedFile(this string filePath)
        {
            return GeneratedFilesExtensions.Any(x => filePath.EndsWith(x));
        }
    }
}
