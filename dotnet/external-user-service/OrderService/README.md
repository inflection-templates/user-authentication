# Order Management Service

A minimal API service for order management with CRUD operations for Customers, Products, Orders, and OrderItems. This service integrates with the User Service for authentication using JWKS (JSON Web Key Set).

## Features

- **4 Core Entities**: Customer, Product, Order, OrderItem
- **CRUD Operations**: Full Create, Read, Update, Delete operations for all entities
- **JWT Authentication**: Integrates with User Service using JWKS endpoint
- **Minimal API**: Built with .NET 8 Minimal API
- **Entity Framework Core**: SQL Server database with code-first approach
- **Swagger Documentation**: Auto-generated API documentation
- **Logging**: Structured logging with Serilog

## Architecture

### Entities

1. **Customer**: Customer information and contact details
2. **Product**: Product catalog with inventory management
3. **Order**: Order header with customer and totals
4. **OrderItem**: Individual items within an order

### Authentication Flow

```
Order Service → Validates JWT Token → User Service JWKS Endpoint
```

The Order Service validates JWT tokens by fetching public keys from the User Service's JWKS endpoint at `/.well-known/jwks.json`.

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- User Service running on `http://localhost:5000`

### Configuration

1. Update `appsettings.json` with your database connection string
2. Ensure the User Service is running and accessible at the configured `Jwt.Authority` URL
3. The service will automatically fetch public keys from the User Service's JWKS endpoint

### Running the Service

```bash
cd OrderService
dotnet run
```

The service will be available at `https://localhost:7000` (or the configured port).

### Database Setup

The service uses Entity Framework Core with code-first migrations. The database will be created automatically on first run with seed data.

## API Endpoints

### Customers
- `GET /api/customers` - Get all customers
- `GET /api/customers/{id}` - Get customer by ID
- `POST /api/customers` - Create new customer
- `PUT /api/customers/{id}` - Update customer
- `DELETE /api/customers/{id}` - Delete customer

### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### Orders
- `GET /api/orders` - Get all orders
- `GET /api/orders/{id}` - Get order by ID
- `POST /api/orders` - Create new order
- `PUT /api/orders/{id}` - Update order
- `DELETE /api/orders/{id}` - Delete order
- `PUT /api/orders/{id}/status` - Update order status

### Health Check
- `GET /health` - Service health status

## Authentication

All API endpoints (except health check) require JWT authentication. Include the JWT token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

To get a JWT token, authenticate with the User Service first.

## Swagger Documentation

When running in development mode, Swagger UI is available at:
- `https://localhost:7000/swagger`

## Database Schema

The service creates the following tables:
- `Customers` - Customer information
- `Products` - Product catalog
- `Orders` - Order headers
- `OrderItems` - Order line items

## Sample Data

The service includes seed data with:
- 2 sample customers
- 3 sample products (Laptop, Mouse, Office Chair)

## Error Handling

The API returns appropriate HTTP status codes:
- `200 OK` - Successful operation
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Missing or invalid JWT token
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

## Logging

Logs are written to:
- Console (development)
- File: `logs/order-service-{date}.txt`

## Integration with User Service

The Order Service integrates with the User Service for authentication:

1. User Service exposes JWKS endpoint at `/.well-known/jwks.json`
2. Order Service fetches public keys from this endpoint
3. JWT tokens issued by User Service are validated using these public keys
4. No shared secrets required between services

This provides a secure, scalable authentication mechanism for microservices architecture.
