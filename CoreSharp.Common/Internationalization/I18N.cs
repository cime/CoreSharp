using System;
using System.Collections.Generic;
using System.Globalization;
using NGettext;

namespace CoreSharp.Common.Internationalization
{
    public static class I18N
    {
        private static readonly CultureInfo EnglishCulture = new CultureInfo("en-US");
        private static readonly Dictionary<CultureInfo, ICatalog> Catalogs = new Dictionary<CultureInfo, ICatalog>();

        static I18N()
        {
            Catalogs.Add(EnglishCulture, new Catalog());
        }

        public static void AddCatalog(CultureInfo cultureInfo, ICatalog catalog)
        {
            if (cultureInfo == null)
            {
                throw new ArgumentNullException(nameof(cultureInfo));
            }

            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            Catalogs[cultureInfo] = catalog;
        }

        /// <summary>
        /// Use this function in order to register string as localizable (passing a variable as parameter will not work!)
        /// Valid usage: I18N.register("Hello world")
        /// </summary>
        /// <param name="id"></param>
        /// <returns>id</returns>
        public static string Register(string id)
        {
            return id;
        }

        private static ICatalog GetCatalog()
        {
            if (Catalogs.ContainsKey(CultureInfo.CurrentCulture))
            {
                return Catalogs[CultureInfo.CurrentCulture];
            }

            return Catalogs[EnglishCulture];
        }

        private static ICatalog GetCatalog(CultureInfo cultureInfo)
        {
            if (Catalogs.ContainsKey(cultureInfo))
            {
                return Catalogs[cultureInfo];
            }

            return Catalogs[EnglishCulture];
        }

        public static string _(string text)
        {
            return GetCatalog().GetString(text);
        }

        public static string _(string text, CultureInfo cultureInfo)
        {
            return GetCatalog(cultureInfo).GetString(text);
        }

        public static string _(string text, params object[] args)
        {
            return GetCatalog().GetString(text, args);
        }

        public static string _n(string text, string pluralText, long n)
        {
            return GetCatalog().GetPluralString(text, pluralText, n);
        }

        public static string _n(string text, string pluralText, long n, params object[] args)
        {
            return GetCatalog().GetPluralString(text, pluralText, n, args);
        }

        public static string _p(string context, string text)
        {
            return GetCatalog().GetParticularString(context, text);
        }

        public static string _p(string context, string text, params object[] args)
        {
            return GetCatalog().GetParticularString(context, text, args);
        }

        public static string _pn(string context, string text, string pluralText, long n)
        {
            return GetCatalog().GetParticularPluralString(context, text, pluralText, n);
        }

        public static string _pn(string context, string text, string pluralText, long n, params object[] args)
        {
            return GetCatalog().GetParticularPluralString(context, text, pluralText, n, args);
        }
    }
}
