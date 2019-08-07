using System.Collections.Generic;

namespace CoreSharp.DataAccess
{
    public interface ILocalizableCodeList<TCodeList, TCodeListNames> : ICodeList
        where TCodeList : ILocalizableCodeList<TCodeList, TCodeListNames>
        where TCodeListNames : ILocalizableCodeListLanguage<TCodeList, TCodeListNames>
    {
        ISet<TCodeListNames> Names { get; }

        void AddName(TCodeListNames name);

        void RemoveName(TCodeListNames name);
    }
}
