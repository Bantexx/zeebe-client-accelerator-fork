using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client;

[TestFixture]
public class RequestMetadataTest : BaseZeebeTest
{
    [Test]
    public async Task ShouldUseUserAgentHeader()
    {
        // given
        Metadata sendMetadata = null;
        TestService.ConsumeRequestHeaders(metadata => { sendMetadata = metadata; });

        // when
        _ = await ZeebeClient.TopologyRequest().Send();

        // then
        Assert.That(sendMetadata, Is.Not.Null);

        var entry = sendMetadata[0];
        Assert.That(entry.Key, Is.EqualTo("user-agent"), $"Expect user agent in metadata '{sendMetadata}'");
        var expectedUserAgent = "zeebe-client-csharp/" + typeof(ZeebeClient).Assembly.GetName().Version;
        Assert.That(entry.Value, Does.Contain(expectedUserAgent), $"Expect user agent contains zeebe-client-csharp, but was {entry}");
    }
}