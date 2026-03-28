using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Google.Protobuf;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client;

[TestFixture]
public class DeploymentTest : BaseZeebeTest
{
    private readonly string _demoProcessPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "demo-process.bpmn");

    private readonly string _demoFormPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "demo-form.form");

    [Test]
    public async Task ShouldSendDeployResourceFileAsExpected()
    {
        // given
        var expectedRequest = new DeployResourceRequest
        {
            Resources =
            {
                new Resource
                {
                    Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                    Name = _demoProcessPath
                }
            }
        };

        // when
        _ = await ZeebeClient.NewDeployCommand().AddResourceFile(_demoProcessPath).Send();

        // then
        var actualRequest = TestService.Requests[typeof(DeployResourceRequest)][0];

        Assert.That(expectedRequest, Is.EqualTo(actualRequest));
    }

    [Test]
    public void ShouldTimeoutRequest()
    {
        // given

        // when
        var task = ZeebeClient
            .NewDeployCommand()
            .AddResourceFile(_demoProcessPath)
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
        var task = ZeebeClient
            .NewDeployCommand()
            .AddResourceFile(_demoProcessPath)
            .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
        var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
        var rpcException = (RpcException)aggregateException.InnerExceptions[0];

        // then
        Assert.That(StatusCode.Cancelled, Is.EqualTo(rpcException.Status.StatusCode));
    }

    [Test]
    public async Task ShouldSendDeployResourceStringAsExpected()
    {
        // given
        var expectedRequest = new DeployResourceRequest
        {
            Resources =
            {
                new Resource
                {
                    Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                    Name = _demoProcessPath
                }
            }
        };

        // when
        var fileContent = File.ReadAllText(_demoProcessPath);
        _ = await ZeebeClient.NewDeployCommand()
        .AddResourceString(fileContent, Encoding.UTF8, _demoProcessPath)
        .Send();

        // then
        var actualRequest = TestService.Requests[typeof(DeployResourceRequest)][0];

        Assert.That(expectedRequest, Is.EqualTo(actualRequest));
    }

    [Test]
    public async Task ShouldSendDeployResourceStringUtf8AsExpected()
    {
        // given
        var expectedRequest = new DeployResourceRequest
        {
            Resources =
            {
                new Resource
                {
                    Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                    Name = _demoProcessPath
                }
            }
        };

        // when
        var fileContent = File.ReadAllText(_demoProcessPath);
        _ = await ZeebeClient.NewDeployCommand()
        .AddResourceStringUtf8(fileContent, _demoProcessPath)
        .Send();

        // then
        var actualRequest = TestService.Requests[typeof(DeployResourceRequest)][0];

        Assert.That(expectedRequest, Is.EqualTo(actualRequest));
    }

    [Test]
    public async Task ShouldSendDeployResourceBytesAsExpected()
    {
        // given
        var expectedRequest = new DeployResourceRequest
        {
            Resources =
            {
                new Resource
                {
                    Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                    Name = _demoProcessPath
                }
            }
        };

        // when
        var fileContent = File.ReadAllText(_demoProcessPath);
        _ = await ZeebeClient.NewDeployCommand()
        .AddResourceBytes(Encoding.UTF8.GetBytes(fileContent), _demoProcessPath)
        .Send();

        // then
        var actualRequest = TestService.Requests[typeof(DeployResourceRequest)][0];

        Assert.That(expectedRequest, Is.EqualTo(actualRequest));
    }

    [Test]
    public async Task ShouldSendDeployResourceStreamAsExpected()
    {
        // given
        var expectedRequest = new DeployResourceRequest
        {
            Resources =
            {
                new Resource
                {
                    Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                    Name = _demoProcessPath
                }
            }
        };

        // when
        _ = await ZeebeClient.NewDeployCommand()
        .AddResourceStream(File.OpenRead(_demoProcessPath), _demoProcessPath)
        .Send();

        // then
        var actualRequest = TestService.Requests[typeof(DeployResourceRequest)][0];

        Assert.That(expectedRequest, Is.EqualTo(actualRequest));
    }

    [Test]
    public async Task ShouldSendDeployResourceAndGetResponseAsExpected()
    {
        // given
        var expectedResponse = new DeployResourceResponse
        {
            Key = 1,
            Deployments =
            {
                new Deployment
                {
                    Process = new ProcessMetadata
                    {
                        BpmnProcessId = "process",
                        ResourceName = _demoProcessPath,
                        Version = 1,
                        ProcessDefinitionKey = 2
                    }
                }
            }
        };

        TestService.AddRequestHandler(typeof(DeployResourceRequest), request => expectedResponse);

        // when
        var deployProcessResponse = await ZeebeClient.NewDeployCommand()
            .AddResourceFile(_demoProcessPath)
            .Send();

        // then
        Assert.That(1, Is.EqualTo(deployProcessResponse.Key));
        Assert.That(1, Is.EqualTo(deployProcessResponse.Processes.Count));

        var processMetadata = deployProcessResponse.Processes[0];
        Assert.That("process", Is.EqualTo(processMetadata.BpmnProcessId));
        Assert.That(1, Is.EqualTo(processMetadata.Version));
        Assert.That(_demoProcessPath, Is.EqualTo(processMetadata.ResourceName));
        Assert.That(2, Is.EqualTo(processMetadata.ProcessDefinitionKey));
    }

    [Test]
    public async Task ShouldSendMultipleDeployResourceAsExpected()
    {
        // given
        var expectedRequest = new DeployResourceRequest
        {
            Resources =
            {
                new Resource
                {
                    Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                    Name = _demoProcessPath
                },
                new Resource
                {
                    Content = ByteString.FromStream(File.OpenRead(_demoFormPath)),
                    Name = _demoFormPath
                }
            }
        };

        // when
        _ = await ZeebeClient.NewDeployCommand()
        .AddResourceFile(_demoProcessPath)
        .AddResourceStream(File.OpenRead(_demoFormPath), _demoFormPath)
        .Send();

        // then
        var actualRequest = TestService.Requests[typeof(DeployResourceRequest)][0];

        Assert.That(expectedRequest, Is.EqualTo(actualRequest));
    }

    [Test]
    public async Task ShouldSendMultipleDeployResourceAndGetResponseAsExpected()
    {
        // given
        var expectedResponse = new DeployResourceResponse
        {
            Key = 1,
            Deployments =
            {
                new Deployment
                {
                    Process = new ProcessMetadata
                    {
                        BpmnProcessId = "process",
                        ResourceName = _demoProcessPath,
                        Version = 1,
                        ProcessDefinitionKey = 2
                    }
                },
                new Deployment
                {
                    Decision = new DecisionMetadata
                    {
                        DecisionKey = 1,
                        DecisionRequirementsKey = 2,
                        Version = 3,
                        DmnDecisionId = "decisionId",
                        DmnDecisionName = "decisionName",
                        DmnDecisionRequirementsId = "idk"
                    }
                },
                new Deployment
                {
                    DecisionRequirements = new DecisionRequirementsMetadata
                    {
                        Version = 1,
                        ResourceName = "requirement",
                        DecisionRequirementsKey = 2,
                        DmnDecisionRequirementsId = "id",
                        DmnDecisionRequirementsName = "nameRequirement"
                    }
                },
                new Deployment
                {
                    Form = new FormMetadata
                    {
                        FormKey = 3,
                        Version = 1,
                        ResourceName = "form",
                        FormId = "demoForm",
                        TenantId = "formTenantId"
                    }
                }
            }
        };

        TestService.AddRequestHandler(typeof(DeployResourceRequest), request => expectedResponse);

        // when
        var processFileContent = await File.ReadAllTextAsync(_demoProcessPath);
        var deployProcessResponse = await ZeebeClient.NewDeployCommand()
            .AddResourceString(processFileContent, Encoding.UTF8, _demoProcessPath)
            .AddResourceFile(_demoFormPath)
            .Send();

        // then
        Assert.That(1, Is.EqualTo(deployProcessResponse.Key));
        Assert.That(1, Is.EqualTo(deployProcessResponse.Processes.Count));
        Assert.That(1, Is.EqualTo(deployProcessResponse.Decisions.Count));
        Assert.That(1, Is.EqualTo(deployProcessResponse.DecisionRequirements.Count));
        Assert.That(1, Is.EqualTo(deployProcessResponse.Forms.Count));

        var processMetadata = deployProcessResponse.Processes[0];
        Assert.That("process", Is.EqualTo(processMetadata.BpmnProcessId));
        Assert.That(1, Is.EqualTo(processMetadata.Version));
        Assert.That(_demoProcessPath, Is.EqualTo(processMetadata.ResourceName));
        Assert.That(2, Is.EqualTo(processMetadata.ProcessDefinitionKey));

        var decisionMetadata = deployProcessResponse.Decisions[0];
        Assert.That(1, Is.EqualTo(decisionMetadata.DecisionKey));
        Assert.That(2, Is.EqualTo(decisionMetadata.DecisionRequirementsKey));
        Assert.That(3, Is.EqualTo(decisionMetadata.Version));
        Assert.That("decisionId", Is.EqualTo(decisionMetadata.DmnDecisionId));
        Assert.That("decisionName", Is.EqualTo(decisionMetadata.DmnDecisionName));
        Assert.That("idk", Is.EqualTo(decisionMetadata.DmnDecisionRequirementsId));

        var decisionRequirementsMetadata = deployProcessResponse.DecisionRequirements[0];
        Assert.That(2, Is.EqualTo(decisionRequirementsMetadata.DecisionRequirementsKey));
        Assert.That(1, Is.EqualTo(decisionRequirementsMetadata.Version));
        Assert.That("requirement", Is.EqualTo(decisionRequirementsMetadata.ResourceName));
        Assert.That("nameRequirement", Is.EqualTo(decisionRequirementsMetadata.DmnDecisionRequirementsName));
        Assert.That("id", Is.EqualTo(decisionRequirementsMetadata.DmnDecisionRequirementsId));

        var formMetadata = deployProcessResponse.Forms[0];
        Assert.That(3, Is.EqualTo(formMetadata.FormKey));
        Assert.That(1, Is.EqualTo(formMetadata.Version));
        Assert.That("form", Is.EqualTo(formMetadata.ResourceName));
        Assert.That("demoForm", Is.EqualTo(formMetadata.FormId));
        Assert.That("formTenantId", Is.EqualTo(formMetadata.TenantId));
    }

    [Test]
    public async Task ShouldSetTenantIdAsExpected()
    {
        // given
        var expectedRequest = new DeployResourceRequest
        {
            TenantId = "1234",
            Resources =
            {
                new Resource
                {
                    Content = ByteString.FromStream(File.OpenRead(_demoProcessPath)),
                    Name = _demoProcessPath
                }
            }
        };

        // when
        _ = await ZeebeClient.NewDeployCommand().AddResourceFile(_demoProcessPath).AddTenantId("1234").Send();

        // then
        var actualRequest = TestService.Requests[typeof(DeployResourceRequest)][0];

        Assert.That(expectedRequest, Is.EqualTo(actualRequest));
    }
}