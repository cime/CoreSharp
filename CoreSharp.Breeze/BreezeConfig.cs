using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CoreSharp.Breeze
{
    public class BreezeConfig : IBreezeConfig
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings = CreateJsonSerializerSettings();
        private readonly JsonSerializerSettings _jsonSerializerSettingsForSave = CreateJsonSerializerSettingsForSave();

        public BreezeConfig()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (sender, args) =>
            {
                Interlocked.Increment(ref _assemblyLoadedCount);
            };
        }

        public JsonSerializerSettings GetJsonSerializerSettings()
        {
            return _jsonSerializerSettings;
        }

        public JsonSerializerSettings GetJsonSerializerSettingsForSave()
        {
            return _jsonSerializerSettingsForSave;
        }

        public ReadOnlyCollection<Assembly> ProbeAssemblies
        {
            get
            {
                if (_assemblyCount == 0 || _assemblyCount != _assemblyLoadedCount)
                {
                    // Cache the ProbeAssemblies.
                    _probeAssemblies = new ReadOnlyCollection<Assembly>(AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => !IsFrameworkAssembly(a)).ToList());
                    _assemblyCount = _assemblyLoadedCount;
                }

                return _probeAssemblies;
            }
        }

        private static ReadOnlyCollection<Assembly> _probeAssemblies;
        private static int _assemblyCount = 0;
        private static int _assemblyLoadedCount = 0;

        /// <summary>
        /// Override to use a specialized JsonSerializer implementation.
        /// </summary>
        private static JsonSerializerSettings CreateJsonSerializerSettings()
        {
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Include,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            };

            // Default is DateTimeZoneHandling.RoundtripKind - you can change that here.
            // jsonSerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

            // Hack is for the issue described in this post:
            // http://stackoverflow.com/questions/11789114/internet-explorer-json-net-javascript-date-and-milliseconds-issue
            jsonSerializerSettings.Converters.Add(new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy-MM-dd\\THH:mm:ss.fffK"
                // DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK"
            });
            // Needed because JSON.NET does not natively support I8601 Duration formats for TimeSpan
            jsonSerializerSettings.Converters.Add(new TimeSpanConverter());
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());

            return jsonSerializerSettings;
        }

        /// <summary>
        /// Override to use a specialized JsonSerializer implementation for saving.
        /// Base implementation uses CreateJsonSerializerSettings, then changes TypeNameHandling to None.
        /// http://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_TypeNameHandling.htm
        /// </summary>
        private static JsonSerializerSettings CreateJsonSerializerSettingsForSave()
        {
            var settings = CreateJsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.None;

            return settings;
        }

        public static bool IsFrameworkAssembly(Assembly assembly)
        {
            var fullName = assembly.FullName;
            if (fullName.StartsWith("Microsoft.")) return true;
            if (fullName.StartsWith("EntityFramework")) return true;
            if (fullName.StartsWith("NHibernate")) return true;
            var attrs = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)
                .OfType<AssemblyProductAttribute>();
            var attr = attrs.FirstOrDefault();

            if (attr == null)
            {
                return false;
            }

            var productName = attr.Product;

            return FrameworkProductNames.Any(nm => productName.StartsWith(nm));
        }

        protected static readonly List<string> FrameworkProductNames = new List<string>
        {
            "Microsoft®",
            "Microsoft (R)",
            "Microsoft ASP.",
            "System.Net.Http",
            "Json.NET",
            "Antlr3.Runtime",
            "Iesi.Collections",
            "WebGrease",
            "Breeze.ContextProvider"
        };

        /// <summary>
        /// Returns TransactionSettings.Default.  Override to return different settings.
        /// </summary>
        /// <returns></returns>
        public virtual TransactionSettings GetTransactionSettings()
        {
            return TransactionSettings.Default;
        }
    }

    // http://www.w3.org/TR/xmlschema-2/#duration
    public class TimeSpanConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var ts = (TimeSpan) value;
            var tsString = XmlConvert.ToString(ts);
            serializer.Serialize(writer, tsString);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var value = serializer.Deserialize<string>(reader);

            return XmlConvert.ToTimeSpan(value);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);
        }
    }
}
