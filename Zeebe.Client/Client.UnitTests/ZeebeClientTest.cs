using System;
using System.IO;
using NUnit.Framework;

namespace Zeebe.Client;

[TestFixture]
public class ZeebeClientTest
{
    private static readonly string ServerCertPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "chain.cert.pem");

    private static readonly string ClientCertPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "chain.cert.pem");

    private static readonly string ServerKeyPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "private.key.pem");

    private static readonly string WrongCertPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "server.crt");

    [Test]
    public void ShouldThrowExceptionAfterDispose()
    {
        // given
        var zeebeClient = ZeebeClient.Builder()
            .UseGatewayAddress("localhost:26500")
            .UsePlainText()
            .Build();

        // when
        zeebeClient.Dispose();

        // then
        var aggregateException = Assert.Throws<AggregateException>(
            () => zeebeClient.TopologyRequest().Send().Wait());

        Assert.That(1, Is.EqualTo(aggregateException.InnerExceptions.Count));

        var catchedExceptions = aggregateException.InnerExceptions[0];
        Assert.That(catchedExceptions.Message.Contains("ZeebeClient was already disposed."), Is.True);
        Assert.That(catchedExceptions, Is.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public void ShouldNotThrowExceptionWhenDisposingMultipleTimes()
    {
        // given
        var zeebeClient = ZeebeClient.Builder()
            .UseGatewayAddress("localhost:26500")
            .UsePlainText()
            .Build();

        // when
        zeebeClient.Dispose();

        // then
        Assert.DoesNotThrow(() => zeebeClient.Dispose());
    }

    [Test]
    public void ShouldCalculateNextGreaterWaitTime()
    {
        // given
        var defaultWaitTimeProvider = ZeebeClient.DefaultWaitTimeProvider;

        // when
        var firstSpan = defaultWaitTimeProvider.Invoke(1);
        var secondSpan = defaultWaitTimeProvider.Invoke(2);

        // then
        Assert.That(secondSpan, Is.GreaterThan(firstSpan));
    }

    [Test]
    public void ShouldReturnMaxIfReachingMaxWaitTime()
    {
        // given
        var defaultWaitTimeProvider = ZeebeClient.DefaultWaitTimeProvider;

        // when
        var maxTime = defaultWaitTimeProvider.Invoke(100);

        // then
        Assert.That(TimeSpan.FromSeconds(ZeebeClient.MaxWaitTimeInSeconds), Is.EqualTo(maxTime));
    }
}