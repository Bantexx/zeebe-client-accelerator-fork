# Bootstrap Accelerator for the C# Zeebe client (fork)

Fork repository: [zeebe-client-csharp-accelerator](https://github.com/camunda-community-hub/zeebe-client-csharp-accelerator)

NuGet package: [zb-client-accelerator-fork](https://www.nuget.org/packages/zb-client-accelerator-fork)

This project extends the C# Zeebe client with automatic worker discovery and bootstrap via a .NET `HostedService`.

## Requirements

- [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) only
- Zeebe 8.x
- Zeebe client from this fork (`Zeebe.Client`)

## How to use

Install package:

```bash
dotnet add package zb-client-accelerator-fork
```

Register accelerator:

```csharp
builder.Services.BootstrapZeebe(
    builder.Configuration.GetSection("ZeebeConfiguration"),
    typeof(Program).Assembly);
```

Minimal config:

```json
{
  "ZeebeConfiguration": {
    "Client": {
      "GatewayAddress": "127.0.0.1:26500"
    }
  }
}
```

For full usage documentation (workers, attributes, SaaS setup, deployment, advanced options), see the upstream README:
[VonDerBeck/zeebe-client-csharp-accelerator README](https://github.com/VonDerBeck/zeebe-client-csharp-accelerator/blob/main/README.md)

## New functionality: StreamEnabled

This fork supports configuring Zeebe worker streaming mode (`StreamEnabled`) both globally and per worker.

### 1) Global setting (all workers by default)

```json
{
  "ZeebeConfiguration": {
    "Worker": {
      "StreamEnabled": true
    }
  }
}
```

### 2) Per-worker override via attribute

```csharp
using Zeebe.Client.Accelerator.Attributes;

[StreamEnabled(true)]
public class MyStreamWorker : IAsyncZeebeWorker
{
    public Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
    {
        // handle job
        return Task.CompletedTask;
    }
}
```

### Resolution order

- If a worker has `[StreamEnabled(...)]`, that value is used for this worker.
- Otherwise, the global `ZeebeConfiguration:Worker:StreamEnabled` value is used.

## Build and test

```bash
dotnet build Zeebe.Client.Accelerator.sln
dotnet test Zeebe.Client.Accelerator.sln
```
