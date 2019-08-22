using System;
using System.Collections.Generic;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;
using CoreSharp.DataAccess.Attributes;
using CoreSharp.NHibernate.CodeList.Extensions;

namespace CoreSharp.NHibernate.CodeList
{
    [Ignore]
    [Serializable]
    public abstract class LocalizableCodeListWithUser<TUser, TLanguage, TCodeList, TCodeListTranslation> : VersionedEntityWithUser<TUser, string>, ILocalizableCodeList<TLanguage, TCodeList, TCodeListTranslation>
        where TLanguage : ICodeList
        where TCodeListTranslation : class, ILocalizableCodeListLanguage<TLanguage, TCodeList, TCodeListTranslation>
        where TCodeList : LocalizableCodeListWithUser<TUser, TLanguage, TCodeList, TCodeListTranslation>
    {
        public virtual ISet<TCodeListTranslation> Translations { get; set; } = new HashSet<TCodeListTranslation>();

        public virtual bool Active { get; set; } = true;

        public virtual void AddTranslation(TCodeListTranslation name)
        {
            this.AddOneToMany(o => o.Translations, name, o => o.CodeList, o => RemoveTranslation);
        }

        public virtual void RemoveTranslation(TCodeListTranslation name)
        {
            this.RemoveOneToMany(o => o.Translations, name, o => o.CodeList);
        }

        // Id can be changed via Code so we have to check CreatedDate
        public override bool IsTransient()
        {
            return CreatedDate == DateTime.MinValue;
        }
    }
}
