using System;
using FluentAssertions;
using Xunit;

namespace CoreSharp.Common.Tests.Extensions
{
    public class TypeExtensionsTests
    {
        [Fact]
        public void IsSimpleTypeShouldSucceed()
        {
            1.GetType().IsSimpleType().Should().BeTrue();
            false.GetType().IsSimpleType().Should().BeTrue();
            new {}.GetType().IsSimpleType().Should().BeFalse();
        }
        
        [Fact]
        public void IsNumbericTypeShouldSucceed()
        {
            1.GetType().IsNumericType().Should().BeTrue();
            1.0.GetType().IsNumericType().Should().BeTrue();
        }
        
        [Fact]
        public void IsNullableShouldSucceed()
        {
            false.GetType().IsNullable().Should().BeFalse();
            1.GetType().IsNullable().Should().BeFalse();
        }
        
        [Fact]
        public void IsDateShouldSucceed()
        {
            DateTime.UtcNow.Date.GetType().IsDateTime().Should().BeTrue();
        }
    }
}
