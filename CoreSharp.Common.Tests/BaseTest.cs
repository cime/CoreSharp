using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;

namespace CoreSharp.Common.Tests
{
    public abstract class BaseTest : IClassFixture<Bootstrapper>
    {
        private readonly Bootstrapper _bootstrapper;

        protected BaseTest(Bootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
            if (_bootstrapper.Initialized)
            {
                return;
            }

            _bootstrapper.Configure += Configure;
            _bootstrapper.Cleanup += Cleanup;
            _bootstrapper.Initialize();
        }

        protected Container Container => _bootstrapper.Container;

        protected virtual void ConfigureContainer(Container container)
        {

        }

        protected virtual void SetUp()
        {
            
        }

        protected virtual void Cleanup()
        {
        }

        private void Configure(Container container)
        {
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            ConfigureContainer(container);
            container.RegisterPackages();
            container.Verify();
            SetUp();
        }
    }
}
