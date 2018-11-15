using FluentAssertions;
using Xunit;
using Yuriy.RabbitMq.Options;

namespace Yuriy.RabbitMq.Tests.Options
{
    public class ExclusivityTests
    {
        [Fact]
        public void Yes_ToBoolean_IsTrue()
        {
            var yes = Exclusivity.Yes;

            bool explicitResult = yes;
            var implicitResult = (bool)yes;

            explicitResult.Should().BeTrue();
            implicitResult.Should().BeTrue();
        }

        [Fact]
        public void No_ToBoolean_IsFalse()
        {
            var no = Exclusivity.No;

            bool explicitResult = no;
            var implicitResult = (bool)no;

            explicitResult.Should().BeFalse();
            implicitResult.Should().BeFalse();
        }
    }
}