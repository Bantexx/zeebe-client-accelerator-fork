using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client;

[TestFixture]
public class CreateProcessInstanceTest : BaseZeebeTest
{
    [Test]
    public async Task ShouldSendRequestAsExpected()
    {
        // given
        var expectedRequest = new CreateProcessInstanceRequest
        {
            BpmnProcessId = "process",
            Version = -1
        };

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
        .BpmnProcessId("process")
        .LatestVersion()
        .Send();

        // then
        var request = TestService.Requests[typeof(CreateProcessInstanceRequest)][0];
        Assert.That(expectedRequest, Is.EqualTo(request));
    }

    [Test]
    public void ShouldTimeoutRequest()
    {
        // given

        // when
        var task = ZeebeClient.NewCreateProcessInstanceCommand()
            .BpmnProcessId("process")
            .LatestVersion()
            .Send(TimeSpan.Zero);
        var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
        var rpcException = (RpcException)aggregateException.InnerExceptions[0];

        // then
        Assert.That(StatusCode.DeadlineExceeded, Is.EqualTo(rpcException.Status.StatusCode));
    }

    [Test]
    public void ShouldCancelRequest()
    {
        // given

        // when
        var task = ZeebeClient.NewCreateProcessInstanceCommand()
            .BpmnProcessId("process")
            .LatestVersion()
            .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
        var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
        var rpcException = (RpcException)aggregateException.InnerExceptions[0];

        // then
        Assert.That(StatusCode.Cancelled, Is.EqualTo(rpcException.Status.StatusCode));
    }

    [Test]
    public async Task ShouldSendRequestWithVersionAsExpected()
    {
        // given
        var expectedRequest = new CreateProcessInstanceRequest
        {
            BpmnProcessId = "process",
            Version = 1
        };

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
        .BpmnProcessId("process")
        .Version(1)
        .Send();

        // then
        var request = TestService.Requests[typeof(CreateProcessInstanceRequest)][0];
        Assert.That(expectedRequest, Is.EqualTo(request));
    }

    [Test]
    public async Task ShouldSendRequestWithProcessDefinitionKeyAsExpected()
    {
        // given
        var expectedRequest = new CreateProcessInstanceRequest
        {
            ProcessDefinitionKey = 1
        };

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
        .ProcessDefinitionKey(1)
        .Send();

        // then
        var request = TestService.Requests[typeof(CreateProcessInstanceRequest)][0];
        Assert.That(expectedRequest, Is.EqualTo(request));
    }

    [Test]
    public async Task ShouldSendRequestWithVariablesAsExpected()
    {
        // given
        var expectedRequest = new CreateProcessInstanceRequest
        {
            ProcessDefinitionKey = 1,
            Variables = "{\"foo\":1}"
        };

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
        .ProcessDefinitionKey(1)
        .Variables("{\"foo\":1}")
        .Send();

        // then
        var request = TestService.Requests[typeof(CreateProcessInstanceRequest)][0];
        Assert.That(expectedRequest, Is.EqualTo(request));
    }

    [Test]
    public async Task ShouldSendRequestWithVariablesAndProcessIdAsExpected()
    {
        // given
        var expectedRequest = new CreateProcessInstanceRequest
        {
            BpmnProcessId = "process",
            Version = -1,
            Variables = "{\"foo\":1}"
        };

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
        .BpmnProcessId("process")
        .LatestVersion()
        .Variables("{\"foo\":1}")
        .Send();

        // then
        var request = TestService.Requests[typeof(CreateProcessInstanceRequest)][0];
        Assert.That(expectedRequest, Is.EqualTo(request));
    }

    [Test]
    public async Task ShouldSendRequestWithTenantIdAsExpected()
    {
        // given
        var expectedRequest = new CreateProcessInstanceRequest
        {
            ProcessDefinitionKey = 1,
            TenantId = "tenant1"
        };

        // when
        _ = await ZeebeClient.NewCreateProcessInstanceCommand()
        .ProcessDefinitionKey(1)
        .AddTenantId("tenant1")
        .Send();

        // then
        var request = TestService.Requests[typeof(CreateProcessInstanceRequest)][0];
        Assert.That(expectedRequest, Is.EqualTo(request));
    }

    [Test]
    public async Task ShouldReceiveResponseAsExpected()
    {
        // given
        var expectedResponse = new CreateProcessInstanceResponse
        {
            BpmnProcessId = "process",
            Version = 1,
            ProcessDefinitionKey = 2,
            ProcessInstanceKey = 121
        };

        TestService.AddRequestHandler(typeof(CreateProcessInstanceRequest), request => expectedResponse);

        // when
        var processInstanceResponse = await ZeebeClient.NewCreateProcessInstanceCommand()
            .BpmnProcessId("process")
            .LatestVersion()
            .Send();

        // then
        Assert.That(2, Is.EqualTo(processInstanceResponse.ProcessDefinitionKey));
        Assert.That(1, Is.EqualTo(processInstanceResponse.Version));
        Assert.That(121, Is.EqualTo(processInstanceResponse.ProcessInstanceKey));
        Assert.That("process", Is.EqualTo(processInstanceResponse.BpmnProcessId));
    }
}