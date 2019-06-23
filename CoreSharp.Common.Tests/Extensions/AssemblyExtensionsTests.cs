using CoreSharp.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace CoreSharp.Common.Tests.Extensions
{
    public class AssemblyExtensionsTests
    {
        [Fact]
        public void GetDependentAssembliesShouldSucceed()
        {
            typeof(CoreSharp.Common.Package).Assembly.GetDependentAssemblies().Should().Contain(typeof(AssemblyExtensionsTests).Assembly);
        }
    }
}
