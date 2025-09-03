# Order Management Service - Python FastAPI

A Python FastAPI implementation of the Order Management Service, equivalent to the .NET OrderService.

## Features

- **Customer Management**: CRUD operations for customers
- **Product Management**: CRUD operations for products with inventory tracking
- **Order Management**: Complete order processing with order items
- **JWT Authentication**: Validates tokens from User Service
- **Database**: SQLite with SQLAlchemy async support
- **API Documentation**: Auto-generated OpenAPI/Swagger docs

## Architecture

```
OrderService/
├── app.py                 # Main FastAPI application
├── models/
│   ├── entities.py        # SQLAlchemy database models
│   └── dtos.py           # Pydantic request/response models
├── services/
│   ├── customer_service.py
│   ├── product_service.py
│   └── order_service.py
├── database/
│   └── db_context.py     # Database configuration and seeding
├── appsettings.json      # Configuration
└── requirements.txt      # Python dependencies
```

## Setup and Installation

1. **Install Dependencies**:
   ```bash
   pip install -r requirements.txt
   ```

2. **Environment Variables** (optional):
   ```bash
   export DATABASE_URL="sqlite+aiosqlite:///./order_service.db"
   export JWT_AUTHORITY="http://localhost:5000"
   export JWT_AUDIENCE="shala"
   ```

3. **Run the Service**:
   ```bash
   python app.py
   ```
   Or using uvicorn directly:
   ```bash
   uvicorn app:app --host 0.0.0.0 --port 5001 --reload
   ```

## Service URLs

- **API Base**: http://localhost:5001/api
- **Health Check**: http://localhost:5001/health
- **API Documentation**: http://localhost:5001/docs
- **OpenAPI Schema**: http://localhost:5001/openapi.json

## API Endpoints

### Customers
- `GET /api/customers` - Get all customers
- `GET /api/customers/{id}` - Get customer by ID
- `POST /api/customers` - Create customer
- `PUT /api/customers/{id}` - Update customer
- `DELETE /api/customers/{id}` - Delete customer

### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### Orders
- `GET /api/orders` - Get all orders
- `GET /api/orders/{id}` - Get order by ID
- `POST /api/orders` - Create order
- `PUT /api/orders/{id}` - Update order
- `DELETE /api/orders/{id}` - Delete order
- `PUT /api/orders/{id}/status` - Update order status

## Authentication

All endpoints (except `/health`) require JWT authentication. Include the Bearer token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

The service validates tokens against the User Service JWKS endpoint.

## Database

The service uses SQLite by default with automatic database initialization and seeding. Initial data includes:

- 2 sample customers
- 3 sample products (Laptop, Mouse, Office Chair)

## Error Handling

The service provides comprehensive error handling with appropriate HTTP status codes:

- `400 Bad Request` - Validation errors, business rule violations
- `401 Unauthorized` - Authentication failures
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Unexpected server errors

## Logging

Structured logging is configured with timestamps and log levels. Logs include:

- Service startup and configuration
- Request processing
- Authentication events
- Business operations (create, update, delete)
- Error conditions

## Development

For development, the service includes:

- Auto-reload on code changes
- Detailed error messages
- SQL query logging (in debug mode)
- CORS enabled for all origins

## Production Considerations

For production deployment:

1. Use a production database (PostgreSQL, MySQL)
2. Configure proper logging levels
3. Set up environment-specific configuration
4. Implement proper error handling and monitoring
5. Configure CORS for specific origins only
6. Use HTTPS and proper security headers
