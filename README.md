Order Processing Service (C#/.NET 8, Docker, PostgreSQL and Redis)

Minimal microservice that accepts orders via HTTP and processes them asynchronously using a background worker and Redis queue. Orders are persisted in PostgreSQL. Containerized with Docker.

Quick Start
Follow these steps to clone, run, and test locally.

1) Clone the repo
git clone https://github.com/vinil2001/Order_Processing_Service.git
cd <repo>

2) Start infrastructure (PostgreSQL + Redis)
docker compose up -d

3) Apply EF Core migrations
dotnet ef database update \
  --project ./src/OrderProcessingService.Infrastructure/OrderProcessingService.Infrastructure.csproj \
  --startup-project ./src/OrderProcessingService.Api/OrderProcessingService.Api.csproj

4) Run API and Worker (two terminals)
dotnet run --project ./src/OrderProcessingService.Api/OrderProcessingService.Api.csproj
dotnet run --project ./src/OrderProcessingService.Worker/OrderProcessingService.Worker.csproj

5) Send a test request (see below)

How to run the service
  - .NET 8 SDK
  - Docker Desktop (for PostgreSQL and Redis)
  - Optional: curl/Postman for testing

1) Start infrastructure (PostgreSQL + Redis)

From the repository root:

# Start postgres and redis in background
docker compose up -d
# Check containers
docker compose ps

- Postgres DSN used by the app: `Host=localhost;Port=5432;Database=orders;Username=postgres;Password=postgres`
- Redis endpoint used by the app: `localhost:6379`

If ports are busy, adjust `docker-compose.yml` port mappings and update `ConnectionStrings` in `appsettings.json` accordingly.

2) Apply EF Core migrations

# From repo root
dotnet ef database update \
  --project ./src/OrderProcessingService.Infrastructure/OrderProcessingService.Infrastructure.csproj \
  --startup-project ./src/OrderProcessingService.Api/OrderProcessingService.Api.csproj

This creates the `Orders` and `OrderItems` tables in the `orders` database.

3) Run services locally

Use two terminals:

# Terminal 1: API
dotnet run --project ./src/OrderProcessingService.Api/OrderProcessingService.Api.csproj
# API listens on http://localhost:5116 (per launchSettings.json)

# Terminal 2: Worker
dotnet run --project ./src/OrderProcessingService.Worker/OrderProcessingService.Worker.csproj

4) Send a test order

- Swagger: open `http://localhost:5116/swagger`
- Or curl:

curl -X POST http://localhost:5116/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "00000000-0000-0000-0000-000000000001",
    "items": [
      { "productId": "00000000-0000-0000-0000-0000000000a1", "quantity": 2, "unitPrice": 10.5 },
      { "productId": "00000000-0000-0000-0000-0000000000b2", "quantity": 1, "unitPrice": 5.0 }
    ],
    "totalAmount": 0
  }'


Expected response: HTTP 202 Accepted with `orderId` and `status = Pending`.
The worker consumes the message from Redis, simulates processing (~1s), and sets `status = Processed` in the database.

5) Inspect data in PostgreSQL

Quick check via container:

docker exec -it orders-postgres psql -U postgres -d orders -c 'SELECT "Id","Status","ProcessedAt" FROM "Orders" ORDER BY "CreatedAt" DESC LIMIT 10;'

API

- **POST `/orders`** — Submits an order and returns 202 immediately with `orderId`.
  - Body (example): see section “Send a test order”.
  - Response: `{ orderId, status, createdAt, processedAt }`
- (Optional) Add **GET `/orders/{id}`** to query current status if needed.

Configuration

- Connection strings are in:
  - `src/OrderProcessingService.Api/appsettings.json`
  - `src/OrderProcessingService.Worker/appsettings.json`
- Defaults for local runs:
  - PostgreSQL: `Host=localhost;Port=5432;Database=orders;Username=postgres;Password=postgres`
  - Redis: `localhost:6379`
- If startup ordering causes Redis to be unavailable momentarily, you can allow reconnect attempts by appending `,abortConnect=false` to the Redis connection string in the Worker.
- If you later containerize API/Worker inside compose, use service names from the compose network:
  - PostgreSQL host `postgres:5432`
  - Redis host `redis:6379`

Design decisions and trade-offs

- **Clean Architecture (simplified)**
  - `Core` independent of frameworks; `Infrastructure` implements persistence/queue; `Api` exposes HTTP; `Worker` handles async processing.
  - Trade-off: More projects for a small app, but better separation of concerns.

- **Redis list as queue**
  - Enqueue with `RPUSH`, dequeue with polling `LPOP` (via `StackExchange.Redis`).
  - Trade-off: Simpler than Streams/BRPOP; acceptable for demo. For production, consider blocking ops (BRPOP) or Redis Streams (XADD/XREAD) for robust delivery.

- **EF Core + PostgreSQL**
  - Fast development, migrations, strong typing.
  - Trade-off: ORM adds overhead; for hot paths, Dapper could be used.

- **BackgroundService + scoped services**
  - `OrderProcessingWorker` is singleton; it creates a scope per message to resolve `IOrderProcessor`/`DbContext` safely.
  - Trade-off: Slightly more boilerplate; correct lifetimes.

- **Observability**
  - Logging via `ILogger` (API and Worker). Metric `orders.processed` via `System.Diagnostics.Metrics`.
  - Trade-off: Minimal; can extend with OpenTelemetry exporters.

Assumptions

- Minimal, not production-grade: no idempotency, no retries/backoff beyond simple polling, no dead-letter queue, no auth.
- Business logic is simulated (validation + artificial delay).
- Local development defaults: `localhost` for Postgres/Redis. If containerizing API/Worker, use service names (`postgres:5432`, `redis:6379`) inside the compose network.
- Swagger enabled in Development for manual testing.

Troubleshooting

- **Cannot connect to PostgreSQL**: ensure containers are up (`docker compose ps`), wait 10–20 seconds for health checks, then re-run migrations. Check logs: `docker compose logs postgres --since=30s`.
- **Cannot connect to Redis**: verify Redis is running. Consider `,abortConnect=false` in the Worker connection string to allow retries. Logs: `docker compose logs redis --since=30s`.
- **Port conflicts (5432/6379)**: edit `docker-compose.yml` port mappings (e.g., `"5433:5432"`) and update `appsettings.json`.
- **EF Tools mismatch**: prefer running commands from the repo root and use absolute paths if needed. A local tool manifest can pin `dotnet-ef` to 8.x.
