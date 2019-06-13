using System.Collections.Generic;
using CoreSharp.Breeze.Extensions;

namespace CoreSharp.Breeze.Metadata
{
    public class Validator : MetadataDictionary
    {
        public Validator()
        {
        }

        public Validator(Dictionary<string, object> dict) : base(dict)
        {
        }

        #region Name

        /// <summary>
        /// On deserialization, this must match the name of some validator already registered on the breeze client.
        /// </summary>
        public string Name
        {
            get { return OriginalDictionary.GetValue<string>("name"); }
            set { OriginalDictionary["name"] = value; }
        }

        #endregion
    }
}
