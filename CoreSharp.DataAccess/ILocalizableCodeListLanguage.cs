namespace CoreSharp.DataAccess
{
    public interface ILocalizableCodeListLanguage
    {
        string LanguageCode { get; }

        string Name { get; set; }

        object GetCodeList();
    }

    public interface ILocalizableCodeListLanguage<TLanguage, TCodeList, TCodeListTranslation> : ILocalizableCodeListLanguage, IEntity
        where TLanguage : ICodeList
        where TCodeList : ICodeList
        where TCodeListTranslation : ILocalizableCodeListLanguage<TLanguage, TCodeList, TCodeListTranslation>
    {
        TCodeList CodeList { get; set; }

        void SetCodeList(TCodeList codeList);

        void UnsetCodeList();
    }
}
