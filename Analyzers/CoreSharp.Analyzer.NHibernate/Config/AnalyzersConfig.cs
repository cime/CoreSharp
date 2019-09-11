using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace CoreSharp.Analyzer.NHibernate.Config
{
    [XmlRoot("Analyzers")]
    public class AnalyzersConfig
    {
        private static AnalyzersConfig _instance;

        public VirtualModifierAnalyzer VirtualModifierAnalyzer { get; set; } = new VirtualModifierAnalyzer();
        public PropertyOrderAnalyzer PropertyOrderAnalyzer { get; set; } = new PropertyOrderAnalyzer();

        [XmlArray("ValidTypes")]
        [XmlArrayItem("ValidType")]
        public ValidTypes ValidTypes { get; set; } = new ValidTypes() { "CoreSharp.DataAccess.IEntity", "CoreSharp.DataAccess.ICodeList" };

        public List<string> VirtualModifierAnalyzerValidTypes => new List<string>().Concat(ValidTypes ?? new List<string>()).Concat(VirtualModifierAnalyzer?.ValidTypes ?? new List<string>()).Distinct().ToList();
        public List<string> PropertyOrderAnalyzerValidTypes => new List<string>().Concat(ValidTypes ?? new List<string>()).Concat(PropertyOrderAnalyzer?.ValidTypes ?? new List<string>()).Distinct().ToList();

        [DebuggerStepThrough]
        public static AnalyzersConfig Deserialize(string content)
        {
            try
            {
                var xs = new XmlSerializer(typeof(AnalyzersConfig));
                var sr = new StringReader(content);

                return (AnalyzersConfig)xs.Deserialize(sr);
            }
            catch (Exception)
            {
                return _instance ?? (_instance = new AnalyzersConfig());
            }
        }
    }

    public class VirtualModifierAnalyzer
    {
        [XmlArray("ValidTypes")]
        [XmlArrayItem("ValidType")]
        public ValidTypes ValidTypes { get; set; }
    }

    public class PropertyOrderAnalyzer
    {
        [XmlArray("ValidTypes")]
        [XmlArrayItem("ValidType")]
        public ValidTypes ValidTypes { get; set; }
    }

    public class ValidTypes : List<string>
    {
    }
}
