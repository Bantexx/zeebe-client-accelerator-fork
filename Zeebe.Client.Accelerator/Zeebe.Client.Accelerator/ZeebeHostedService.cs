using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Api.Worker;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Accelerator.Options;
using Microsoft.Extensions.Options;
using static Zeebe.Client.Accelerator.Options.ZeebeClientAcceleratorOptions;
using Microsoft.Extensions.DependencyInjection;
using Zeebe.Client.Api.Responses;
using System.Linq;

namespace Zeebe.Client.Accelerator
{
    public class ZeebeHostedService : IHostedService, IDisposable
    {
        private CancellationTokenSource cancellationTokenSource;
        private readonly IServiceScope serviceScope;
        private readonly IZeebeClient zeebeClient;
        private readonly IJobHandlerInfoProvider jobHandlerInfoProvider;
        private readonly WorkerOptions zeebeWorkerOptions;
        private readonly ILogger<ZeebeHostedService> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly List<IJobWorker> workers = [];

        public ZeebeHostedService(IServiceScopeFactory serviceScopeFactory, IJobHandlerInfoProvider jobHandlerInfoProvider, IOptions<ZeebeClientAcceleratorOptions> options, ILogger<ZeebeHostedService> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.jobHandlerInfoProvider = jobHandlerInfoProvider ?? throw new ArgumentNullException(nameof(jobHandlerInfoProvider));
            this.zeebeWorkerOptions = options?.Value?.Worker ?? throw new ArgumentNullException(nameof(options), $"{nameof(IOptions<ZeebeClientAcceleratorOptions>)}.Value.{nameof(ZeebeClientAcceleratorOptions.Worker)} is null.");
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceScope = this.serviceScopeFactory.CreateScope();
            this.zeebeClient = this.serviceScope.ServiceProvider.GetRequiredService<IZeebeClient>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            foreach (var jobHandlerInfo in jobHandlerInfoProvider.JobHandlerInfoCollection)
            {
                var pollingTimeout = jobHandlerInfo.PollingTimeout ?? zeebeWorkerOptions.PollingTimeout;
                var streamTimeout = zeebeWorkerOptions.StreamTimeout.TotalMilliseconds > 0
                    ? zeebeWorkerOptions.StreamTimeout
                    : pollingTimeout;

                var worker = zeebeClient.NewWorker()
                    .JobType(jobHandlerInfo.JobType)
                    .Handler((jobClient, job) => HandleJob(jobClient, job, cancellationTokenSource.Token))
                    .FetchVariables(jobHandlerInfo.FetchVariabeles)
                    .MaxJobsActive(jobHandlerInfo.MaxJobsActive ?? zeebeWorkerOptions.MaxJobsActive)
                    .TenantIds(jobHandlerInfo.TenantIds.Length > 0 ? jobHandlerInfo.TenantIds : zeebeWorkerOptions.TenantIds)
                    .Name(zeebeWorkerOptions.Name ?? jobHandlerInfo.WorkerName)
                    .PollingTimeout(pollingTimeout)
                    .StreamTimeout(streamTimeout)
                    .PollInterval(jobHandlerInfo.PollInterval ?? zeebeWorkerOptions.PollInterval)
                    .Timeout(jobHandlerInfo.Timeout ?? zeebeWorkerOptions.Timeout)
                    .HandlerThreads(jobHandlerInfo.HandlerThreads ?? zeebeWorkerOptions.HandlerThreads)
                    .StreamEnabled(jobHandlerInfo.StreamEnabled ?? zeebeWorkerOptions.StreamEnabled)
                    .Open();

                if (jobHandlerInfo.FetchVariabeles.Length > 0)
                {
                    if (jobHandlerInfo.FetchVariabeles.Length == 1 && "".Equals(jobHandlerInfo.FetchVariabeles.First()))
                    {
                        logger.LogInformation($"Created job worker for type '{jobHandlerInfo.JobType}' fetching no variables.");
                    } 
                    else
                    {
                        logger.LogInformation($"Created job worker for type '{jobHandlerInfo.JobType}' with variables {string.Join(",", jobHandlerInfo.FetchVariabeles)}.");
                    }
                }
                else 
                {
                    logger.LogInformation($"Created job worker for type '{jobHandlerInfo.JobType}' fetching all variables.");
                }

                workers.Add(worker);
            }

            logger.LogInformation($"Created {workers.Count} job workers.");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                cancellationTokenSource.Cancel();
            }
            finally
            {
                StopInternal();
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            StopInternal();
            serviceScope.Dispose();
        }

        private void StopInternal()
        {
            workers.ToList().ForEach(w => w.Dispose());
            workers.Clear();
        }

        private async Task HandleJob(IJobClient jobClient, IJob job, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var scope = serviceScopeFactory.CreateScope();
            var bootstrapJobHandler = scope.ServiceProvider.GetRequiredService<IBootstrapJobHandler>();
            await bootstrapJobHandler.HandleJob(jobClient, job, cancellationToken);
        }
    }
}
