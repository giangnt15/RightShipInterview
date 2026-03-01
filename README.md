# RightShip

A microservices-based order management system built with .NET 8, following Domain-Driven Design (DDD) and Onion Architecture.

## Improvements over Legacy Service

The `Revamped/` solution replaces the original `LegacyOrderService` console app with the following improvements:

| Area | Legacy | Revamped |
|------|--------|----------|
| **Correctness** | SQL injection risk, blocking `Thread.Sleep` in product lookup, hardcoded in-memory products | Parameterized queries, async I/O, database-backed product catalog with proper validation |
| **Architecture** | Single console app, anemic models, no separation of concerns | Two services (Order, Product), DDD aggregates, Onion Architecture, clear layers |
| **Product data** | In-memory dictionary, three hardcoded products | SQLite/DB-backed `Product` aggregate with CRUD, stock management, reservations |
| **Stock handling** | No stock tracking or reservation | Two-phase reservation with TTL, background expiry, prevents overselling |
| **Order model** | Denormalized (CustomerName, ProductName, Price), single line | `Order` aggregate with `OrderLine` entities, multiple products per order, value objects (`Money`) |
| **API** | Console stdin/stdout only | REST API (Swagger), gRPC for internal calls |
| **Observability** | No tracing or structured logging | OpenTelemetry, distributed tracing, W3C context propagation |
| **Operations** | No health checks | Liveness and readiness probes, dependency checks |
| **Resilience** | None | Polly retry, circuit breaker, timeouts for Product Service calls |
| **Rate limiting** | None | Per-`X-Created-By` sliding window on order creation |
| **Testability** | Tightly coupled, no interfaces | Unit tests for domain and application, mocked repositories |
| **Events** | None | Domain events, transactional outbox for future integrations |

---

## Architecture

### High-Level Overview

The system consists of two collaborating services:

| Service | Purpose |
|---------|---------|
| **Order Service** | Creates and queries orders. Validates products and reserves stock via Product Service. |
| **Product Service** | Manages product catalog, pricing, and stock reservations. |

**Communication**: Order Service → Product Service via **gRPC** (with retry, circuit breaker, and timeouts).

### Domain-Driven Design

- **Aggregates**
  - Order Service: `Order` (with `OrderLine` entities)
  - Product Service: `Product`, `ProductReservation`
- **Value Objects**: `Money`, `ProductQuantity`
- **Domain Events**: e.g. `OrderCreated`
- **Repository Interfaces**: Persistence abstractions in the domain layer

### Onion Architecture

Both services follow Onion Architecture with these layers:

- **Domain** — Core model, aggregates, value objects, repository interfaces
- **Application** — Use cases, DTOs, application services
- **Infrastructure** — Persistence (EF Core), gRPC clients, rate limiting
- **Presentation** — ASP.NET Core Web API, gRPC endpoints

### Cross-Cutting Concerns

- **OpenTelemetry** — Distributed tracing and metrics
- **Health Checks** — Liveness (`/health/live`) and readiness (`/health/ready`) including DB and dependency checks
- **Rate Limiting** — Order creation limited per `X-Created-By` header
- **Resilience** — Polly retry and circuit breaker for Product Service calls

### Key Decisions

#### Reservation Model

Product Service uses a **two-phase reservation flow** to coordinate stock between services without distributed locks:

1. **CreateReservation** — Order Service requests a reservation. Product Service creates a `ProductReservation` aggregate with status `Pending` and a TTL (default 5 minutes). No quantity is deducted yet; available stock is computed as `Product.Quantity - sum(Pending reservations)`.
2. **ConfirmReservations** — After Order Service commits the order, it calls Product Service to confirm. The reservation is marked `Confirmed` and quantity is deducted from the product.

**Why two-phase?** If Order Service fails to commit (e.g. DB error), the reservation eventually expires. A background job (`ExpiredReservationReleaseService`) periodically marks expired pending reservations as `Expired`, releasing stock without manual intervention. This avoids losing stock when the order never materializes.

#### Outbox Pattern

Domain events (e.g. `OrderCreated`, `ProductReservationCreated`) are persisted to an **outbox table** (`outbox_message`) in the **same database transaction** as the aggregate changes. This is the [Transactional Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html).

**Benefits:**
- **At-least-once delivery** — Events are never lost if the service crashes before publishing.
- **Consistency** — Event and aggregate are committed atomically; no partial state.

**Current state:** The outbox stores messages but no publisher runs yet. To integrate with other parts of the system (notifications, analytics, other services), we can add:

- **Polling Publisher** — A background job that polls `outbox_message` for unsent rows and publishes to a message broker (RabbitMQ, Kafka, etc.).
- **CDC (Change Data Capture)** — Use database change streams (e.g. Debezium with SQL Server/PostgreSQL) to capture inserts into `outbox_message` and publish them, avoiding application-level polling.

Both approaches allow eventual consistency across services without compromising the transactional integrity of the source database.

---

## Solution Structure

```
Revamped/
├── RightShip.Core/                    # Shared libraries
│   ├── RightShip.Core.Domain/
│   ├── RightShip.Core.Application/
│   ├── RightShip.Core.Common/
│   ├── RightShip.Core.Observability/
│   └── RightShip.Core.Persistence.EfCore/
├── RightShip.OrderService/
│   ├── RightShip.OrderService.Domain/
│   ├── RightShip.OrderService.Domain.Shared/
│   ├── RightShip.OrderService.Application/
│   ├── RightShip.OrderService.Application.Contracts/
│   ├── RightShip.OrderService.Infrastructure/
│   ├── RightShip.OrderService.Persistence.EfCore/
│   ├── RightShip.OrderService.WebApi/
│   └── RightShip.OrderService.UnitTest/
├── RightShip.ProductService/
│   ├── RightShip.ProductService.Domain/
│   ├── RightShip.ProductService.Domain.Shared/
│   ├── RightShip.ProductService.Application/
│   ├── RightShip.ProductService.Application.Contracts/
│   ├── RightShip.ProductService.Infrastructure/
│   ├── RightShip.ProductService.Persistence.EfCore/
│   ├── RightShip.ProductService.WebApi/
│   └── RightShip.ProductService.UnitTest/
└── benchmark/                         # K6 load tests
    ├── order-creation.js
    └── reports/
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- (Optional) [Docker](https://www.docker.com/) for containerized runs
- (Optional) [K6](https://k6.io/docs/get-started/installation/) for load testing

---

## Running Locally

### Option 1: `dotnet run` (recommended for development)

1. **Start Product Service** (Order Service depends on it). Use HTTPS for gRPC support:

   ```bash
   cd Revamped/RightShip.ProductService
   dotnet run --project RightShip.ProductService.WebApi --launch-profile https
   ```

   - HTTP: `http://localhost:5118`
   - HTTPS (gRPC): `https://localhost:7071`
   - Swagger: `http://localhost:5118/swagger`

2. **Start Order Service** (in a new terminal):

   ```bash
   cd Revamped/RightShip.OrderService
   dotnet run --project RightShip.OrderService.WebApi
   ```

   - HTTP: `http://localhost:5213`
   - Swagger: `http://localhost:5213/swagger`

3. **Create products** (required before creating orders):

   Use Swagger at `http://localhost:5118/swagger` to `POST /api/products` with body:

   ```json
   {
     "name": "Widget A",
     "price": 19.99,
     "quantity": 100
   }
   ```

---

## Configuration

### Order Service (`appsettings.json`)

| Section | Key | Default | Description |
|---------|-----|---------|-------------|
| ConnectionStrings | DefaultConnection | `Data Source=orders.db` | SQLite database path |
| ProductService | Url | `http://localhost:5118` | Product Service base URL |
| ProductService | TimeoutSeconds | 10 | gRPC call timeout |
| ProductService | RetryCount | 3 | Polly retry count |
| ProductService | CircuitBreakerFailureCount | 5 | Failures before circuit opens |
| RateLimiting | OrderCreation:PermitLimit | 1 | Orders per window per X-Created-By |
| RateLimiting | OrderCreation:WindowSeconds | 60 | Sliding window (seconds) |

### Product Service (`appsettings.json`)

| Section | Key | Default | Description |
|---------|-----|---------|-------------|
| ConnectionStrings | DefaultConnection | `Data Source=products.db` | SQLite database path |
| Reservation | DefaultTtlSeconds | 300 | Reservation TTL |

---

## API Endpoints

### Order Service

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/orders/{id}` | Get order by ID |
| POST | `/api/orders` | Create order (rate limited; requires `X-Created-By` header) |
| GET | `/health/live` | Liveness probe |
| GET | `/health/ready` | Readiness (DB + Product Service) |

### Product Service

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Paged product list |
| GET | `/api/products/{id}` | Get product by ID |
| POST | `/api/products` | Create product |
| GET | `/health/live` | Liveness probe |
| GET | `/health/ready` | Readiness (DB) |

Product Service also exposes **gRPC** for internal calls (price lookup, reservations).

---

## Load Testing (K6)

An order creation load test is available in `Revamped/benchmark/`.

**Prerequisites**: 
- K6 installed
- Product Service and Order Service running with at least one product.

```bash
# From project root
k6 run Revamped/benchmark/order-creation.js
```

| Env Variable | Default | Description |
|--------------|---------|-------------|
| ORDER_SERVICE_BASE_URL | `http://localhost:5213` | Order Service URL |
| PRODUCT_SERVICE_BASE_URL | `http://localhost:5118` | Product Service URL (for setup) |
| K6_REPORT_DIR | `Revamped/benchmark/reports` | Report output path |

Reports are written to `Revamped/benchmark/reports/summary.json` and `summary.html`.

---

## Development

### Run Tests

```bash
# Order Service
cd Revamped/RightShip.OrderService
dotnet test

# Product Service
cd Revamped/RightShip.ProductService
dotnet test
```

### Add Migrations

```bash
# Order Service
cd Revamped/RightShip.OrderService
dotnet ef migrations add <MigrationName> --project RightShip.OrderService.Persistence.EfCore --startup-project RightShip.OrderService.WebApi

# Product Service
cd Revamped/RightShip.ProductService
dotnet ef migrations add <MigrationName> --project RightShip.ProductService.Persistence.EfCore --startup-project RightShip.ProductService.WebApi
```

---
