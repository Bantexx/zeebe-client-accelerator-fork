# Zeebe C# Client + Accelerator (форк)

Объединённый форк двух библиотек для работы с [Zeebe](https://camunda.com/platform/zeebe/) из .NET:

- **Zeebe.Client** — gRPC-клиент к брокеру Zeebe (пакет `zb-client`).
- **Zeebe.Client.Accelerator** — расширение поверх того же клиента: автоматический bootstrap job workers через .NET `HostedService` и DI.

Форк предназначен для собственных доработок и единой сборки обоих проектов в одном репозитории.

**Исходные проекты:**

- [camunda-community-hub/zeebe-client-csharp](https://github.com/camunda-community-hub/zeebe-client-csharp)
- [VonDerBeck/zeebe-client-csharp-accelerator](https://github.com/VonDerBeck/zeebe-client-csharp-accelerator)

**Сборка:** откройте `Zeebe.Client.Fork.sln` в IDE или выполните `dotnet build Zeebe.Client.Fork.sln`.

---

## English

This repository is a combined fork of two .NET libraries for Zeebe: the **Zeebe.Client** gRPC client (`zb-client`) and **Zeebe.Client.Accelerator**, which bootstraps job workers via a .NET `HostedService` and dependency injection. Upstream sources are linked above. Build with `Zeebe.Client.Fork.sln` or `dotnet build Zeebe.Client.Fork.sln`.
