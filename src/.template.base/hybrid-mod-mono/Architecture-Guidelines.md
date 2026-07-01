# Hybrid Modular Monolith With Application-Owned Vertical Slices

## A practical architecture for growing applications

A hybrid modular monolith with application-owned vertical slices combines two useful ideas:

```text
Modular monolith = organize the system around business boundaries.
Vertical slices = organize callable behavior around complete use cases.
```

This version adds one extra rule:

```text
Core owns business concepts.
Application owns callable use cases.
Infrastructure owns technical implementations.
Entrypoints adapt transport input and output.
```

That gives the system clear boundaries without forcing every feature into heavy layers or early microservices.

The examples in this article use a generic online ordering system because most people understand the basic flow: browse items, place an order, pay, and receive a confirmation.

---

## 1. The Core Idea

The architecture separates four concerns:

```text
What is the business concept?
What use case is being performed?
What technical system makes it work?
How does a caller interact with it?
```

Those map to four zones:

```text
Core
  Business modules, contracts, domain models, policies, and rules.

Application
  Callable use cases, request/response DTOs, validation, orchestration, and results.

Infrastructure
  Database, providers, templates, files, queues, external APIs, and technical clients.

Entrypoints
  HTTP APIs, CLI commands, background workers, bot commands, mobile hosts, or desktop hosts.
```

The dependency flow is:

```text
Entrypoint
  -> Application use case
       -> Core contracts and domain rules
       -> Infrastructure implementation, wired by the composition root
```

The simple rule:

```text
Callers use Application.
Core is not an entrypoint API.
Infrastructure is not an entrypoint API.
```

---

## 2. Why This Shape Helps

Traditional layered applications often start like this:

```text
Controllers/
Services/
Repositories/
Models/
DTOs/
Validators/
```

That looks tidy at first, but a single use case can become scattered across many technical folders. To understand one behavior, a developer may need to jump through a controller, service, repository, mapper, validator, DTO, and provider.

This architecture avoids two common problems:

```text
Too little structure:
  Everything can call everything else.

Too much structure:
  Every feature needs layers of ceremony before it can do useful work.
```

The compromise is:

```text
Keep domain code pure.
Keep callable use cases close together.
Keep infrastructure details out of Application.
Keep entrypoints thin.
Enforce dependency direction with tests.
```

---

## 3. Recommended Project Roles

A practical .NET solution can look like this:

```text
src/
  Shop.Core/
  Shop.Application/
  Shop.Infrastructure/
  Shop.Api/
  Shop.Cli/

tests/
  Shop.Core.Tests/
  Shop.Application.Tests/
  Shop.Infrastructure.IntegrationTests/
  Shop.ArchitectureTests/
```

The project roles are:

| Project | Role |
|---|---|
| `Shop.Core` | Business modules, contracts, domain models, rules, and module-owned policies. |
| `Shop.Application` | Callable use cases, cross-module orchestration, request/response DTOs, validation, and result primitives. |
| `Shop.Infrastructure` | EF Core, persistence, provider clients, templates, migrations, files, queues, and external systems. |
| `Shop.Api` | HTTP host and composition root. |
| `Shop.Cli` | Command-line adapter over Application use cases. |

The names can change. The responsibilities should not.

---

## 4. Dependency Direction

The target dependency direction is:

```text
Core
  -> BCL only, or as close to BCL-only as practical

Application
  -> Core

Infrastructure
  -> Core

Entrypoints and composition roots
  -> Application
  -> Infrastructure
```

Application and Infrastructure both depend on Core, but not on each other.

That is intentional.

Application depends on Core because it needs business contracts and domain concepts while orchestrating use cases. Infrastructure depends on Core because it implements Core contracts and maps Core concepts to databases, providers, files, templates, and external APIs.

The reverse direction should be forbidden:

```text
Core must not depend on Application.
Core must not depend on Infrastructure.
Application must not depend on Infrastructure.
Infrastructure must not depend on Application.
Entrypoints must not call Core directly.
```

This prevents the domain from slowly becoming aware of HTTP, EF Core, email providers, background workers, UI code, or hosting concerns.

---

## 5. Core Is Not a Feature Folder

Many vertical-slice examples put feature folders directly under modules:

```text
Modules/
  Orders/
    Features/
      PlaceOrder/
      CancelOrder/
```

That can work, but this version keeps Core focused on business concepts instead of callable actions.

Core modules should look more like this:

```text
Shop.Core/
  Modules/
    ModuleName/
      Contracts/
      Domain/
      Policies/
      Internal/
      Models/
```

Core owns:

- business language,
- module contracts,
- domain models,
- validation that belongs to the business concept,
- module-local policies,
- outcomes that are meaningful inside the module.

Core does not own:

- HTTP endpoints,
- CLI commands,
- background services,
- EF entities,
- provider clients,
- template rendering,
- application result types,
- `UseCase` classes,
- `Features` folders.

The reason is simple: Core should describe the business, not how a user or system asks the application to do something.

---

## 6. Application Owns the Vertical Slices

Callable behavior lives in Application:

```text
Shop.Application/
  UseCases/
    ModuleName/
      VerbFirstAction.cs
      OptionalModuleModels.cs
```

Good names:

```text
PlaceOrder
CancelOrder
TrackDelivery
SearchProducts
GenerateInvoice
```

Avoid suffix-heavy names:

```text
PlaceOrderUseCase
SendReceiptWorkflow
TrackDeliveryService
```

The action name should already say what it does.

Application use cases own their public request and response DTOs. They may map to Core types internally, but they should not leak Core models through public `Execute*` method parameters or return values.

Rule:

```text
The Application boundary exposes Application contracts.
Core contracts are internal implementation details from the caller's point of view.
```

---

## 7. Generic Example: Online Ordering

Imagine a simple online ordering system with these modules:

```text
Customers
Menu
Orders
Payments
Notifications
```

The Core modules might expose contracts and domain rules:

```text
Shop.Core/
  Modules/
    Customers/
      Contracts/
        ICustomerLookup.cs
      Domain/
        CustomerStatus.cs

    Menu/
      Contracts/
        IMenuLookup.cs
      Domain/
        MenuItemAvailability.cs

    Orders/
      Contracts/
        IOrderStore.cs
      Domain/
        Order.cs
        OrderPolicy.cs

    Payments/
      Contracts/
        IPaymentAuthorizer.cs
```

The callable use case lives in Application:

```text
Shop.Application/
  UseCases/
    Orders/
      PlaceOrder.cs
```

The file might contain:

```csharp
namespace Shop.Application.UseCases.Orders;

public sealed record PlaceOrderRequest(
    Guid CustomerId,
    IReadOnlyList<OrderItemRequest> Items,
    string PaymentMethodId);

public sealed record OrderItemRequest(
    Guid MenuItemId,
    int Quantity);

public sealed record PlaceOrderResponse(
    Guid OrderId,
    decimal Total,
    DateTimeOffset EstimatedReadyAt);

public sealed class PlaceOrder(
    ICustomerLookup customers,
    IMenuLookup menu,
    IPaymentAuthorizer payments,
    IOrderStore orders,
    TimeProvider clock)
{
    public async Task<Result<PlaceOrderResponse>> ExecuteAsync(
        PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await customers.FindAsync(request.CustomerId, cancellationToken);
        if (customer is null)
            return Result<PlaceOrderResponse>.NotFound("Customer.NotFound", "Customer was not found.");

        var itemIds = request.Items.Select(x => x.MenuItemId).Distinct().ToArray();
        var menuItems = await menu.FindAvailableItemsAsync(itemIds, cancellationToken);

        var decision = OrderPolicy.CanPlaceOrder(
            customer,
            menuItems,
            request.Items,
            clock.GetUtcNow());
        if (!decision.Allowed)
            return Result<PlaceOrderResponse>.Conflict("Order.NotAllowed", decision.Reason);

        var authorization = await payments.AuthorizeAsync(
            request.PaymentMethodId,
            decision.Total,
            cancellationToken);
        if (!authorization.Approved)
            return Result<PlaceOrderResponse>.Conflict("Payment.Declined", authorization.Message);

        var order = Order.Place(
            customer.Id,
            decision.Items,
            authorization.AuthorizationId,
            decision.EstimatedReadyAt);

        await orders.SaveAsync(order, cancellationToken);

        return Result<PlaceOrderResponse>.Success(
            new PlaceOrderResponse(order.Id, order.Total, order.EstimatedReadyAt));
    }
}
```

The important parts are the boundaries:

```text
PlaceOrder is callable from outside the module.
PlaceOrder uses Core contracts and domain policy.
PlaceOrder returns Application-owned DTOs.
PlaceOrder does not expose database entities.
PlaceOrder does not know whether the caller is HTTP, CLI, mobile, or a background job.
```

---

## 8. Cross-Module Orchestration

Some use cases naturally coordinate more than one module.

Cross-module orchestration belongs in the Application use case owned by the initiating module. Do not create a generic `Workflows` folder just because a use case touches several concepts.

Generic example:

```text
UseCases/
  Orders/
    PlaceOrderAndSendReceipt.cs
```

That action belongs under `Orders` because the user intent is to place an order. It may call:

```text
Customers contract
Menu contract
Orders contract
Payments contract
Notifications contract
```

But it should not become:

```text
UseCases/
  Workflows/
    PlaceOrderAndSendReceiptWorkflow.cs
```

The module that owns the user intent should own the Application action unless there is an explicit architecture decision that no module can honestly own it.

---

## 9. Infrastructure Owns Technical Reality

Infrastructure implements Core contracts and owns technical details.

This includes things like:

- database persistence,
- EF Core entities and mappings,
- migrations,
- database stores,
- email or SMS providers,
- template rendering,
- provider-specific options,
- embedded templates,
- external HTTP clients,
- file storage,
- queues or outbox processors.

Infrastructure may map between Core domain models and database entities. It may choose table shape, indexes, row versions, provider payload storage, JSON columns, and EF navigation properties.

Core should not be forced to look like the database.

Generic example:

```text
Core concept:
  Order
  OrderTotal
  OrderStatus

Infrastructure table shape:
  order_records
  order_items
  payment_authorization_id
  status_text
  provider_correlation_id
  row_version
```

Those are not the same thing, and that is fine.

Rule:

```text
Core names the business concept.
Infrastructure chooses the persistence shape.
Application decides which use case is exposed.
Entrypoints decide how transport input and output are represented.
```

---

## 10. Entrypoints Stay Thin

An entrypoint adapts a transport to Application.

For HTTP, that means:

```text
Read route/body/header data.
Create an Application request.
Call an Application use case.
Convert the Application result to an HTTP response.
```

Generic example:

```csharp
group.MapPost("/orders", async (
        PlaceOrderRequest request,
        PlaceOrder placeOrder,
        CancellationToken cancellationToken) =>
    {
        var result = await placeOrder.ExecuteAsync(request, cancellationToken);
        return result.ToHttpResult();
    });
```

The endpoint does not:

- enforce domain rules itself,
- query EF directly,
- call provider clients directly,
- construct Core entities as a public contract,
- duplicate Application validation.

The same idea applies to a CLI command, bot command, mobile host, desktop UI, or worker:

```text
Transport in.
Application request.
Application result.
Transport out.
```

---

## 11. Data Access and Ownership

A modular monolith is not a set of microservices. The practical default is:

```text
One application database.
Infrastructure-owned database context.
Logical table ownership by module.
Application use cases call Core contracts.
Infrastructure implements those contracts.
```

This avoids pretending every module is already independently deployed.

The ownership rule is:

```text
Only the owning module writes to its logical data.
Other modules read through contracts, read models, or Application orchestration.
```

Generic example:

| Module | Owned Data |
|---|---|
| Customers | customer profiles, saved addresses, account state |
| Menu | products, prices, item availability |
| Orders | order records, order items, fulfillment status |
| Payments | payment attempts, authorization state |
| Notifications | notification attempts, delivery state |

If the Orders use case needs a product name or price, it should not mutate Menu tables. It can read through a Menu contract or use a read model designed for that purpose.

---

## 12. Reliable Side Effects

Side effects should not make the primary user action unreliable when they can be handled after durable intake.

Generic example:

```text
Place order.
Save the order.
Save a receipt outbox job.
Return success.
Background worker sends the receipt later.
```

This avoids a fragile flow:

```text
Place order.
Save the order.
Try to send the receipt immediately.
Email provider fails.
The user action appears to fail even though the order was saved.
```

The broader rule:

```text
If work must not be lost, persist it before doing external side effects.
```

---

## 13. Validation and Results

Use two levels of validation:

```text
Request validation = input shape and basic correctness.
Business validation = rules based on current domain state.
```

Request validation examples:

- required fields,
- maximum lengths,
- valid email format,
- positive quantities,
- valid enum values.

Business validation examples:

- a customer cannot place an order with an inactive account,
- an item cannot be ordered when unavailable,
- payment must be authorized before the order is confirmed,
- an order cannot be cancelled after delivery starts.

Application owns result primitives because entrypoints need a stable way to translate outcomes into transport responses.

Generic result statuses might map like this:

| Result Status | HTTP Meaning |
|---|---|
| Success | `200 OK` |
| Accepted | `202 Accepted` |
| Created | `201 Created` |
| Validation | `400 Bad Request` |
| NotFound | `404 Not Found` |
| Conflict | `409 Conflict` |

Core should not own these transport-facing result types. Core can return module-specific decisions or outcomes. Application translates those into caller-facing results.

---

## 14. Configuration and Options

Configuration belongs to the layer that needs it.

Strongly typed options make configuration discoverable and testable. Option classes should expose a clear section name and defaults where defaults are safe.

Generic example:

```csharp
public sealed class EmailDeliveryOptions
{
    public const string SectionName = "Notifications:Email";

    public string Provider { get; init; } = "Smtp";
    public int TimeoutSeconds { get; init; } = 30;
}
```

The caller should not need to guess whether the real setting is `EMAIL_TIMEOUT`, `Email:Timeout`, `Mail:TimeoutSeconds`, or something else. The code should define the contract.

Rule:

```text
Configuration is a contract.
Make it typed, discoverable, and owned by the layer that consumes it.
```

---

## 15. Architecture Tests

Architecture rules should be executable.

Useful tests include:

- Core must not reference Application, Infrastructure, or entrypoints.
- Core must not reference framework or provider packages unless explicitly allowed.
- Core must not expose a global `Shared` namespace by default.
- Core must not contain `Features` namespaces.
- Application must not reference Infrastructure or entrypoints.
- Application `Execute*` contracts must not expose Core DTOs.
- Application callable types must avoid `UseCase` and `Workflow` suffixes.
- Infrastructure must not reference Application or entrypoints.
- Entrypoints must not reference Core directly.

This matters because architecture written only in Markdown slowly becomes a suggestion. Architecture tested in CI becomes a working constraint.

---

## 16. Adding a New Feature

When adding a new backend feature, use this checklist:

```text
1. Identify the owning business module.
2. Put business contracts and domain concepts in Core.
3. Put the callable action in Application/UseCases/<Module>.
4. Use a verb-first, suffix-less action name.
5. Keep request and response DTOs Application-owned.
6. Keep cross-module orchestration in the module that owns the user intent.
7. Implement Core contracts in Infrastructure.
8. Keep EF entities and provider details out of Core.
9. Keep endpoints, commands, and workers thin.
10. Register services through the appropriate composition root.
11. Add focused tests for behavior.
12. Add or update architecture tests if the rule surface changes.
13. Update docs when the module boundary or dependency rule changes.
```

Do not start by creating a controller, repository, service, mapper, and DTO folder. Start by identifying the use case and the module that owns it.

---

## 17. Anti-Patterns to Avoid

### 17.1 Core as a Dumping Ground

Core should not become the place where every shared type goes.

Bad:

```text
Shop.Core/
  Shared/
  Helpers/
  Features/
  Http/
  Persistence/
```

Better:

```text
Core module owns business concepts.
Application owns caller-facing primitives.
Infrastructure owns technical implementations.
```

### 17.2 Generic Services for Every Module

Avoid broad service classes with unrelated methods.

Bad:

```text
OrderService
  PlaceOrder
  CancelOrder
  RefundOrder
  TrackDelivery
  SendReceipt
  ExportOrders
```

Better:

```text
PlaceOrder
CancelOrder
RefundOrder
TrackDelivery
SendReceipt
ExportOrders
```

Each callable action should be easy to read without understanding an entire service class.

### 17.3 Infrastructure Leaking Into Application

Application should not know that a store uses PostgreSQL, EF Core, SMTP, a payment provider SDK, a template engine, or a file system.

Bad:

```text
Application use case creates DbContext.
Application use case creates provider HTTP client.
Application request exposes EF entity.
```

Better:

```text
Application depends on Core contracts.
Infrastructure implements those contracts.
Entrypoint wires both together.
```

### 17.4 Entrypoints Calling Core Directly

Calling Core directly from API, CLI, bot, UI, or worker code bypasses the Application facade.

That creates duplicated orchestration and makes every adapter invent its own behavior.

The rule is:

```text
Entrypoints call Application.
Application coordinates Core contracts.
Infrastructure does the technical work.
```

### 17.5 Premature Workflow Folders

Do not create a generic `Workflows` namespace just because a use case coordinates multiple modules.

Prefer:

```text
UseCases/
  Orders/
    PlaceOrderAndSendReceipt.cs
```

Avoid:

```text
UseCases/
  Workflows/
    PlaceOrderAndSendReceiptWorkflow.cs
```

Use the module that owns the user intent.

---

## 18. How This Can Evolve

The architecture can grow in stages.

### Stage 1: Focused Modular Monolith

```text
One backend.
One database.
Core modules.
Application use cases.
Infrastructure implementations.
Thin entrypoints.
```

### Stage 2: More Entrypoints

```text
API calls Application.
CLI calls Application.
Bot calls Application.
Mobile host calls Application.
Background workers call Application.
```

The use-case surface stays stable even as adapters are added.

### Stage 3: Stronger Module Contracts

```text
Clear module contracts.
Architecture tests.
Outbox for reliable side effects.
Explicit table ownership.
Focused integration tests.
```

### Stage 4: Service Extraction Only If Needed

A module should be extracted only when the reason is real:

- independent scaling is required,
- independent deployment is worth the cost,
- the module boundary is stable,
- the data ownership is clean,
- the team has operational maturity for distributed systems.

Until then, the modular monolith gives most of the benefit with far less runtime complexity.

---

## 19. Decision Matrix

| Criterion | Layered App | Clean Architecture | Modular Monolith With Application Slices | Microservices |
|---|---:|---:|---:|---:|
| Startup simplicity | High | Medium | Medium | Low |
| Feature discoverability | Medium | Medium | High | Medium |
| Business boundary clarity | Low | Medium | High | High |
| Deployment simplicity | High | High | High | Low |
| Operational complexity | Low | Low | Low | High |
| Works for small teams | High | Medium | High | Low |
| Protects against tangling | Low | Medium | High if enforced | High if designed well |
| Ceremony risk | Low | High | Medium | High |
| Best default for growing product | Medium | Medium | High | Low early, high later |

---

## 20. Final Position

Hybrid modular monolith with application-owned vertical slices is a strong default for many growing applications.

It gives developers:

- one deployable system,
- clear business modules,
- callable use cases that are easy to find,
- a pure business core,
- infrastructure isolated from callers,
- thin entrypoints,
- enforceable dependency rules,
- a future path toward service extraction if a boundary proves it deserves one.

The goal is not architectural ceremony. The goal is to make the next feature easier to place, easier to test, and easier to expose through whichever entrypoint needs it.
