using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace CoreSharp.NHibernate.Generator
{
    [XmlRoot(ElementName = "AttributeIdentifierLength")]
    public class AttributeIdentifierLength
    {
        [XmlElement(ElementName = "Type")]
        public string Type { get; set; }
        [XmlElement(ElementName = "PropertyName")]
        public string PropertyName { get; set; }
    }

    [XmlRoot(ElementName = "AttributeIdentifierLengths")]
    public class AttributeIdentifierLengths
    {
        [XmlElement(ElementName = "AttributeIdentifierLength")]
        public AttributeIdentifierLength AttributeIdentifierLength { get; set; }
    }

    [XmlRoot(ElementName = "DefaultProperty")]
    public class DefaultProperty
    {
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "Type")]
        public string Type { get; set; }
        [XmlElement(ElementName = "Nullable")]
        public string Nullable { get; set; }
        [XmlElement(ElementName = "Constructor")]
        public string Constructor { get; set; }
    }

    [XmlRoot(ElementName = "KeyColumn")]
    public class KeyColumn
    {
        [XmlElement(ElementName = "Prefix")]
        public string Prefix { get; set; }
        [XmlElement(ElementName = "Postfix")]
        public string Postfix { get; set; }
    }

    [XmlRoot(ElementName = "EntityType")]
    public class EntityType
    {
        [XmlElement(ElementName = "DerivedFromType")]
        public string DerivedFromType { get; set; }
        [XmlElement(ElementName = "IdentifierPropertyName")]
        public string IdentifierPropertyName { get; set; }
        [XmlElement(ElementName = "IdentifierPropertyType")]
        public string IdentifierPropertyType { get; set; }
        [XmlElement(ElementName = "IdentifierLength")]
        public string IdentifierLength { get; set; }
        [XmlElement(ElementName = "AttributeIdentifierLengths")]
        public AttributeIdentifierLengths AttributeIdentifierLengths { get; set; }
        [XmlElement(ElementName = "DefaultProperty")]
        public DefaultProperty DefaultProperty { get; set; }
        [XmlElement(ElementName = "KeyColumn")]
        public KeyColumn KeyColumn { get; set; }
    }

    [XmlRoot(ElementName = "EntityTypes")]
    public class EntityTypes
    {
        [XmlElement(ElementName = "EntityType")]
        public List<EntityType> EntityType { get; set; }
    }

    [XmlRoot(ElementName = "CustomAttributes")]
    public class CustomAttributes
    {
        [XmlElement(ElementName = "CustomAttribute")]
        public string CustomAttribute { get; set; }
    }

    [XmlRoot(ElementName = "SyntheticProperties")]
    public class SyntheticProperties
    {
        [XmlElement(ElementName = "Generate")]
        public string Generate { get; set; }
        [XmlElement(ElementName = "EmtyGetSet")]
        public string EmtyGetSet { get; set; }
        [XmlElement(ElementName = "Comment")]
        public string Comment { get; set; }
        [XmlElement(ElementName = "AddNotNullAttribute")]
        public string AddNotNullAttribute { get; set; }
        [XmlElement(ElementName = "AddLengthAttribute")]
        public string AddLengthAttribute { get; set; }
        [XmlElement(ElementName = "CustomAttributes")]
        public CustomAttributes CustomAttributes { get; set; }
        [XmlElement(ElementName = "GenerateForDeriveredTypes")]
        public string GenerateForDeriveredTypes { get; set; }
    }

    [XmlRoot(ElementName = "GenerateMetadata")]
    public class GenerateMetadata
    {
        [XmlElement(ElementName = "Relations")]
        public string Relations { get; set; }
    }

    [XmlRoot(ElementName = "LanguageModel")]
    public class LanguageModel
    {
        [XmlElement(ElementName = "Type")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "LocalizeAttribute")]
    public class LocalizeAttribute
    {
        [XmlElement(ElementName = "Type")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "Localization")]
    public class Localization
    {
        [XmlElement(ElementName = "Type")]
        public string Type { get; set; }
        [XmlElement(ElementName = "LanguageModel")]
        public LanguageModel LanguageModel { get; set; }
        [XmlElement(ElementName = "LocalizeAttribute")]
        public LocalizeAttribute LocalizeAttribute { get; set; }
    }

    [XmlRoot(ElementName = "Settings")]
    public class Settings
    {
        [XmlElement(ElementName = "BaseEntityType")]
        public string BaseEntityType { get; set; }
        [XmlElement(ElementName = "EntityTypes")]
        public EntityTypes EntityTypes { get; set; }
        [XmlElement(ElementName = "Strict")]
        public string Strict { get; set; }
        [XmlElement(ElementName = "SyntheticProperties")]
        public SyntheticProperties SyntheticProperties { get; set; }
        [XmlElement(ElementName = "GenerateMetadata")]
        public GenerateMetadata GenerateMetadata { get; set; }
        [XmlElement(ElementName = "Localization")]
        public Localization Localization { get; set; }
        [XmlElement(ElementName = "NotNullAttribute")]
        public string NotNullAttribute { get; set; }
        [XmlElement(ElementName = "LengthAttribute")]
        public string LengthAttribute { get; set; }
        [XmlElement(ElementName = "DefaultCollectionMapType")]
        public string DefaultCollectionMapType { get; set; }

        public static Settings Deserialize(string filePath)
        {
            var serializer = new XmlSerializer(typeof(Settings));

            using (var reader = new StreamReader(filePath))
            {
                return (Settings)serializer.Deserialize(reader);
            }
        }
    }
}
