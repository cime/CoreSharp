using System;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;
using CoreSharp.NHibernate.CodeList.Extensions;

namespace CoreSharp.NHibernate.CodeList
{
    [Ignore]
    [Serializable]
    public abstract class LocalizableCodeListLanguage<TCodeList, TCodeListNames> : Entity, ILocalizableCodeListLanguage<TCodeList, TCodeListNames>
        where TCodeList : class, ILocalizableCodeList<TCodeList, TCodeListNames>
        where TCodeListNames : class, ILocalizableCodeListLanguage<TCodeList, TCodeListNames>
    {
        public virtual TCodeList CodeList { get; set; }

        public virtual string LanguageCode { get; set; }

        public virtual string Name { get; set; }

        public virtual object GetCodeList()
        {
            return CodeList;
        }

        public virtual void SetCodeList(TCodeList codeList)
        {
            var me = this as TCodeListNames;
            me.SetManyToOne(o => o.CodeList, codeList, o => o.RemoveName, o => o.Names);
        }

        public virtual void UnsetCodeList()
        {
            var me = this as TCodeListNames;
            me.UnsetManyToOne(o => o.CodeList, o => o.Names);
        }
    }
}
