namespace CoreSharp.DataAccess
{
    public interface ILocalizableCodeListTranslation
    {
        string LanguageId { get; }

        string Name { get; set; }

        object GetCodeList();
    }

    public interface ILocalizableCodeListTranslation<TLanguage, TCodeList, TCodeListTranslation> : ILocalizableCodeListTranslation, IEntity
        where TLanguage : ICodeList
        where TCodeList : ICodeList
        where TCodeListTranslation : ILocalizableCodeListTranslation<TLanguage, TCodeList, TCodeListTranslation>
    {
        TCodeList CodeList { get; set; }

        void SetCodeList(TCodeList codeList);

        void UnsetCodeList();
    }
}
