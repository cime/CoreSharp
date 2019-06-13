using System.Collections.Generic;
using CoreSharp.Breeze.Extensions;

namespace CoreSharp.Breeze.Metadata
{
    public class DataService : MetadataDictionary
    {
        public DataService() { }

        public DataService(Dictionary<string, object> dict) : base(dict) { }

        #region ServiceName

        public string ServiceName
        {
            get { return OriginalDictionary.GetValue<string>("serviceName"); }
            set { OriginalDictionary["serviceName"] = value; }
        }

        #endregion

        #region AdapterName

        /// <summary>
        /// On deserialization, this must match the name of some 'dataService adapter' already registered on the breeze client.
        /// </summary>
        public string AdapterName
        {
            get { return OriginalDictionary.GetValue<string>("adapterName"); }
            set { OriginalDictionary["adapterName"] = value; }
        }

        #endregion

        #region HasServerMetadata

        /// <summary>
        /// Whether the server can provide metadata for this service.
        /// </summary>
        public bool HasServerMetadata
        {
            get { return OriginalDictionary.GetValue<bool>("hasServerMetadata"); }
            set { OriginalDictionary["hasServerMetadata"] = value; }
        }

        #endregion

        #region JsonResultsAdapter

        /// <summary>
        /// On deserialization, this must match the name of some jsonResultsAdapter registered on the breeze client.
        /// </summary>
        public string JsonResultsAdapter
        {
            get { return OriginalDictionary.GetValue<string>("jsonResultsAdapter"); }
            set { OriginalDictionary["jsonResultsAdapter"] = value; }
        }

        #endregion

        #region UseJsonp

        /// <summary>
        /// Whether to use JSONP when performing a 'GET' request against this service.
        /// </summary>
        public bool UseJsonp
        {
            get { return OriginalDictionary.GetValue<bool>("useJsonp"); }
            set { OriginalDictionary["useJsonp"] = value; }
        }

        #endregion

        #region UriBuilderName

        /// <summary>
        /// The name of the uriBuilder to be used with this service.
        /// </summary>
        public string UriBuilderName
        {
            get { return OriginalDictionary.GetValue<string>("uriBuilderName"); }
            set { OriginalDictionary["uriBuilderName"] = value; }
        }

        #endregion
    }
}
