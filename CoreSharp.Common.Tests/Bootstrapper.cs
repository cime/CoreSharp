using System;
using System.Collections.Generic;
using SimpleInjector;

namespace CoreSharp.Common.Tests
{
    public class Bootstrapper : IDisposable
    {
        private bool _initialized;
        private List<Action<Container>> _beforeInitializationActions = new List<Action<Container>>();

        public Bootstrapper()
        {
            Container = new Container();
        }

        public Container Container { get; }

        public void BeforeInitialization(Action<Container> action)
        {
            _beforeInitializationActions.Add(action);
        }

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            foreach (var action in _beforeInitializationActions)
            {
                action(Container);
            }

            Container.Verify();
            _initialized = true;
        }

        public void Dispose()
        {
            Container.Dispose();
        }
    }
}
