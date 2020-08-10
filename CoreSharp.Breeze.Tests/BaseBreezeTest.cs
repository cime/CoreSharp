using System;
using System.Collections.Generic;
using System.Reflection;
using CoreSharp.Breeze.Tests.Entities;
using CoreSharp.Common.Tests;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace CoreSharp.Breeze.Tests
{
    public class BaseBreezeTest : BaseDatabaseTest
    {
        public BaseBreezeTest(Bootstrapper bootstrapper) : base(bootstrapper)
        {
        }

        protected override IEnumerable<Assembly> GetEntityAssemblies()
        {
            yield return typeof(BaseBreezeTest).Assembly;
        }

        protected override void ConfigureContainer(Container container)
        {
            container.RegisterValidatorsFromAssemblyOf<Order>();
            base.ConfigureContainer(container);
        }

        protected Container CreateTestContainer(Action<BreezeOptions> configureAction = null)
        {
            var testContainer = new Container();
            testContainer.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            ConfigureContainer(testContainer);
            testContainer.RegisterValidatorsFromAssemblyOf<Order>();
            testContainer.AddBreeze(configureAction);

            testContainer.Register<BreezeEntityManager>(Lifestyle.Transient);
            testContainer.RegisterPackages();

            testContainer.Verify();

            return testContainer;
        }
    }
}
