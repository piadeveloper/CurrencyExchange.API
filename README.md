
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

Visit: [http://localhost:8080/api/v1/version/getversion](http://localhost:8080/api/version/getversion)

endpoint should return assembly api version

---

#### **Application Endpoint examples**
**Auth**
For getting JWT token use this endpoint:
HTTP POST http://localhost:8080/api/v1/Auth/login?userName=Igor.P&userSecret=secret
userName - mandatory parameter. Random user name. If you send invalid_key as userName you will receive HTTP 401 (for test purposes)
userSecret - mandatory parameter.
Endpoint returns JWT token, which can be used as Bearer token with all enpoint below

Reponse example:
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJJZ29yLlAiLCJqdGkiOiJlNWExOTRjZS03Y2RkLTRkZGYtODViNC05NTg4ODNkNmMwY2UiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiSWdvci5QIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsImV4cCI6MTc0NzgyMzA2MSwiaXNzIjoiQ3VycmVuY3lFeGNoYW5nZUFQSSIsImF1ZCI6IkN1cnJlbmN5RXhjaGFuZ2VDbGllbnRzIn0.an3O0ddPVCVhU3RM374b4UnZaJu2Qld7PoL1XmbTVsE",
    "expiresAt": "2025-05-21T07:24:21.5015351Z"
}

**V1**
1. HTTP GET http://localhost:8080/api/v1/CurrencyConverter/latest?provider=Frankfurter&baseCurrency=USD&amount=1 - return last retes for base currency

2. HTTP GET http://localhost:8080/api/v1/CurrencyConverter/timeseries/2021-01-01/2022-12-31?provider=Frankfurter&baseCurrency=USD&amount=1&page=5&pageSize=25 - return currency rates from 2021-01-01 to 2022-12-31. Information will start from 5 page, page will contain 25 items (dates) 

3. HTTP GET http://localhost:8080/api/v1/CurrencyConverter/historical/2021-01-01?provider=Frankfurter&baseCurrency=USD&amount=1&page=1&pageSize=5 - return currency rates for 2021-01-01. Information will start from 1 page, page will contain 5 items (rates) 

**V2**
1. HTTP POST http://localhost:8080/api/v2/CurrencyConverter/latest - return last retes for base currency
{
 "provider": "Frankfurter" // if leave this parameter empty, "Frankfurter" would be used by default
 "baseCurrency": USD
 "amount": 1 // min value 0.01
}

2. HTTP POST http://localhost:8080/api/v2/CurrencyConverter/timeseries - return currency rates between requested dates
{
 "startDate": "2021-01-01", // date is mandatory
 "endDate": "2022-01-01", // date is mandatory
 "provider":"Frankfurter", // if leave this parameter empty, "Frankfurter" would be used by default
 "baseCurrency":"USD",
 "amount": 1, // min value 0.01
 "page": 1, // Information will start from 1 page
 "pageSize": 5	// Page will contain 5 items (rates) 
}

3. HTTP POST http://localhost:8080/api/v2/CurrencyConverter/historical - return currency rates for requested date. 
{
 "date": "2021-01-01", // date is mandatory
 "provider":"Frankfurter", // if leave this parameter empty, "Frankfurter" would be used by default
 "baseCurrency":"USD",
 "amount": 1, // min value 0.01
 "page": 1, // Information will start from 1 page
 "pageSize": 5	// Page will contain 5 items (rates) 
}


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
dotnet test -v diag /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=../TestResults/
```
Test coverage report can be generated by 

```bash
reportgenerator -reports:"TestResults/coverage.cobertura.xml" -targetdir:"TestResults/Report" -reporttypes:Html
```

In the TestResults/Report folder you can find index.html file and open it for test coverage research.

if reportgenerator is not installed, please install it by

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool  
```

Also you can use Visual stusio test explorer.

Test results will provide insights into which areas of the application have been tested and the overall coverage percentage.

```
---
### **Known Issues**

- Ensure Elasticsearch is running before starting the API.
- JWT token expiration is set to 120 minutes, adjust as needed.
- Elasticsearch authentication is disabled in the current configuration for testing purposes.
- All methods in API is GET methods. As improvement we can create models for body and validate it instead of query parameters validation
- Login method (Auth controller) should use basic authorization (something like ClientId, SecretKey) for getting JWT token. 


