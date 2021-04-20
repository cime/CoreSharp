using System.Xml.Serialization;

namespace CoreSharp.NHibernate.SourceGenerator
{
    public class VirtualModifierAnalyzer
    {
        [XmlArray("ValidTypes")]
        [XmlArrayItem("ValidType")]
        public ValidTypes ValidTypes { get; set; }
    }
}
