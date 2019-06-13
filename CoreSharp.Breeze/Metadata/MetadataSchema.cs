using System.Collections.Generic;
using CoreSharp.Breeze.Extensions;

namespace CoreSharp.Breeze.Metadata
{
    /// <summary>
    /// Metadata describing the entity model.  Converted to JSON to send to Breeze client.
    /// </summary>
    public class MetadataSchema : MetadataDictionary
    {
        public MetadataSchema()
        {
        }

        public MetadataSchema(IDictionary<string, object> metadata)
            : base(metadata as Dictionary<string, object>)
        {
        }

        #region MetadataVersion

        /// <summary>
        /// The serialization version for this document
        /// </summary>
        public string MetadataVersion
        {
            get { return OriginalDictionary.GetValue<string>("metadataVersion"); }
            set { OriginalDictionary["metadataVersion"] = value; }
        }

        #endregion

        #region NamingConvention

        /// <summary>
        /// On deserialization, this must match the name of some 'namingConvention' already registered on the breeze client.
        /// </summary>
        public string NamingConvention
        {
            get { return OriginalDictionary.GetValue<string>("namingConvention"); }
            set { OriginalDictionary["namingConvention"] = value; }
        }

        #endregion

        #region LocalQueryComparisonOptions

        /// <summary>
        /// On deserialization, this must match the name of some 'localQueryComparisonOptions' already registered on the breeze client.
        /// </summary>
        public string LocalQueryComparisonOptions
        {
            get { return OriginalDictionary.GetValue<string>("localQueryComparisonOptions"); }
            set { OriginalDictionary["localQueryComparisonOptions"] = value; }
        }

        #endregion

        #region StructuralTypes

        private StructuralTypes _structuralTypes;

        /// <summary>
        /// Array of entity type/complex type names to their metadata definitions.  The key is a structural type name and the value is either an entityType or a complexType
        /// </summary>
        public StructuralTypes StructuralTypes
        {
            get
            {
                if (_structuralTypes != null)
                    return _structuralTypes;
                if (!OriginalDictionary.ContainsKey("structuralTypes"))
                    OriginalDictionary["structuralTypes"] = new List<Dictionary<string, object>>();
                _structuralTypes = new StructuralTypes(OriginalDictionary["structuralTypes"] as List<Dictionary<string, object>>);
                return _structuralTypes;
            }
            set
            {
                _structuralTypes = value;
                OriginalDictionary["structuralTypes"] = value.OriginalList;
            }
        }

        #endregion

        #region DataServices

        private DataServices _dataServices;

        public DataServices DataServices
        {
            get
            {
                if (_dataServices != null)
                    return _dataServices;
                if (!OriginalDictionary.ContainsKey("dataServices"))
                    OriginalDictionary["dataServices"] = new List<Dictionary<string, object>>();
                _dataServices = new DataServices(OriginalDictionary["dataServices"] as List<Dictionary<string, object>>);
                return _dataServices;
            }
            set
            {
                _dataServices = value;
                OriginalDictionary["dataServices"] = value.OriginalList;
            }
        }

        #endregion

        #region ResourceEntityTypeMap

        private ResourceEntityTypeMap _resourceEntityTypeMap;

        /// <summary>
        /// Map of resource names to entity type names.
        /// </summary>
        public ResourceEntityTypeMap ResourceEntityTypeMap
        {
            get
            {
                if (_resourceEntityTypeMap != null)
                    return _resourceEntityTypeMap;
                if (!OriginalDictionary.ContainsKey("resourceEntityTypeMap"))
                    OriginalDictionary["resourceEntityTypeMap"] = new Dictionary<string, string>();
                _resourceEntityTypeMap = new ResourceEntityTypeMap(OriginalDictionary["resourceEntityTypeMap"] as Dictionary<string, string>);
                return _resourceEntityTypeMap;
            }
            set
            {
                _resourceEntityTypeMap = value;
                OriginalDictionary["resourceEntityTypeMap"] = value.OriginalDictionary;
            }
        }

        #endregion

        #region ForeignKeyMap

        /// <summary>
        /// Map of relationship name -> foreign key name, e.g. "Customer" -> "CustomerID".
        /// Used for re-establishing the entity relationships from the foreign key values during save.
        /// This part is not sent to the client because it is separate from the base dictionary implementation.
        /// </summary>
        public IDictionary<string, string> ForeignKeyMap;

        #endregion
    }
}
