using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Integration.Tests.Handlers
{
    [StreamEnabled(true)]
    public class StreamEnabledJobHandler : IAsyncZeebeWorker
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public StreamEnabledJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            handleJobDelegate(job, cancellationToken);
            return Task.CompletedTask;
        }
    }
}
