using Microsoft.OpenApi;
using System.Reflection;
using Zeebe.Client.Accelerator.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Bootstrap Zeebe Integration
builder.Services.BootstrapZeebe(
    builder.Configuration.GetSection("ZeebeConfiguration"),
    typeof(Program).Assembly);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Camunda 8 Showcase API",
        Description = "An ASP.NET Core Web API for starting Camunda 8 process instances",
    });

    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Deploy process resources (await: Deploy() was async void and could crash the process on gRPC errors)
try
{
    await app.CreateZeebeDeployment()
        .UsingDirectory("Resources")
        .AddResource("process.bpmn")
        .AddResource("ApproveUser.form")
        .DeployAsync();
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex,
        "Zeebe resource deployment failed. Ensure the broker is reachable (see ZeebeConfiguration) and restart.");
}

app.Run();

public partial class Program { }