using FluentAssertions;
using Xunit;
using Yuriy.RabbitMq.Options;

namespace Yuriy.RabbitMq.Tests.Options
{
    public class DurabilityTests
    {
        [Fact]
        public void Durable_ToBoolean_IsTrue()
        {
            var durable = Durability.Durable;

            bool explicitResult = durable;
            var implicitResult = (bool)durable;

            explicitResult.Should().BeTrue();
            implicitResult.Should().BeTrue();
        }

        [Fact]
        public void Transient_ToBoolean_IsFalse()
        {
            var transient = Durability.Transient;

            bool explicitResult = transient;
            var implicitResult = (bool)transient;

            explicitResult.Should().BeFalse();
            implicitResult.Should().BeFalse();
        }
    }
}