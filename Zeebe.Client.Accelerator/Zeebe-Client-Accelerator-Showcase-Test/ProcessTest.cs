using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Zeebe.Client;
using Zeebe_Client_Accelerator_Showcase.Controllers;
using Zeebe_Client_Accelerator_Showcase_Test.testcontainers;

//[assembly: CaptureConsole]
namespace Zeebe_Client_Accelerator_Showcase_Test
{
    public class ProcessTest : IClassFixture<IntegrationTestFactory<Program>>
    {

        private readonly IntegrationTestFactory<Program> _factory;
        private readonly BpmAssert _bpmAssert;
        private readonly IZeebeClient _zeebeClient;
        private readonly HttpClient _zeebeHttpClient;

        public ProcessTest(IntegrationTestFactory<Program> factory, ITestOutputHelper outputHelper)
        {
            factory.OutputHelper = outputHelper;
            _factory = factory;
            _bpmAssert = factory.Services.GetRequiredService<BpmAssert>();
            _zeebeClient = factory.Services.GetRequiredService<IZeebeClient>();
            _zeebeHttpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:8080"),
            };
            _zeebeHttpClient.DefaultRequestHeaders.Accept.Clear();
            _zeebeHttpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [Fact]
        public async Task TestHappyPathAsync()
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            // Given
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            var request = new ApplicationRequest()
            {
                ApplicantName = "John Doe"
            };

            // When
            var response = await client.PostAsync("/application", ToJsonContent(request), cancellationToken);

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var applicationResponse = await response.Content.ReadFromJsonAsync<ApplicationResponse>(cancellationToken);
            Assert.NotNull(applicationResponse);
            var processInstanceKey = applicationResponse.ProcessInstanceKey;
            _bpmAssert.WaitUntilProcessInstanceHasStarted(processInstanceKey);

            // wait for user task
            _bpmAssert.WaitUntilProcessInstanceHasReachedElement(processInstanceKey, "Task_AppoveUser");

            // complete the user task
            await FindAndCompleteUserTaskAsync(processInstanceKey, "Task_AppoveUser", new
            {
                approved = true,
            }, cancellationToken);

            // await user account creation and end of process
            _bpmAssert.WaitUntilProcessInstanceHasCompletedElement(processInstanceKey, "Activity_CreateUserAccount");
            _bpmAssert.WaitUntilProcessInstanceHasEnded(processInstanceKey);
            _bpmAssert.AssertThatProcessInstanceHasCompletedElement(processInstanceKey, "EndEvent_ApplicationApproved");
        }

        private async Task FindAndCompleteUserTaskAsync(long processInstanceKey, string taskName, object payload, CancellationToken cancellationToken)
        {
            var userTask = _bpmAssert.AssertThatUserTaskExistsAndReturnValue(processInstanceKey, taskName);
            var completePayload = new
            {
                variables = payload
            };
            var userTasksResponse = await _zeebeHttpClient.PostAsync($"/v2/user-tasks/{userTask.UserTaskKey}/completion", ToJsonContent(completePayload), cancellationToken);
            Assert.Equal(HttpStatusCode.NoContent, userTasksResponse.StatusCode);

        }

        private StringContent ToJsonContent(object? request)
        {
            var json = JsonConvert.SerializeObject(request);
            var completeContent = new StringContent(json, Encoding.UTF8, "application/json");
            return completeContent;
        }
    }
}