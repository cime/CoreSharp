using System;
using SimpleInjector;

namespace CoreSharp.Common.Tests
{
    public class Bootstrapper : IDisposable
    {
        public Bootstrapper()
        {
            Container = new Container();
        }

        public Container Container { get; }

        public bool Initialized { get; private set; }

        public event Action<Container> Configure;

        public event Action Cleanup;

        public void Initialize()
        {
            if (Initialized)
            {
                return;
            }

            Configure?.Invoke(Container);

            Initialized = true;
        }
        
        public void Dispose()
        {
            Cleanup?.Invoke();
            Container.Dispose();
        }
    }
}
