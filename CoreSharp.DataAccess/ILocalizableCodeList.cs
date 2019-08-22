using System.Collections.Generic;

namespace CoreSharp.DataAccess
{
    public interface ILocalizableCodeList<TLanguage, TCodeList, TCodeListTranslation> : ICodeList
        where TLanguage : ICodeList
        where TCodeList : ILocalizableCodeList<TLanguage, TCodeList, TCodeListTranslation>
        where TCodeListTranslation : ILocalizableCodeListLanguage<TLanguage, TCodeList, TCodeListTranslation>
    {
        ISet<TCodeListTranslation> Translations { get; set; }

        void AddTranslation(TCodeListTranslation name);

        void RemoveTranslation(TCodeListTranslation name);
    }
}
