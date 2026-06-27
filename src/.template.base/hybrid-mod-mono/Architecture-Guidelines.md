# Hybrid Modular Monolith + Vertical Slice Architecture

## A Pragmatic Architecture for Modern Application Development

A hybrid modular monolith with vertical slices is an architecture style that combines two useful ideas:

```text
Modular monolith = organize the system around business boundaries.
Vertical slice architecture = organize each feature around a complete use case.
```

The result is a single deployable application that is internally divided into clear modules, while each module is implemented as feature-oriented slices instead of broad technical layers.

This architecture is not about theoretical purity. It is about building software that can grow without becoming tangled, while avoiding the operational cost of premature microservices.

---

## 1. The Core Idea

Most applications start simple. Then they grow. The danger is not that the first version is too small. The danger is that the structure does not give the codebase a way to grow safely.

A hybrid modular monolith + vertical slice approach creates two levels of organization:

```text
Application
  └── Modules
        └── Features
              ├── Endpoint / Controller / Message Handler
              ├── Request
              ├── Response
              ├── Validator
              ├── Handler
              └── Supporting feature-specific code
```

The module answers:

```text
Which business area owns this behavior?
```

The vertical slice answers:

```text
Which use case does this code support?
```

The architecture is called hybrid because it does not follow a single strict school of architecture. It borrows the useful parts of modular monoliths, vertical slices, domain-driven design, clean architecture, and pragmatic data access, while avoiding unnecessary ceremony.

---

## 2. What Problem This Solves

Traditional layered applications usually start like this:

```text
Controllers/
Services/
Repositories/
Entities/
DTOs/
Validators/
```

This looks clean at first, but over time it often becomes difficult to change. A single feature is scattered across many folders. To understand one use case, a developer may need to jump through controller, service, repository, mapper, validator, DTO, and configuration files.

The structure is organized by technical type, not by business behavior.

A feature-oriented structure instead groups the code needed for one use case:

```text
Features/
  AddItemToCart/
    AddItemToCartEndpoint.cs
    AddItemToCartRequest.cs
    AddItemToCartResponse.cs
    AddItemToCartValidator.cs
    AddItemToCartHandler.cs
```

This makes the system easier to navigate because the unit of change is usually a feature, not a technical layer.

The modular monolith part solves a different problem: feature folders alone are not enough for a large system. Without module boundaries, feature folders can still become a flat pile of unrelated use cases.

Modules give the system business boundaries:

```text
Modules/
  Identity/
  Billing/
  Catalog/
  Orders/
  Notifications/
  Reporting/
```

Vertical slices inside modules give each boundary an implementation style that stays focused on use cases.

---

## 3. The Recommended Structure

A practical backend structure can look like this:

```text
src/
  App.Api/
    Program.cs

    Modules/
      Identity/
        IdentityModule.cs
        Domain/
        Contracts/
        Features/
          RegisterUser/
          Login/
          GetCurrentUser/

      Catalog/
        CatalogModule.cs
        Domain/
        Contracts/
        Features/
          CreateProduct/
          SearchProducts/
          GetProductDetails/

      Orders/
        OrdersModule.cs
        Domain/
        Contracts/
        Features/
          PlaceOrder/
          CancelOrder/
          GetOrderHistory/

      Notifications/
        NotificationsModule.cs
        Domain/
        Contracts/
        Features/
          SendNotification/
          MarkNotificationRead/

    Shared/
      Auth/
      Errors/
      Results/
      Validation/
      Pagination/
      Time/

    Infrastructure/
      Persistence/
      Caching/
      BackgroundJobs/
      Email/
      Files/
      Observability/

tests/
  App.UnitTests/
  App.IntegrationTests/
  App.ArchitectureTests/
```

This structure has three important zones:

| Zone | Purpose |
|---|---|
| `Modules` | Business boundaries and feature slices. |
| `Shared` | Small cross-cutting abstractions and utilities used by many modules. |
| `Infrastructure` | Technical implementation details such as persistence, caching, jobs, email, file storage, and observability. |

The rule is simple:

```text
Business code lives in modules.
Reusable cross-cutting concepts live in Shared.
Technical implementation lives in Infrastructure.
```

---

## 4. Module Design

A module is a business boundary, not just a folder. It owns a related set of features, domain rules, and data.

A module should usually contain:

```text
ModuleName/
  ModuleNameModule.cs
  Domain/
  Contracts/
  Features/
```

Optional folders can be added only when needed:

```text
ModuleName/
  Events/
  Policies/
  ReadModels/
  Jobs/
  Mappings/
```

Do not create folders just because an architecture diagram says they exist. Create them when the module actually needs them.

### 4.1 Module Responsibilities

A module owns:

- its business rules,
- its write operations,
- its domain entities,
- its feature handlers,
- its module-specific contracts,
- its route registration or message registration,
- its logical data ownership.

A module should not expose its internal implementation to other modules.

### 4.2 Public Module Contracts

When another module needs to interact with a module, it should depend on a contract, not internal domain or persistence code.

Example:

```text
Modules/
  Catalog/
    Contracts/
      ICatalogLookup.cs
      ProductSummary.cs
    Domain/
      Product.cs
    Features/
      SearchProducts/
```

Other modules may depend on:

```text
Catalog.Contracts
```

Other modules must not depend on:

```text
Catalog.Domain.Product
Catalog.Features.SearchProducts.SearchProductsHandler
Catalog.Persistence.ProductConfiguration
```

This keeps the module boundary clean.

### 4.3 Dependency Rule

A pragmatic rule:

```text
A module may depend on another module's Contracts folder.
A module must not depend on another module's Domain, Features, or Persistence internals.
```

This is much easier to enforce than a strict distributed system boundary while still preventing the worst coupling.

---

## 5. Vertical Slice Design

A vertical slice is a complete use case packaged together.

Example:

```text
Modules/
  Orders/
    Features/
      PlaceOrder/
        PlaceOrderEndpoint.cs
        PlaceOrderRequest.cs
        PlaceOrderResponse.cs
        PlaceOrderValidator.cs
        PlaceOrderHandler.cs
```

A slice should contain code that changes together.

The endpoint handles HTTP concerns. The validator handles request validation. The handler orchestrates the use case. The response defines the output shape.

### 5.1 What Belongs in a Slice

A slice may contain:

- endpoint mapping,
- request model,
- response model,
- validator,
- handler,
- feature-specific mapping,
- feature-specific query projection,
- feature-specific authorization logic,
- feature-specific tests.

### 5.2 What Does Not Belong in a Slice

A slice should not contain unrelated shared business rules. If multiple slices need the same real business rule, move it into the module domain or a module service.

A slice should not become a dumping ground. The purpose is local cohesion, not copying the same logic everywhere.

### 5.3 Handler Responsibility

A handler should orchestrate one use case:

```text
Validate business conditions.
Load required data.
Apply business rules.
Persist changes.
Return a result.
```

It should not become a generic service with many unrelated methods.

Bad:

```csharp
public sealed class OrderService
{
    public Task PlaceOrder(...);
    public Task CancelOrder(...);
    public Task RefundOrder(...);
    public Task UpdateShippingAddress(...);
    public Task GetOrderHistory(...);
}
```

Better:

```csharp
public sealed class PlaceOrderHandler
{
    public Task<Result<PlaceOrderResponse>> HandleAsync(
        PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        // Use case logic
    }
}
```

A handler should be boring, direct, and easy to read.

---

## 6. Minimal API Example in .NET

The pattern works with controllers, message handlers, CLI commands, or background jobs. In ASP.NET Core, Minimal APIs fit this architecture well because route mapping can live directly beside the feature.

Example feature folder:

```text
Modules/
  Orders/
    Features/
      PlaceOrder/
        PlaceOrderEndpoint.cs
        PlaceOrderRequest.cs
        PlaceOrderResponse.cs
        PlaceOrderValidator.cs
        PlaceOrderHandler.cs
```

Endpoint:

```csharp
namespace App.Api.Modules.Orders.Features.PlaceOrder;

public static class PlaceOrderEndpoint
{
    public static RouteGroupBuilder MapPlaceOrderEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync)
            .WithName("PlaceOrder")
            .Produces<PlaceOrderResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }

    private static async Task<IResult> HandleAsync(
        PlaceOrderRequest request,
        PlaceOrderHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return result.ToHttpResult();
    }
}
```

Request:

```csharp
namespace App.Api.Modules.Orders.Features.PlaceOrder;

public sealed record PlaceOrderRequest(
    Guid CustomerId,
    IReadOnlyList<PlaceOrderItemRequest> Items);

public sealed record PlaceOrderItemRequest(
    Guid ProductId,
    int Quantity);
```

Response:

```csharp
namespace App.Api.Modules.Orders.Features.PlaceOrder;

public sealed record PlaceOrderResponse(
    Guid OrderId,
    string Status,
    decimal Total);
```

Validator:

```csharp
namespace App.Api.Modules.Orders.Features.PlaceOrder;

public sealed class PlaceOrderValidator : AbstractValidator<PlaceOrderRequest>
{
    public PlaceOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}
```

Handler:

```csharp
namespace App.Api.Modules.Orders.Features.PlaceOrder;

public sealed class PlaceOrderHandler
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;

    public PlaceOrderHandler(AppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<PlaceOrderResponse>> HandleAsync(
        PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var customerExists = await _db.Customers
            .AnyAsync(x => x.Id == request.CustomerId, cancellationToken);

        if (!customerExists)
        {
            return Result.NotFound<PlaceOrderResponse>("Customer was not found.");
        }

        var productIds = request.Items.Select(x => x.ProductId).Distinct().ToList();

        var products = await _db.Products
            .Where(x => productIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            return Result.Validation<PlaceOrderResponse>("One or more products were not found.");
        }

        var order = Order.Place(
            request.CustomerId,
            request.Items.Select(x => new OrderItemInput(x.ProductId, x.Quantity)),
            products,
            _clock.UtcNow);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Created(new PlaceOrderResponse(
            order.Id,
            order.Status.ToString(),
            order.Total));
    }
}
```

This is intentionally direct. There is no automatic repository abstraction. There is no generic service layer. The handler uses the database context directly because the use case is clearer that way.

---

## 7. Module Registration

Each module should register its own services and endpoints.

```csharp
namespace App.Api.Modules.Orders;

public static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services)
    {
        services.AddScoped<PlaceOrderHandler>();
        services.AddScoped<CancelOrderHandler>();
        services.AddScoped<GetOrderHistoryHandler>();
        services.AddScoped<IOrdersLookup, OrdersLookup>();

        return services;
    }

    public static IEndpointRouteBuilder MapOrdersModule(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapPlaceOrderEndpoint();
        group.MapCancelOrderEndpoint();
        group.MapGetOrderHistoryEndpoint();

        return app;
    }
}
```

Program startup stays clean:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCatalogModule();
builder.Services.AddOrdersModule();
builder.Services.AddNotificationsModule();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityModule();
app.MapCatalogModule();
app.MapOrdersModule();
app.MapNotificationsModule();

app.Run();
```

The application is still one deployable unit, but the internal structure makes the system easier to understand and enforce.

---

## 8. Data Access Strategy

A pragmatic modular monolith usually starts with:

```text
One application database.
One application DbContext.
Logical table ownership by module.
Direct EF Core usage in feature handlers.
```

This is intentionally different from pretending every module is already a microservice.

### 8.1 Logical Ownership

Even with one database, each module should own certain tables.

Example:

| Module | Owned Tables |
|---|---|
| Identity | users, roles, user_profiles |
| Catalog | products, categories, product_images |
| Orders | orders, order_items, order_status_history |
| Payments | payments, payment_attempts |
| Notifications | notifications, notification_preferences |

The ownership rule:

```text
Only the owning module writes to its tables.
Other modules may read through contracts, projections, or explicit read models.
```

This prevents every module from becoming a free-for-all database script.

### 8.2 Direct DbContext Usage

Direct DbContext usage is often the simplest correct choice.

Good:

```csharp
var order = await _db.Orders
    .Include(x => x.Items)
    .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
```

Often unnecessary:

```csharp
var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId);
```

A repository can be useful when it contains meaningful domain-specific behavior or hides a genuinely complex data source. A generic repository over EF Core usually adds indirection without adding value.

### 8.3 When to Add a Repository

Add a repository only when at least one of these is true:

- the aggregate has complex persistence rules,
- the data source is not EF Core,
- the module must hide persistence from rich domain logic,
- queries are reused heavily and have meaningful names,
- persistence details are volatile,
- the repository protects important invariants.

Do not add a repository just to satisfy an architecture diagram.

### 8.4 Read Composition

Read screens often need data from multiple modules. For performance and simplicity, allow read-only composition queries.

Example:

```text
Order history screen needs:
- order status from Orders,
- product names from Catalog,
- payment status from Payments.
```

A read-only query may join across tables if it does not write into another module's owned tables.

Rule:

```text
Writes must respect module ownership.
Reads may compose data pragmatically.
```

This is one of the most important pragmatic compromises in this architecture.

---

## 9. Transactions and Consistency

Because the application is a monolith with one database, most use cases can use a normal database transaction.

For a single module write:

```text
Handler changes entities.
Handler calls SaveChangesAsync.
Database transaction commits.
```

For multi-module workflows, avoid directly modifying another module's tables. Prefer one of these:

1. Call a contract exposed by the owning module.
2. Publish an in-process domain event.
3. Use an outbox if reliability matters.
4. Use a background job for delayed work.

### 9.1 In-Process Events

In-process events are useful when one module needs to react after something happens.

Example:

```text
Orders module publishes OrderPlaced.
Notifications module sends confirmation notification.
Reporting module updates projections.
```

The event should represent something that already happened:

```csharp
public sealed record OrderPlaced(
    Guid OrderId,
    Guid CustomerId,
    DateTimeOffset PlacedAt);
```

Do not use events as a way to hide synchronous command calls when the workflow must succeed immediately.

### 9.2 Outbox Pattern

Use an outbox when work must not be lost after the main transaction commits.

Example:

```text
Place order.
Save order and outbox message in same transaction.
Background worker later sends email or publishes external event.
```

This avoids the common failure mode:

```text
Database save succeeds.
Email send fails.
Application has no durable record to retry email.
```

An outbox is not required on day one for every project. Add it when reliability matters.

---

## 10. Validation Strategy

Use two levels of validation:

```text
Request validation = shape and basic input correctness.
Business validation = domain rules and current state checks.
```

Request validation examples:

- required fields,
- max length,
- valid email format,
- positive quantity,
- valid enum value.

Business validation examples:

- cannot cancel an order that already shipped,
- cannot add an inactive product to an order,
- cannot refund more than the captured amount,
- cannot invite a user who is already a member.

Request validation can live in the feature validator.

Business validation should live in the handler, domain entity, domain service, or module policy, depending on complexity.

---

## 11. Error Handling

A shared result type can keep handlers readable and endpoint responses consistent.

Example:

```csharp
public sealed record Error(string Code, string Message);

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }
    public ResultStatus Status { get; }
}
```

Endpoint conversion:

```csharp
public static IResult ToHttpResult<T>(this Result<T> result)
{
    return result.Status switch
    {
        ResultStatus.Ok => Results.Ok(result.Value),
        ResultStatus.Created => Results.Created(string.Empty, result.Value),
        ResultStatus.NotFound => Results.NotFound(ToProblem(result)),
        ResultStatus.Validation => Results.BadRequest(ToProblem(result)),
        ResultStatus.Conflict => Results.Conflict(ToProblem(result)),
        _ => Results.Problem("Unexpected error")
    };
}
```

This prevents every endpoint from manually translating errors in a different way.

---

## 12. Authorization Strategy

Authorization should be explicit at the route or feature level.

Examples:

```csharp
group.MapPost("/", HandleAsync)
    .RequireAuthorization("CanPlaceOrders");
```

For ownership checks, route authorization is not enough. The handler must verify the current user can act on the specific resource.

Example:

```csharp
if (order.CustomerId != currentUser.UserId)
{
    return Result.Forbidden<OrderResponse>("You cannot access this order.");
}
```

Use policies for broad access rules. Use handler checks for resource-specific ownership rules.

---

## 13. Shared Kernel vs Shared Utilities

Shared code is useful, but it can become a dumping ground.

Keep `Shared` small.

Good shared concepts:

- `Result<T>`,
- error models,
- pagination models,
- clock abstraction,
- current user abstraction,
- validation behavior,
- common guard helpers,
- base value objects only when truly universal.

Bad shared concepts:

- generic business services,
- vague helpers,
- cross-module domain entities,
- shared repositories,
- a universal `CommonService`,
- god-level mapping utilities.

Rule:

```text
If the code has business meaning, it probably belongs to a module.
If the code has technical or cross-cutting meaning, it may belong in Shared or Infrastructure.
```

---

## 14. Testing Strategy

This architecture benefits from three test categories.

### 14.1 Unit Tests

Use unit tests for pure business rules and small domain behavior.

Examples:

- status transitions,
- price calculations,
- validation rules,
- domain methods,
- mapping logic with no external dependency.

### 14.2 Integration Tests

Use integration tests for feature handlers and endpoints.

Examples:

- create order endpoint returns 201,
- duplicate order is rejected,
- unauthorized user cannot access another user's resource,
- search endpoint returns paged results,
- database constraints are respected.

For many applications, integration tests are more valuable than heavily mocked unit tests because the feature handler depends on EF Core, validation, authorization, and persistence behavior.

### 14.3 Architecture Tests

Architecture tests enforce boundaries.

They should verify:

- modules do not reference another module's internal folders,
- no controller classes exist if Minimal APIs are the chosen API style,
- domain entities do not depend on web framework types,
- infrastructure does not leak into domain code,
- forbidden project references do not exist,
- naming conventions are followed,
- endpoints are registered through modules.

Example using NetArchTest-style rules:

```csharp
[Fact]
public void Modules_Should_Not_Depend_On_Other_Module_Internals()
{
    var result = Types.InCurrentDomain()
        .That()
        .ResideInNamespace("App.Api.Modules.Orders")
        .ShouldNot()
        .HaveDependencyOn("App.Api.Modules.Catalog.Domain")
        .GetResult();

    Assert.True(result.IsSuccessful);
}
```

Architecture tests are important because module boundaries are only real if the build can catch violations.

---

## 15. Frontend Alignment

The same idea can be applied to frontend code.

Avoid organizing frontend code only by technical type:

```text
components/
pages/
services/
models/
```

Prefer feature-oriented structure:

```text
src/app/
  features/
    auth/
    catalog/
    orders/
    account/
    notifications/

  shared/
    components/
    pipes/
    directives/
    models/

  core/
    api/
    auth/
    guards/
    interceptors/
    layout/
    config/
```

Frontend features should call backend APIs. They should not bypass the backend and call protected external providers directly.

Rule:

```text
Frontend feature folders should mirror product capabilities, not database tables.
```

---

## 16. Comparison With Other Popular Architectures

No architecture is universally best. The right architecture depends on team size, domain complexity, deployment requirements, scaling needs, and expected rate of change.

### 16.1 Traditional Layered Architecture

Typical structure:

```text
Controllers/
Services/
Repositories/
Entities/
DTOs/
```

Pros:

- familiar to many developers,
- easy to start,
- works well for small CRUD applications,
- simple mental model,
- many tutorials follow it.

Cons:

- features are scattered across technical layers,
- service layer often becomes bloated,
- repository layer often adds little value over ORM,
- business boundaries are weak,
- high risk of god services,
- changes often touch many folders.

When to use:

- very small apps,
- simple CRUD admin tools,
- prototypes,
- teams that strongly prefer conventional organization.

Why hybrid modular monolith + vertical slices may be better:

- features stay together,
- business modules are clearer,
- less generic service/repository ceremony,
- easier to enforce boundaries as the app grows.

### 16.2 Clean Architecture

Typical structure:

```text
Domain/
Application/
Infrastructure/
Presentation/
```

Pros:

- strong separation of concerns,
- domain logic can be isolated,
- infrastructure dependencies are controlled,
- works well for complex domains,
- testable when implemented carefully.

Cons:

- often overused for simple applications,
- many abstractions can appear before they are needed,
- code for one feature can be scattered across projects/layers,
- mapping overhead can become large,
- developers may focus more on dependency direction than feature clarity.

When to use:

- complex business domains,
- long-lived enterprise systems,
- systems where domain logic must be highly isolated,
- applications with volatile infrastructure choices.

Why hybrid modular monolith + vertical slices may be better:

- keeps use cases easier to find,
- allows direct data access when pragmatic,
- avoids unnecessary repository and mediator layers,
- can still preserve domain isolation where needed.

Important nuance:

```text
Hybrid modular monolith + vertical slices does not reject Clean Architecture.
It rejects applying Clean Architecture mechanically everywhere.
```

A module with complex domain behavior can still use clean architecture internally. A simple module can stay direct.

### 16.3 Hexagonal Architecture / Ports and Adapters

Core idea:

```text
Business logic depends on ports.
Infrastructure implements adapters.
```

Pros:

- excellent for isolating external systems,
- useful for payment gateways, email providers, file storage, APIs,
- supports test doubles for external dependencies,
- protects business logic from infrastructure volatility.

Cons:

- can create too many interfaces,
- unnecessary for simple database-backed CRUD,
- can be confusing when every dependency becomes a port,
- may add ceremony before the domain justifies it.

When to use:

- integrations with external providers,
- systems with swappable infrastructure,
- domains where external dependencies are unstable,
- applications with strong test isolation requirements.

How it fits the hybrid approach:

```text
Use ports/adapters at real boundaries.
Do not wrap everything by default.
```

For example, use an interface for an email sender, payment gateway, file store, or external API client. Do not create an interface for every small internal class.

### 16.4 Microservices

Core idea:

```text
Split the system into independently deployable services.
Each service owns its data and deployment lifecycle.
```

Pros:

- independent deployment,
- independent scaling,
- strong service boundaries,
- team autonomy at large scale,
- technology independence where needed,
- fault isolation when designed well.

Cons:

- high operational complexity,
- distributed transactions become hard,
- debugging is harder,
- local development is harder,
- observability requirements increase,
- network reliability becomes part of application logic,
- data consistency becomes more complex,
- deployment and infrastructure burden increases.

When to use:

- large teams,
- independent deployment is required,
- parts of the system need different scaling profiles,
- organizational boundaries match service boundaries,
- the team has strong DevOps and observability maturity.

When not to use:

- solo developer projects,
- early-stage products,
- small teams without platform support,
- domains whose boundaries are still changing,
- applications that can scale as one deployable.

Why hybrid modular monolith + vertical slices may be better:

- gives internal boundaries without distributed systems cost,
- keeps deployment simple,
- allows faster feature development,
- can evolve toward services later if boundaries become proven.

Key principle:

```text
A well-structured modular monolith is often the best starting point before microservices.
```

### 16.5 Service-Oriented Architecture

SOA often uses coarser services and enterprise integration patterns.

Pros:

- useful in large organizations,
- supports integration across systems,
- promotes service contracts,
- can work well for enterprise workflows.

Cons:

- can become heavyweight,
- may require governance overhead,
- shared enterprise models can become bloated,
- integration complexity can dominate development.

When to use:

- enterprise environments,
- integration-heavy organizations,
- multiple systems sharing business processes.

Why hybrid modular monolith + vertical slices may be better for a product application:

- less governance,
- faster iteration,
- simpler deployment,
- easier local reasoning.

### 16.6 Event-Driven Architecture

Core idea:

```text
Components communicate by publishing and consuming events.
```

Pros:

- decouples producers and consumers,
- supports async workflows,
- useful for audit trails and projections,
- scales well for integration-heavy systems,
- works well with outbox and message brokers.

Cons:

- eventual consistency is harder to reason about,
- debugging event flows can be difficult,
- ordering and idempotency matter,
- simple workflows can become unnecessarily complex,
- hidden coupling can move from code to event contracts.

When to use:

- async workflows,
- background processing,
- notifications,
- audit logs,
- projections,
- integration between bounded contexts.

How it fits the hybrid approach:

```text
Use events for reactions and async side effects.
Use direct calls for immediate required behavior.
```

Do not turn every method call into an event.

### 16.7 CQRS

Core idea:

```text
Separate commands that change state from queries that read state.
```

Pros:

- useful for complex read models,
- supports optimized queries,
- clarifies write vs read intent,
- works well with vertical slices,
- can reduce overgeneralized service methods.

Cons:

- full CQRS with separate stores is complex,
- eventual consistency may be introduced,
- unnecessary for simple CRUD,
- can lead to duplicate models.

When to use:

- complex querying,
- high read/write asymmetry,
- reporting screens,
- workflow-heavy systems,
- features where read and write models naturally differ.

How it fits the hybrid approach:

```text
Use simple CQRS naming at the feature level.
Do not introduce separate databases unless needed.
```

Example:

```text
Features/
  PlaceOrder/        command-style write slice
  GetOrderHistory/   query-style read slice
```

### 16.8 Domain-Driven Design

DDD is not just folder structure. It is a way to model complex business domains.

Pros:

- strong domain modeling,
- clear bounded contexts,
- ubiquitous language,
- good for complex business rules,
- helps prevent anemic models in rule-heavy systems.

Cons:

- often misapplied to CRUD apps,
- terminology can become performative,
- aggregates can be overdesigned,
- requires domain understanding,
- not every module needs rich domain modeling.

When to use:

- complex domains,
- non-trivial business rules,
- workflows with invariants,
- systems where language precision matters.

How it fits the hybrid approach:

```text
Use DDD concepts where the domain deserves them.
Keep simple modules simple.
```

A module can have rich aggregates if needed. Another module can be mostly transaction scripts and direct EF Core queries. That is acceptable.

---

## 17. When This Architecture Is a Good Fit

This architecture is applicable when:

- the application is expected to grow beyond basic CRUD,
- the team wants one deployable backend,
- microservices would be premature,
- feature work should be easy to locate,
- business areas can be identified,
- the domain has some complexity but not enough to justify heavy architecture everywhere,
- the team wants clear boundaries without distributed systems overhead,
- the codebase must be understandable by agents, new developers, or small teams,
- the system needs pragmatic testing and fast iteration.

Typical examples:

- SaaS products,
- internal business applications,
- marketplace platforms,
- workflow systems,
- content platforms,
- booking systems,
- media libraries,
- inventory systems,
- learning platforms,
- finance/admin tools,
- early-stage products that may scale later.

---

## 18. When This Architecture Is Not a Good Fit

Avoid or modify this architecture when:

- the application is a tiny CRUD app with no expected growth,
- the system requires independently deployable services from day one,
- different modules require radically different technology stacks,
- teams are already organized around separate service ownership,
- the organization has mature platform support for microservices,
- the application is mostly a script or data pipeline,
- the domain is so simple that modules add more structure than value.

No architecture is free. A modular monolith still requires discipline. Without tests and rules, it can degrade into a normal tangled monolith.

---

## 19. Practical Implementation Steps

### Step 1: Start With One Deployable

Create one backend application.

Do not split into services early.

```text
One API.
One deployment pipeline.
One database.
One local development setup.
```

This keeps the system easy to run and debug.

### Step 2: Define Initial Modules

Choose modules based on business capabilities, not database tables.

Bad module names:

```text
UsersTable
OrdersTable
ProductService
Common
Helpers
```

Better module names:

```text
Identity
Catalog
Orders
Billing
Notifications
Reporting
```

Module boundaries will improve over time. The goal is a reasonable starting point, not perfect domain decomposition.

### Step 3: Create Feature Slices Inside Modules

Each use case gets a folder.

```text
Modules/
  Orders/
    Features/
      PlaceOrder/
      CancelOrder/
      GetOrderHistory/
```

Do not place all requests in one global `Requests` folder. Do not place all handlers in one global `Handlers` folder. Keep code close to the use case.

### Step 4: Add Shared and Infrastructure Carefully

Create shared code only when multiple modules genuinely need it.

Start small:

```text
Shared/
  Results/
  Errors/
  Validation/
  Pagination/
  Time/

Infrastructure/
  Persistence/
  Caching/
  Email/
  BackgroundJobs/
  Observability/
```

Do not create a large shared kernel before the application proves what is actually shared.

### Step 5: Use Direct Data Access First

Use the ORM directly inside feature handlers unless there is a concrete reason not to.

This keeps code easy to follow.

Add repositories only when they remove complexity rather than add it.

### Step 6: Enforce Module Boundaries

Document the dependency rules and enforce them with architecture tests.

Rules should include:

```text
Modules cannot reference another module's Domain folder.
Modules cannot reference another module's Features folder.
Modules cannot reference another module's Persistence configuration.
Modules may reference another module's Contracts folder.
Domain code cannot depend on ASP.NET Core types.
Endpoints must stay thin.
Handlers must not be reused as cross-module services.
```

### Step 7: Add Integration Tests Around Features

Test important behavior through endpoint or handler integration tests.

Focus on:

- auth,
- ownership,
- duplicate prevention,
- validation,
- database persistence,
- transactions,
- pagination,
- important business rules.

### Step 8: Add Observability Early

Even a monolith needs observability.

Add:

- structured logging,
- request correlation IDs,
- error logging,
- health checks,
- metrics where useful,
- background job visibility.

This helps debugging before complexity grows.

### Step 9: Keep Documentation Close to Architecture

Maintain a small architecture guide in the repository.

It should define:

- module rules,
- feature folder rules,
- data ownership rules,
- API conventions,
- test expectations,
- what not to do,
- how to add a new feature.

Documentation matters because architecture is a set of decisions, not just folders.

---

## 20. Feature Implementation Checklist

When adding a new backend feature:

```text
1. Identify the owning module.
2. Create a feature folder under that module.
3. Add request and response models.
4. Add a validator for request-level validation.
5. Add a handler for use case orchestration.
6. Keep endpoint/controller/message handler thin.
7. Use the module's domain rules where needed.
8. Use direct DbContext access unless abstraction is justified.
9. Respect data ownership rules.
10. Add tests for important behavior.
11. Register services in the module registration file.
12. Register route or message mapping in the module registration file.
13. Update documentation if route, module boundary, or architecture rules changed.
```

---

## 21. Anti-Patterns to Avoid

### 21.1 Fake Repositories Everywhere

Bad:

```text
IProductRepository
IOrderRepository
IUserRepository
IGenericRepository<T>
IUnitOfWork
```

This often duplicates ORM behavior and hides useful query details.

Use repositories only when they add meaningful abstraction.

### 21.2 God Services

Bad:

```text
OrderService
UserService
ProductService
```

with dozens of unrelated methods.

Prefer feature handlers:

```text
PlaceOrderHandler
CancelOrderHandler
GetOrderHistoryHandler
```

### 21.3 Global DTO Folders

Bad:

```text
DTOs/
  CreateOrderRequest.cs
  CancelOrderRequest.cs
  ProductResponse.cs
```

Feature-specific models should live with their feature.

Shared models should exist only when they are truly shared contracts.

### 21.4 Shared Everything

A large `Shared` folder becomes a second monolith inside the monolith.

Move business-specific code back into modules.

### 21.5 Module Boundary Theater

A folder named `Modules` does not create modularity by itself.

Modularity requires:

- ownership rules,
- dependency rules,
- architecture tests,
- stable contracts,
- disciplined writes,
- controlled sharing.

### 21.6 Premature Microservice Design Inside a Monolith

Do not force every module to have its own database, message bus, API client, and deployment boundary while still deploying as one app.

That creates microservice pain without microservice benefits.

---

## 22. How This Architecture Can Evolve

A modular monolith can evolve in stages.

### Stage 1: Simple Modular Monolith

```text
One application.
One database.
Modules as folders.
Vertical slices inside modules.
```

### Stage 2: Stronger Internal Boundaries

```text
Module contracts.
Architecture tests.
Outbox for important events.
Module-owned table rules.
```

### Stage 3: Extracted Service Candidate

Only consider extracting a module when:

- the boundary is stable,
- the module has independent scaling needs,
- the module has a separate team owner,
- the module has clear contracts,
- data ownership is already clean,
- deployment independence is worth the operational cost.

### Stage 4: Service Extraction

Extraction becomes easier because the module already has:

- a defined boundary,
- contracts,
- owned data,
- feature organization,
- tests,
- reduced dependency on other internals.

A modular monolith does not guarantee easy extraction, but it makes extraction far more realistic than a tangled layered monolith.

---

## 23. Decision Matrix

| Criterion | Layered App | Clean Architecture | Modular Monolith + Vertical Slices | Microservices |
|---|---:|---:|---:|---:|
| Startup simplicity | High | Medium | Medium | Low |
| Feature discoverability | Medium | Medium | High | Medium |
| Business boundary clarity | Low | Medium | High | High |
| Deployment simplicity | High | High | High | Low |
| Operational complexity | Low | Low | Low | High |
| Scaling teams independently | Low | Medium | Medium | High |
| Scaling runtime independently | Low | Low | Low/Medium | High |
| Works for small teams | High | Medium | High | Low |
| Protects against tangling | Low | Medium | High if enforced | High if designed well |
| Ceremony risk | Low | High | Medium | High |
| Best default for growing product | Medium | Medium | High | Low early, high later |

---

## 24. Recommended Rules Summary

Use these as the architecture constitution:

```text
1. One backend deployable by default.
2. Organize business areas as modules.
3. Organize use cases as vertical slices inside modules.
4. Keep endpoints thin.
5. Put use case orchestration in handlers.
6. Put reusable business rules in module domain code.
7. Put cross-cutting utilities in Shared only when truly shared.
8. Put technical implementations in Infrastructure.
9. Use direct ORM access unless abstraction is justified.
10. Do not create generic repositories by default.
11. Do not create a UnitOfWork abstraction by default.
12. A module may reference another module's Contracts.
13. A module must not reference another module's internals.
14. Writes must respect module ownership.
15. Reads may compose pragmatically.
16. Use in-process events for reactions.
17. Use an outbox when reliable async work matters.
18. Add architecture tests to enforce boundaries.
19. Keep documentation updated when architecture changes.
20. Prefer clarity over pattern purity.
```

---

## 25. Final Position

Hybrid modular monolith + vertical slice architecture is a strong default for many modern applications because it solves the most common growth problem: a codebase that starts simple but becomes hard to change.

It gives developers:

- one deployable system,
- clear business modules,
- feature-focused code organization,
- pragmatic data access,
- enforceable boundaries,
- a path toward future service extraction,
- fewer unnecessary abstractions than many enterprise templates,
- less operational burden than microservices.

The architecture works best when treated as a practical discipline, not a rigid religion.

The goal is not to prove that every dependency points in the perfect direction. The goal is to make the next feature easy to add, the next bug easy to locate, and the next architectural decision obvious from the structure of the code.
