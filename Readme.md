# GateKeeper — High-Performance .NET 10 API Gateway

**GateKeeper** is a lightweight, high-throughput API Gateway built with **.NET 10**. It features config-driven reverse proxying, sliding-window rate limiting via Redis, TTL-backed API key authentication caching, and asynchronous request telemetry using Redis Streams and a dedicated background consumer worker.

---


### Request Flow
1. **Async Logging Middleware:** Measures total request lifecycle execution time and writes structured request metadata (`apikey`, `path`, `method`, `statuscode`, `ms`) asynchronously to a **Redis Stream** (`gateway:logs`) to keep hot-path latency low.
2. **Sliding Window Rate Limiter:** Evaluates window rate limits using configurable path patterns (e.g., `/products` limited to 100 requests per 60s window). Returns `429 Too Many Requests` when limits are breached.
3. **API Key Authentication:** Validates the `X-Api-Key` request header against a sub-millisecond **Redis TTL cache**. On cache misses, queries **PostgreSQL** and populates the Redis cache dynamically.
4. **Reverse Proxying:** Dynamically forwards authorized traffic to configured backend microservice targets.
5. **Background Telemetry Consumer:** A hosted service continuously processes batch items from the Redis Stream and commits log records to **PostgreSQL**.

---

## Performance & Load Test Results (k6)

Load testing was executed using **k6** with **10 Virtual Users (VUs)** over a **30-second steady load** scenario against the `/products` gateway endpoint.

### Core Benchmarks
| Metric | Measured Value |
| :--- | :--- |
| **Throughput (RPS)** | **93.78 req/sec** |
| **Median (P50) Latency** | **1.94 ms** |
| **P90 Latency** | **2.26 ms** |
| **P95 Latency** | **3.00 ms** |
| **Passed Requests (200 OK)** | **100 requests** |
| **Blocked Requests (429 Rate Limited)** | **2,718 requests** |
| **Success / Block Check Ratio** | **99.86%** |

### Key Performance Highlights
- **Sub-3ms Latency:** 95% of all gateway requests completed within **3.00 ms** (median **1.94 ms**), proving ultra-low middleware overhead.
- **Strict Limiter Accuracy:** Exactly **100 requests** were allowed through before rate-limiting logic capped execution. The remaining 2,718 burst requests were instantly throttled with `429 Too Many Requests`.
- **Zero Critical Failures:** No `500 Internal Server Error` responses were produced under continuous concurrent load.

---

## Tech Stack

- **Framework:** .NET 10 (ASP.NET Core Web API)
- **Data Stores:** PostgreSQL 16 (Entity Framework Core / Npgsql)
- **Caching & Streaming:** Redis 7 (StackExchange.Redis / Distributed Cache)
- **Containerization:** Docker & Docker Compose
- **Benchmarking:** k6

---

## Getting Started (Docker Compose)

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed.

### Run the Infrastructure

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/your-username/GateKeeper.git](https://github.com/your-username/GateKeeper.git)
   cd GateKeeper