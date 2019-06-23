using CoreSharp.Cqrs.Events;
using FluentAssertions;
using Xunit;

namespace CoreSharp.Cqrs.Tests.Events
{
    public class UnhandledExceptionEventTests
    {
        [Fact]
        public void ShouldSucceed()
        {
            new UnhandledExceptionEvent(null).Should().NotBeNull();
            new UnhandledExceptionEvent(null).Exception.Should().BeNull();
        }
    }
}
