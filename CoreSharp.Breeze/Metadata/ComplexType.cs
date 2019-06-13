using System;
using System.Collections.Generic;
using CoreSharp.Breeze.Extensions;

namespace CoreSharp.Breeze.Metadata
{
    public class ComplexType : StructuralType
    {
        public ComplexType() { }

        public ComplexType(Type type) : base(type)
        {
            IsComplexType = true;
        }

        public ComplexType(Dictionary<string, object> dict) : base(dict)
        {
            IsComplexType = true;
        }

        #region IsComplexType

        /// <summary>
        /// This must be 'true'.  This field is what distinguishes an entityType from a complexType.
        /// </summary>
        public bool IsComplexType
        {
            get { return OriginalDictionary.GetValue<bool>("isComplexType"); }
            set { OriginalDictionary["isComplexType"] = value; }
        }

        #endregion
    }
}
