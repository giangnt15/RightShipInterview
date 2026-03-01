# RightShip Benchmark

K6 load tests for RightShip APIs.

## Prerequisites

- [K6](https://k6.io/docs/get-started/installation/) installed
- Product Service and Order Service running
- At least one product created (via `POST /api/products` or Swagger)

## Order Creation Test

Tests `POST /api/orders` on Order Service.

- Uses a **random X-Created-By** (GUID) each iteration to avoid the rate limiter (1 order per 60s per header)
- Fetches product IDs from Product Service during setup
- Exports reports to `benchmark/reports/summary.json` and `summary.html`

### Run

From **project root** (`RightShipInterview/`):

```bash
# Default: 5 VUs, 30 seconds
k6 run Revamped/benchmark/order-creation.js

# Custom
k6 run --vus 10 --duration 60s Revamped/benchmark/order-creation.js
```

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| ORDER_SERVICE_BASE_URL | `http://localhost:5213` | Order Service URL |
| PRODUCT_SERVICE_BASE_URL | `http://localhost:5118` | Product Service URL (for setup) |
| K6_REPORT_DIR | `Revamped/benchmark/reports` | Report output directory |

Example (HTTPS):

```bash
ORDER_SERVICE_BASE_URL=https://localhost:7152 PRODUCT_SERVICE_BASE_URL=https://localhost:7071 k6 run Revamped/benchmark/order-creation.js
```

### Reports

After a run, check:

- `Revamped/benchmark/reports/summary.json` — Full data
- `Revamped/benchmark/reports/summary.html` — HTML report
