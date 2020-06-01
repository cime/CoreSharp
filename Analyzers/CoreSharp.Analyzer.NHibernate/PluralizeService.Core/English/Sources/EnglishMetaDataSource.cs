using CoreSharp.Analyzer.NHibernate.PluralizeService.Core.Core.Builder;
using CoreSharp.Analyzer.NHibernate.PluralizeService.Core.Core.Builder.Base;
using CoreSharp.Analyzer.NHibernate.PluralizeService.Core.English.Providers;
using CoreSharp.Analyzer.NHibernate.PluralizeService.Core.Sources;

namespace CoreSharp.Analyzer.NHibernate.PluralizeService.Core.English.Sources
{
    internal class EnglishMetaDataSource : SourceBase, IPluralizationSource
    {
        // *******************************************************************
        // Protected methods.
        // *******************************************************************

        #region Protected methods

        protected override IBuilderProvider OnBuild(IBuilder builder)
        {
            return new EnglishMetaDataProvider(this);
        }


        #endregion
    }
}
