using System;
using Xunit;
using static Zeebe.Client.Accelerator.Options.ZeebeClientAcceleratorOptions;

namespace Zeebe.Client.Accelerator.Unit.Tests.Options
{
    public class WorkerOptionsTests
    {
        private readonly int maxJobsActive;
        private readonly byte handlerThreads;
        private readonly long timeout;
        private readonly long pollInterval;
        private readonly long pollingTimeout;
        private readonly long retryTimeout;
        private readonly string name;

        [Fact]
        public void TimeoutTimeSpanMatchesTimeoutInMillisecondsWhenCreated()
        {   
            var actual = Create();
            
            Assert.Equal(timeout, actual.Timeout.TotalMilliseconds);
        }

        [Fact]
        public void PollingTimeoutTimeSpanMatchesPollingTimeoutInMillisecondsWhenCreated()
        {   
            var actual = Create();
            
            Assert.Equal(pollingTimeout, actual.PollingTimeout.TotalMilliseconds);
        }

        [Fact]
        public void PollIntervalTimeSpanMatchesPollIntervalInMillisecondsWhenCreated()
        {   
            var actual = Create();
            
            Assert.Equal(pollInterval, actual.PollInterval.TotalMilliseconds);
        }

        [Fact]
        public void RetryTimeoutTimeSpanMatchesRetryTimeoutInMillisecondsWhenCreated()
        {   
            var actual = Create();
            
            Assert.Equal(retryTimeout, actual.RetryTimeout.TotalMilliseconds);
        }

        [Fact]
        public void StreamEnabledDefaultIsFalseWhenNotSet()
        {
            var actual = new WorkerOptions();
            Assert.False(actual.StreamEnabled);
        }

        [Fact]
        public void StreamEnabledIsStoredWhenSetToTrue()
        {
            var actual = new WorkerOptions { StreamEnabled = true };
            Assert.True(actual.StreamEnabled);
        }

        public WorkerOptionsTests()
        {
            var random = new Random();

            maxJobsActive = random.Next(1, int.MaxValue);
            handlerThreads = Convert.ToByte(random.Next(1, 255));
            timeout = random.Next(1, int.MaxValue);
            pollInterval = random.Next(1, int.MaxValue);
            pollingTimeout = random.Next(1, int.MaxValue);
            retryTimeout = random.Next(1, int.MaxValue);
            name = Guid.NewGuid().ToString();
        }

        private WorkerOptions Create()
        {
            var options = new WorkerOptions
            {
                MaxJobsActive = maxJobsActive,
                HandlerThreads = handlerThreads,
                TimeoutInMilliseconds = timeout,
                PollingTimeoutInMilliseconds = pollingTimeout,
                PollIntervalInMilliseconds = pollInterval,
                RetryTimeoutInMilliseconds = retryTimeout,
                Name = name
            };

            return options;
        }
    }
}