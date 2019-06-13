using System;
using System.Collections.Generic;
using CoreSharp.Breeze.Extensions;

namespace CoreSharp.Breeze.Metadata
{
    public abstract class StructuralType : MetadataDictionary
    {
        protected StructuralType() { }

        protected StructuralType(Type type)
        {
            Namespace = type.Namespace;
            ShortName = type.Name;
        }

        protected StructuralType(Dictionary<string, object> dict) : base(dict) { }


        #region ShortName

        /// <summary>
        /// Together the shortName and the namespace make up a fully qualified name.  Within this metadata references to an entityType are all qualified references. See the 'structuralTypeName' definition.instanceof in this document.
        /// </summary>
        public string ShortName
        {
            get { return OriginalDictionary.GetValue<string>("shortName"); }
            set { OriginalDictionary["shortName"] = value; }
        }

        #endregion

        #region Namespace

        public string Namespace
        {
            get { return OriginalDictionary.GetValue<string>("namespace"); }
            set { OriginalDictionary["namespace"] = value; }
        }

        #endregion

        #region DataProperties

        private DataProperties _dataProperties;

        public DataProperties DataProperties
        {
            get
            {
                if (_dataProperties != null)
                    return _dataProperties;
                if (!OriginalDictionary.ContainsKey("dataProperties"))
                    OriginalDictionary["dataProperties"] = new List<Dictionary<string, object>>();
                _dataProperties = new DataProperties(OriginalDictionary["dataProperties"] as List<Dictionary<string, object>>);
                return _dataProperties;
            }
            set
            {
                _dataProperties = value;
                OriginalDictionary["dataProperties"] = value.OriginalList;
            }
        }

        #endregion

        #region Validators

        private Validators _validators;

        /// <summary>
        /// A list of the validators (validations) that will be associated with this structure
        /// </summary>
        public Validators Validators
        {
            get
            {
                if (_validators != null)
                    return _validators;
                if (!OriginalDictionary.ContainsKey("validators"))
                    OriginalDictionary["validators"] = new List<Dictionary<string, object>>();
                _validators = new Validators(OriginalDictionary["validators"] as List<Dictionary<string, object>>);
                return _validators;
            }
            set
            {
                _validators = value;
                OriginalDictionary["validators"] = value.OriginalList;
            }
        }

        #endregion

        #region Custom

        public object Custom
        {
            get { return OriginalDictionary.GetValue<object>("custom"); }
            set { OriginalDictionary["custom"] = value; }
        }

        #endregion

        public string TypeFullName
        {
            get { return string.Format("{0}.{1}", Namespace, ShortName); }
        }

        public override string ToString()
        {
            return string.Format("{0}:#{1}", ShortName, Namespace);
        }

        public static StructuralType CreateInstance(Dictionary<string, object> dict)
        {
            return dict.ContainsKey("isComplexType")
                ? (StructuralType) new ComplexType(dict)
                : new EntityType(dict);
        }
    }
}
