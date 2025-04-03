# CurrencyConverter
The Currency Converter is a high-performance application designed to provide real-time currency exchange rates using the Frankfurter API. Built with Clean Architecture, this project ensures maintainability, and scalability.

<!-- # Currency Converter

This project implements a robust, scalable, and maintainable currency conversion API using .NET 8 with features like caching, circuit breaker, retry policy, API throttling, structured logging, and JWT-based authentication. -->

## Setup Instructions
   
   **Prerequisites**
 - .NET 8 SDK
 - Redis
 - SwaggerUI

## Configuration Steps
1-Clone the Repository
  
    https://github.com/MWaqasRafiq/CurrencyConverter.git

 
2- Update the defualt values in  appsettings.json (as per the requirement)-optional:
```json{
  "JwtSettings": {
    "Issuer": "https://yourdomain.com",
    "Audience": "https://yourdomain.com",
    "SecretKey": "your-256-bit-secret-key"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "ApiThrottling": {
    "RateLimit": 15,
    "TimeWindowInSeconds": 60
  }
}
```
3- Install Redis Server on local machine(Required)
 - https://redis.io/docs/latest/operate/oss_and_stack/install/install-redis/
           
4- configure and running-up redis server (Required)

5- Build & Run the Application

6- Access the API using Swagger
 - Swagger UI: [Swagger UI](https://localhost:{port}/swagger/index.html)

## API Features Implemented

 -  **Retrieve Latest Exchange Rates**
    
    -   Fetch the latest exchange rates for a specific base currency.
        
 -  **Currency Conversion**
    
    -   Convert amounts between different currencies while excluding specific currencies.
        
 -  **Historical Exchange Rates with Pagination**
    
    -   Retrieve historical exchange rates for a given period with pagination.
        
 -  **Caching with Redis**
    
    -   Minimize direct calls to external API with cached data.
        
 -  **Resilience and Performance**
    
    -   Retry Policies with Exponential Backoff.
        
    -   Circuit Breaker for handling API outages.
        
 -  **Security**
    
    -   JWT Authentication.
        
    -   Role-Based Access Control (RBAC).
    -   Roles: Admin, Guest
        
 -  **Rate Limiting Middleware**
    
    -   Implemented with in-memory caching.
        
 -  **Structured Logging with Serilog**
    
    -   Logs client IP, JWT ClientId, method, endpoint, response code, and response time.
        
    -   OpenTelemetry for distributed tracing.
        
 -  **API Versioning**
    
    -   Versioned routes for backward compatibility.
   

 ## Assumptions Made
 -  **Exchange Rate Provider** 
    -   The Frankfurter API is used as the primary exchange rate provider.
 -  **CI/CD Integration** 
     - we used azure app service for deployment so create yml file
   accordingly
   
 ## Possible Future Enhancements
 -  **Database Persistence**    
    - Store historical rates in a relational database for long-term
   analysis.
 - **Monitoring Dashboard** 
     - Integrate with tools like Grafana for real-time monitoring of API
   usage and performance.
 -  **Multi-Provider Support** 
    - Extend the CurrencyProviderFactory to support multiple exchange rate
   providers.
   -  **Testing** 
    - Use NUnit for unit testing and Jmeter for stress testing.

## Sample Request Body

**Headers:**
- `cliendid`: frankfruttestingclient

**Request Body:**
```json
For Guest : { "username": "guest", "password": "12345" }
For Admin : { "username": "admin", "password": "12345" }
```