namespace CoreSharp.DataAccess
{
    public interface ILocalizableCodeListLanguage
    {
        string LanguageCode { get; set; }

        string Name { get; set; }

        object GetCodeList();
    }

    public interface ILocalizableCodeListLanguage<TCodeList, TCodeListNames> : ILocalizableCodeListLanguage, IEntity
        where TCodeList : ICodeList
        where TCodeListNames : ILocalizableCodeListLanguage<TCodeList, TCodeListNames>
    {
        TCodeList CodeList { get; set; } // TODO: remove setter after T4FluentHN is fixed

        void SetCodeList(TCodeList codeList);

        void UnsetCodeList();
    }
}
