using System.Collections.Generic;
using CoreSharp.Breeze.Extensions;

namespace CoreSharp.Breeze.Metadata
{
    public abstract class BaseProperty : MetadataDictionary
    {
        protected BaseProperty()
        {
        }

        protected BaseProperty(Dictionary<string, object> dict) : base(dict)
        {
        }

        #region DisplayName

        public string DisplayName
        {
            get { return OriginalDictionary.GetValue<string>("displayName"); }
            set { OriginalDictionary["displayName"] = value; }
        }

        #endregion

        #region Name

        /// <summary>
        /// The client side name of this property.
        /// </summary>
        public string Name
        {
            get { return OriginalDictionary.GetValue<string>("name"); }
            set { OriginalDictionary["name"] = value; }
        }

        #endregion

        #region NameOnServer

        /// <summary>
        /// The server side side name of this property. Either name or nameOnServer must be specified and either is sufficient.
        /// </summary>
        public string NameOnServer
        {
            get { return OriginalDictionary.GetValue<string>("nameOnServer"); }
            set { OriginalDictionary["nameOnServer"] = value; }
        }

        #endregion

        #region Custom

        public object Custom
        {
            get { return OriginalDictionary.GetValue<object>("custom"); }
            set { OriginalDictionary["custom"] = value; }
        }

        #endregion
    }
}
