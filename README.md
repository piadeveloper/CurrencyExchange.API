
## CurrencyExchange.API

CurrencyExchange.API is a .NET 9 web API for fetching real-time and historical foreign exchange rates. It integrates with the **Frankfurter** API as the default provider, supports JWT-based authentication, and is optimized for production with OpenTelemetry and Serilog for logging.

---

### **Features**

- **Authentication**: JWT-based token authentication.
- **Real-Time Currency Rates**: Fetch latest currency rates.
- **Historical Rates**: Fetch rates for specific dates.
- **Time Series Data**: Get exchange rates for a period.
- **Pagination**: Support for paginated responses.
- **OpenTelemetry**: Tracing and logging to Elasticsearch.
- **In-Memory Caching**: Reduced API latency and improved performance.
- **Health Checks**: Elasticsearch and API health checks.

---

### **Technologies Used**

- **ASP.NET Core 9**
- **Serilog** (File, Console, Elasticsearch sinks)
- **OpenTelemetry** (Tracing and metrics)
- **Docker and Docker Compose** (for containerized deployments)
- **Frankfurter API** (default currency exchange provider)
- **Elasticsearch & Kibana** (for logging and visualization)

---

### **Getting Started**

#### **Prerequisites**

- **.NET 9 SDK**
- **Docker and Docker Compose**

---

#### **Running the Application**

1. **Clone the Repository:**

```bash
git clone https://github.com/piadeveloper/CurrencyExchange.API.git
cd CurrencyExchange.API
```

2. **Build and Run the Docker Containers:**
To run the application locally, ensure you have Docker and Docker Compose installed and configured for the required services (ElasticSearch, Kibana). After that, you can start the application and its dependencies by running:

```bash
docker-compose up --build
```
This will build and start the services, including:

- `CurrencyExchange.API`: The main API.
- `elasticsearch-server`: The ElasticSearch server.
- `kibana`: The Kibana dashboard for monitoring.

3. **Verify the API is Running:**

Visit: [http://localhost:8080/api/version/getversion](http://localhost:8080/api/version/getversion)

---

#### **Application Endpoint examples**
1. http://localhost:8080/api/CurrencyConverter/latest?provider=Frankfurter&baseCurrency=USD&amount=1 - return last retes for base currency

2. http://localhost:8080/api/CurrencyConverter/timeseries/2021-01-01/2022-12-31?provider=Frankfurter&baseCurrency=USD&amount=1&page=5&pageSize=25 - return currency rates from 2021-01-01 to 2022-12-31. Information will start from 5 page, page will contain 25 items (dates) 

3. http://localhost:8080/api/CurrencyConverter/historical/2021-01-01?provider=Frankfurter&baseCurrency=USD&amount=1&page=1&pageSize=5 - return currency rates for 2021-01-01. Information will start from 1 page, page will contain 5 items (rates) 

**Note:**
1. In all endpoint examples we have provider=Frankfurter, we can remove this parameter and by default API will use Frankfurter as currency exchange API provider
2. If we pass request without page and pageSize parameters API would use page = 1 and PageSize = 20 by default
3. If we pass unsupported currency to any endpoint our API returns HTTP 400

#### **Environment Variables (`appsettings.json`)**
By default all information entered in appsettings.json is ready for docker-compose application run.

1. **JwtSettings** section contains information which will be used in token generating and checking
2. **OpenTelemetry** section contains information which using for sending traces and metrics to elasticsearch APM
3. **Serilog** section contains information about elastic search endpoins and logs indexes

**Important!!!:**
After application started index template and search index will be created automatically in kibana. But we should add Data view manually. Please, use **currency-exchange-api-logs-* ** index template for it

# Test Coverage

Test coverage is essential for maintaining the quality and stability of the application. The test suite covers key aspects of the API, including:

- **Unit Tests**: For business logic in services and providers.
- **Integration Tests**: Simulating real-world API calls to verify the entire flow of currency rate retrieval and conversion.

The current test coverage is measured using **xUnit** and **coverlet**. To run the tests locally, you can execute the following command:

```bash
dotnet test
```
or use Visual stusio test explorer.

Test results will provide insights into which areas of the application have been tested and the overall coverage percentage.

```
---
### **Known Issues**

- Ensure Elasticsearch is running before starting the API.
- JWT token expiration is set to 120 minutes, adjust as needed.
- Elasticsearch authentication is disabled in the current configuration for testing purposes.
- All methods in API is GET methods. As improvement we can create models for body and validate it instead of query parameters validation
- Login method (Auth controller) should use basic authorization (something like ClientId, SecretKey) for getting JWT token. 


