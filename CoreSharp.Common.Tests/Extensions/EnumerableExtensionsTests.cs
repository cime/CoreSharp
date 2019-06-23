using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace CoreSharp.Common.Tests.Extensions
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void GetRandomShouldSucceed()
        {
            var numbers = new[] {1, 2, 3};

            numbers.Should().Contain(numbers.GetRandom());
        }
    }
}
