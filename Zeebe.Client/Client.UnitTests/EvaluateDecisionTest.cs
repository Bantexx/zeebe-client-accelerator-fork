using System;
using System.Threading;
using System.Threading.Tasks;
using GatewayProtocol;
using Grpc.Core;
using NUnit.Framework;

namespace Zeebe.Client;

[TestFixture]
public class EvaluateDecisionTest : BaseZeebeTest
{
    [Test]
    public async Task ShouldSendRequestAsExpected()
    {
        // given
        var expectedRequest = new EvaluateDecisionRequest
        {
            DecisionId = "decision"
        };

        // when
        _ = await ZeebeClient.NewEvaluateDecisionCommand()
        .DecisionId("decision")
        .Send();

        // then
        var request = TestService.Requests[typeof(EvaluateDecisionRequest)][0];
        Assert.That(expectedRequest, Is.EqualTo(request));
    }

    [Test]
    public void ShouldTimeoutRequest()
    {
        // given

        // when
        var task = ZeebeClient.NewEvaluateDecisionCommand()
            .DecisionId("decision")
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
        var task = ZeebeClient.NewEvaluateDecisionCommand()
            .DecisionId("decision")
            .Send(new CancellationTokenSource(TimeSpan.Zero).Token);
        var aggregateException = Assert.Throws<AggregateException>(() => task.Wait());
        var rpcException = (RpcException)aggregateException.InnerExceptions[0];

        // then
        Assert.That(StatusCode.Cancelled, Is.EqualTo(rpcException.Status.StatusCode));
    }

    [Test]
    public async Task ShouldSendRequestWithDecisionKeyExpected()
    {
        // given
        var expectedRequest = new EvaluateDecisionRequest
        {
            DecisionKey = 12
        };

        // when
        _ = await ZeebeClient.NewEvaluateDecisionCommand()
        .DecisionKey(12)
        .Send();

        // then
        var request = TestService.Requests[typeof(EvaluateDecisionRequest)][0];
        Assert.That(expectedRequest, Is.EqualTo(request));
    }

    [Test]
    public async Task ShouldSendRequestWithVariablesAsExpected()
    {
        // given
        var expectedRequest = new EvaluateDecisionRequest
        {
            DecisionKey = 12,
            Variables = "{\"foo\":1}"
        };

        // when
        _ = await ZeebeClient.NewEvaluateDecisionCommand()
        .DecisionKey(12)
        .Variables("{\"foo\":1}")
        .Send();

        // then
        var request = TestService.Requests[typeof(EvaluateDecisionRequest)][0];
        Assert.That(expectedRequest, Is.EqualTo(request));
    }

    [Test]
    public async Task ShouldSendRequestWithVariablesAndDecisionIdAsExpected()
    {
        // given
        var expectedRequest = new EvaluateDecisionRequest
        {
            DecisionId = "decision",
            Variables = "{\"foo\":1}"
        };

        // when
        _ = await ZeebeClient.NewEvaluateDecisionCommand()
        .DecisionId("decision")
        .Variables("{\"foo\":1}")
        .Send();

        // then
        var request = TestService.Requests[typeof(EvaluateDecisionRequest)][0];
        Assert.That(expectedRequest, Is.EqualTo(request));
    }

    [Test]
    public async Task ShouldReceiveResponseAsExpected()
    {
        // given
        var expectedResponse = new EvaluateDecisionResponse
        {
            DecisionId = "decision",
            DecisionKey = 123,
            DecisionName = "decision-123",
            DecisionOutput = "1",
            DecisionVersion = 2,
            FailureMessage = "",
            FailedDecisionId = "",
            DecisionRequirementsId = "12",
            DecisionRequirementsKey = 1234,
            EvaluatedDecisions =
            {
                new EvaluatedDecision
                {
                    DecisionId = "decision",
                    DecisionKey = 123,
                    DecisionName = "decision-123",
                    DecisionOutput = "1",
                    DecisionVersion = 2,
                    DecisionType = "noop",
                    EvaluatedInputs =
                    {
                        new EvaluatedDecisionInput
                        {
                            InputId = "moep",
                            InputName = "moepmoep",
                            InputValue = "boom"
                        },
                        new EvaluatedDecisionInput
                        {
                            InputId = "moeb",
                            InputName = "moebmoeb",
                            InputValue = "boom"
                        }
                    },
                    MatchedRules =
                    {
                        new MatchedDecisionRule
                        {
                            EvaluatedOutputs =
                            {
                                new EvaluatedDecisionOutput
                                {
                                    OutputId = "outputId",
                                    OutputName = "output",
                                    OutputValue = "val"
                                },
                                new EvaluatedDecisionOutput
                                {
                                    OutputId = "outputId2",
                                    OutputName = "output2",
                                    OutputValue = "val2"
                                }
                            },
                            RuleId = "ruleid",
                            RuleIndex = 1
                        }
                    }
                }
            }
        };

        TestService.AddRequestHandler(typeof(EvaluateDecisionRequest), request => expectedResponse);

        // when
        var evaluatedDecisionResponse = await ZeebeClient.NewEvaluateDecisionCommand()
            .DecisionId("decision")
            .Send();

        // then
        Assert.That("decision", Is.EqualTo(evaluatedDecisionResponse.DecisionId));
        Assert.That(123, Is.EqualTo(evaluatedDecisionResponse.DecisionKey));
        Assert.That("decision-123", Is.EqualTo(evaluatedDecisionResponse.DecisionName));
        Assert.That("1", Is.EqualTo(evaluatedDecisionResponse.DecisionOutput));
        Assert.That(2, Is.EqualTo(evaluatedDecisionResponse.DecisionVersion));
        Assert.That("", Is.EqualTo(evaluatedDecisionResponse.FailureMessage));
        Assert.That("", Is.EqualTo(evaluatedDecisionResponse.FailedDecisionId));
        Assert.That("12", Is.EqualTo(evaluatedDecisionResponse.DecisionRequirementsId));
        Assert.That(1234, Is.EqualTo(evaluatedDecisionResponse.DecisionRequirementsKey));

        var evaluatedDecisions = evaluatedDecisionResponse.EvaluatedDecisions;
        Assert.That(1, Is.EqualTo(evaluatedDecisions.Count));

        var decision = evaluatedDecisions[0];
        Assert.That("decision", Is.EqualTo(decision.DecisionId));
        Assert.That(123, Is.EqualTo(decision.DecisionKey));
        Assert.That("decision-123", Is.EqualTo(decision.DecisionName));
        Assert.That("1", Is.EqualTo(decision.DecisionOutput));
        Assert.That(2, Is.EqualTo(decision.DecisionVersion));
        Assert.That("noop", Is.EqualTo(decision.DecisionType));

        var decisionEvaluatedInputs = decision.EvaluatedInputs;
        Assert.That(2, Is.EqualTo(decisionEvaluatedInputs.Count));

        var decisionEvaluatedInput = decisionEvaluatedInputs[0];
        Assert.That("moep", Is.EqualTo(decisionEvaluatedInput.InputId));
        Assert.That("moepmoep", Is.EqualTo(decisionEvaluatedInput.InputName));
        Assert.That("boom", Is.EqualTo(decisionEvaluatedInput.InputValue));

        decisionEvaluatedInput = decisionEvaluatedInputs[1];
        Assert.That("moeb", Is.EqualTo(decisionEvaluatedInput.InputId));
        Assert.That("moebmoeb", Is.EqualTo(decisionEvaluatedInput.InputName));
        Assert.That("boom", Is.EqualTo(decisionEvaluatedInput.InputValue));

        var decisionMatchedRules = decision.MatchedRules;
        Assert.That(1, Is.EqualTo(decisionMatchedRules.Count));
        var decisionMatchedRule = decisionMatchedRules[0];

        Assert.That("ruleid", Is.EqualTo(decisionMatchedRule.RuleId));
        Assert.That(1, Is.EqualTo(decisionMatchedRule.RuleIndex));

        var evaluatedDecisionOutputs = decisionMatchedRule.EvaluatedOutputs;
        Assert.That(2, Is.EqualTo(evaluatedDecisionOutputs.Count));

        var evaluatedDecisionOutput = evaluatedDecisionOutputs[0];
        Assert.That("outputId", Is.EqualTo(evaluatedDecisionOutput.OutputId));
        Assert.That("output", Is.EqualTo(evaluatedDecisionOutput.OutputName));
        Assert.That("val", Is.EqualTo(evaluatedDecisionOutput.OutputValue));

        evaluatedDecisionOutput = evaluatedDecisionOutputs[1];
        Assert.That("outputId2", Is.EqualTo(evaluatedDecisionOutput.OutputId));
        Assert.That("output2", Is.EqualTo(evaluatedDecisionOutput.OutputName));
        Assert.That("val2", Is.EqualTo(evaluatedDecisionOutput.OutputValue));
    }

    [Test]
    public async Task ShouldSendRequestWithTenantIdAsExpected()
    {
        // given
        var expectedRequest = new EvaluateDecisionRequest
        {
            DecisionId = "decision",
            TenantId = "tenant1"
        };

        // when
        _ = await ZeebeClient.NewEvaluateDecisionCommand()
        .DecisionId("decision")
        .AddTenantId("tenant1")
        .Send();

        // then
        var request = TestService.Requests[typeof(EvaluateDecisionRequest)][0];
        Assert.That(expectedRequest, Is.EqualTo(request));
    }
}