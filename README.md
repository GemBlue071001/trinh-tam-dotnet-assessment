# CodeLeap E-Commerce

An e-commerce application composed of an ASP.NET Core Web API, PostgreSQL, and a React/Vite UI prototype. The backend currently focuses on the core business flows: authentication, product management, cart, and checkout.

## Tech Stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core + Npgsql/PostgreSQL 16
- JWT Bearer authentication, role-based authorization, and BCrypt
- Swagger / OpenAPI
- Docker Compose
- xUnit + Moq
- React + TypeScript + Vite (UI prototype; not connected to the API yet)

## Project Structure

```text
.
├── CodeLeapBackend/
│   ├── Domain/                  # Business entities and enums
│   ├── Application/             # Services, request/response models, repository interfaces
│   ├── Infrastructure/          # EF Core DbContext, repository implementations, migrations, seed data
│   ├── ECommerceApi/            # Controllers, startup configuration, middleware, Dockerfile
│   ├── Application.UnitTests/   # Application service unit tests
│   ├── scripts/                 # EF migration scripts for Git Bash
│   └── CodeLeapBackend.slnx
├── CodeLeapFrontend/            # React/Vite UI prototype
├── docker-compose.yml           # PostgreSQL and API containers
└── README.md
```

Dependency direction:

```text
ECommerceApi → Application ← Infrastructure
                    ↑            ↑
                    └── Domain ──┘
```

`Application` contains business services and abstractions only. `Infrastructure` contains EF Core/Npgsql, concrete repositories, migrations, and seed data. `ECommerceApi` is responsible for HTTP, JWT, Swagger, dependency injection, and exception handling.

## Prerequisites

- .NET SDK 10
- Docker Desktop (recommended)
- Or PostgreSQL 16 when running the API locally
- Node.js 20+ when running the frontend prototype

## Run with Docker

From the repository root:

```bash
docker compose up --build
```

Docker starts PostgreSQL on host port `4321` and the API at `http://localhost:8080`. The API automatically applies EF migrations and seeds development data at startup.

- Swagger UI: `http://localhost:8080/swagger`
- PostgreSQL host: `localhost:4321`
- Database: `codeleap_ecommerce`
- Username: `postgres`
- Password: `postgres`

Stop the containers:

```bash
docker compose down
```

Remove containers and the local database volume (all local data will be lost):

```bash
docker compose down -v
```

## Run the API Locally

Start PostgreSQL only:

```bash
docker compose up -d postgres
```

Then run the API:

```powershell
cd .\CodeLeapBackend
dotnet restore
dotnet run --project .\ECommerceApi\ECommerceApi.csproj
```

With the default HTTP launch profile, Swagger is available at `http://localhost:5067/swagger`.

Local settings are stored in `CodeLeapBackend/ECommerceApi/appsettings.Development.json`. Do not use the development JWT key in production; replace it with environment variables or a secret manager.

## Database Migrations

The API calls `Database.MigrateAsync()` on startup. To apply migrations manually from PowerShell:

```powershell
cd .\CodeLeapBackend
dotnet ef database update --project .\Infrastructure\Infrastructure.csproj --startup-project .\ECommerceApi\ECommerceApi.csproj --context ECommerceDbContext
```

Git Bash scripts are also available:

```bash
cd CodeLeapBackend
./scripts/add-migration.sh <MigrationName>
./scripts/update-database.sh
```

## Seed Data

On the first startup, the application creates:

- Admin account: `admin@codeleap.local` / `Admin@123`
- Three electronic products: MacBook Air M3, Sony WH-1000XM5, and Samsung 27-inch Monitor.

These credentials are for development and demo environments only.

## Implemented API Features

Swagger is the source of truth for detailed request and response schemas. Main routes:

| Area | Endpoint | Access | Description |
| --- | --- | --- | --- |
| Auth | `POST /api/auth/register` | Public | Registers a Customer; email addresses must be unique. |
| Auth | `POST /api/auth/login` | Public | Authenticates a user and returns a JWT access token. |
| Auth | `GET /api/auth/me` | JWT | Returns the current user profile. |
| Auth | `POST /api/auth/logout` | JWT | Revokes the current access token. |
| Product | `GET /api/products` | Public | Searches by `keyword`, filters by `category`, and pages via `pageNumber`/`pageSize`. |
| Product | `GET /api/products/{productId}` | Public | Gets an active product by id. |
| Admin product | `POST /api/admin/products` | Admin | Creates a product. |
| Admin product | `GET /api/admin/products/{productId}` | Admin | Gets a product by id. |
| Admin product | `PUT /api/admin/products/{productId}` | Admin | Updates a product. |
| Admin product | `DELETE /api/admin/products/{productId}` | Admin | Soft-deletes a product. |
| Cart | `GET /api/cart` | Customer | Gets the current customer's active cart. |
| Cart | `POST /api/cart/items` | Customer | Adds a product to the cart. |
| Cart | `PUT /api/cart/items/{productId}` | Customer | Changes an item's quantity. |
| Cart | `DELETE /api/cart/items/{productId}` | Customer | Removes an item from the cart. |
| Checkout | `POST /api/cart/checkout` | Customer | Creates a `Paid` order, snapshots prices, decreases stock, and closes the cart. |

### Authentication and Authorization

- Passwords are hashed with BCrypt.
- Each JWT includes the user id, email, role (`Admin` or `Customer`), and a unique `jti`.
- Every `[Authorize]` endpoint checks its `jti` against the `revoked_access_tokens` table. A logged-out access token is rejected even before it expires.
- Products use PostgreSQL's `xmin` concurrency token. Checkout retries up to three times after an optimistic concurrency conflict to prevent overselling during simultaneous checkouts.
- A global exception handler returns `ProblemDetails` for unexpected errors and `409 Conflict` for concurrency conflicts.

### Checkout Rules

Checkout reads the cart with its products, confirms that every product is active and has sufficient stock at checkout time, snapshots `ProductName`, `UnitPrice`, and `LineTotal` into each order item, then decreases stock and persists all changes in one `SaveChanges` call. EF Core uses its default transaction for that save operation.

## Tests

```powershell
cd .\CodeLeapBackend
dotnet test .\Application.UnitTests\Application.UnitTests.csproj
```

There are currently 33 unit tests for Auth, Product, Cart, and Checkout services. They cover happy paths, validation/failure cases, insufficient stock, missing resources, JWT claims, token revocation, and retrying after a concurrency conflict.


## Not Implemented Yet / Future Improvements

- Connect the React frontend to the real API: login, token storage, product search, cart, and checkout.
- Refresh tokens, rotation/session revocation, and multi-device login support. The `AuthSession` entity exists, but refresh endpoints and logic have not been implemented.
- User profile updates.
- Admin order management: listing, details, and order administration actions.
- Hard-delete products. The current scope supports soft delete only.
- Image upload/management instead of accepting a single `imageUrl`.
- PostgreSQL/Testcontainers integration tests and end-to-end API tests.
- Rate limiting, email verification, a detailed password policy, complete audit logging, and scheduled cleanup of expired revoked tokens.
