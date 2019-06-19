using System.Collections.ObjectModel;
using System.Reflection;
using Newtonsoft.Json;

namespace CoreSharp.Breeze
{
    public interface IBreezeConfig
    {
        JsonSerializerSettings GetJsonSerializerSettings();
        JsonSerializerSettings GetJsonSerializerSettingsForSave();

        ReadOnlyCollection<Assembly> ProbeAssemblies { get; }

        /// <summary>
        /// Returns TransactionSettings.Default.  Override to return different settings.
        /// </summary>
        /// <returns></returns>
        TransactionSettings GetTransactionSettings();
    }
}