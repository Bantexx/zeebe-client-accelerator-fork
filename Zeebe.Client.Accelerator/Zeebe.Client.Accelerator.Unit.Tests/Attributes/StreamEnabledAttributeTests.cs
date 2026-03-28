using Xunit;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Attributes
{
    public class StreamEnabledAttributeTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void StreamEnabledPropertyMatchesConstructorArgument(bool streamEnabled)
        {
            var attribute = new StreamEnabledAttribute(streamEnabled);
            Assert.Equal(streamEnabled, attribute.StreamEnabled);
        }
    }
}
