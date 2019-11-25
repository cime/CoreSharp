using System;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;
using CoreSharp.DataAccess.Attributes;
using CoreSharp.NHibernate.CodeList.Extensions;

namespace CoreSharp.NHibernate.CodeList
{
    [Ignore]
    [Serializable]
    public abstract class LocalizableCodeListTranslation<TLanguage, TCodeList, TCodeListTranslation> : Entity, ILocalizableCodeListTranslation<TLanguage, TCodeList, TCodeListTranslation>
        where TLanguage : ICodeList
        where TCodeList : class, ILocalizableCodeList<TLanguage, TCodeList, TCodeListTranslation>
        where TCodeListTranslation : class, ILocalizableCodeListTranslation<TLanguage, TCodeList, TCodeListTranslation>
    {
        public virtual TCodeList CodeList { get; set; }

        [Formula("LanguageId")]
        public virtual string LanguageId { get; }
        public virtual TLanguage Language { get; set; }

        public virtual string Name { get; set; }

        public virtual object GetCodeList()
        {
            return CodeList;
        }

        public virtual void SetCodeList(TCodeList codeList)
        {
            var me = this as TCodeListTranslation;
            me.SetManyToOne(o => o.CodeList, codeList, o => o.RemoveTranslation, o => o.Translations);
        }

        public virtual void UnsetCodeList()
        {
            var me = this as TCodeListTranslation;
            me.UnsetManyToOne(o => o.CodeList, o => o.Translations);
        }
    }
}
