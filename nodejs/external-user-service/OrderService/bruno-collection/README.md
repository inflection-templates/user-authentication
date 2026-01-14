# Order Service API - Bruno Collection

This Bruno collection contains all API endpoints for the Node.js TypeScript Order Management Service.

## Setup Instructions

1. **Install Bruno**: Download and install [Bruno](https://usebruno.com/) from the official website.

2. **Open Collection**: 
   - Launch Bruno
   - Click "Open Collection"
   - Navigate to this `bruno-collection` folder
   - Select the folder to load the collection

3. **Configure Environment**:
   - Go to Environments → Local
   - Update the `jwt_token` variable with a valid JWT token from your User Service
   - Ensure `base_url` points to your running Order Service (default: http://localhost:5001)

## Environment Variables

### Local Environment
- `base_url`: http://localhost:5001
- `jwt_token`: Your JWT token from the User Service

## API Endpoints

### Health Check
- **GET** `/health` - Service health check (no authentication required)

### Customers
- **GET** `/api/customers` - Get all customers
- **GET** `/api/customers/{id}` - Get customer by ID
- **POST** `/api/customers` - Create new customer
- **PUT** `/api/customers/{id}` - Update customer
- **DELETE** `/api/customers/{id}` - Delete customer

### Products
- **GET** `/api/products` - Get all products
- **GET** `/api/products/{id}` - Get product by ID
- **POST** `/api/products` - Create new product
- **PUT** `/api/products/{id}` - Update product
- **DELETE** `/api/products/{id}` - Delete product (soft delete)

### Orders
- **GET** `/api/orders` - Get all orders
- **GET** `/api/orders/{id}` - Get order by ID (includes customer and order items)
- **POST** `/api/orders` - Create new order with items
- **PUT** `/api/orders/{id}` - Update order
- **PUT** `/api/orders/{id}/status` - Update order status only
- **DELETE** `/api/orders/{id}` - Delete order (only pending orders)

## Authentication

All API endpoints (except health check) require JWT authentication:

```
Authorization: Bearer <your-jwt-token>
```

### Getting a JWT Token

**IMPORTANT**: This OrderService requires JWT tokens with RSA signatures and `kid` (Key ID) in the header, exactly like the Python OrderService.

#### 🚀 Quick Start (Recommended)
1. **Use the Authentication folder** in this Bruno collection
2. **Run "Login to User Service"** - automatically gets and sets JWT token
3. **Run "Test Full Auth Flow"** - verifies everything works
4. **Use any OrderService endpoint** - authentication is now set up!

#### 📋 Manual Steps
1. **Start Python User Service** on `http://localhost:5000`
2. **Login via Python User Service** to get a proper JWT token with `kid`
3. **Set the token** in Bruno's Local environment as `jwt_token`

#### ❌ Node.js User Service Not Compatible
The Node.js User Service uses HMAC tokens without `kid`, which won't work with this OrderService. You must use the Python User Service for proper JWKS support.

#### 🔍 Token Validation Process
The Order Service will:
- Extract `kid` from JWT header
- Fetch the corresponding public key from JWKS endpoint (`/.well-known/jwks.json`)
- Verify the token signature using RSA public key
- Check token expiration, issuer, and audience claims

See the **Authentication folder** for detailed workflow and troubleshooting.

### Environment Variables Required

Make sure your Order Service `.env` file includes:
```env
JWT_AUTHORITY=http://localhost:5000
JWT_AUDIENCE=shala
JWT_JWKS_URL=http://localhost:5000/.well-known/jwks.json
```

## Order Statuses

Valid order statuses:
- `Pending` - Initial status
- `Processing` - Order is being processed
- `Shipped` - Order has been shipped
- `Delivered` - Order has been delivered
- `Cancelled` - Order has been cancelled

## Sample Data

The service automatically seeds with:
- 2 sample customers
- 3 sample products (Laptop, Mouse, Office Chair)

## Testing Workflow

1. **Health Check**: Verify service is running
2. **Get All Products**: See available products (seeded data)
3. **Get All Customers**: See available customers (seeded data)
4. **Create Customer**: Create a new customer (stores ID for later use)
5. **Create Order**: Create an order with the new customer and existing products
6. **Update Order Status**: Change order status through the workflow
7. **Get Order**: Verify order details with customer and items included

## Variables

The collection uses variables to chain requests:
- `customer_id` - Automatically set when creating a customer
- `product_id` - Automatically set when creating a product
- `order_id` - Automatically set when creating an order

## Error Handling

The API returns standardized error responses:

```json
{
  "success": false,
  "message": "Error description",
  "httpcode": 400
}
```

Common HTTP status codes:
- `200` - Success
- `201` - Created
- `400` - Bad Request (validation errors)
- `401` - Unauthorized (invalid/missing JWT)
- `404` - Not Found
- `409` - Conflict (unique constraint violations - e.g., duplicate email)
- `500` - Internal Server Error

### Common Issues

**409 Conflict Error when creating customers:**
- This happens when trying to create a customer with an email that already exists
- The collection uses `{{$timestamp}}` to generate unique emails
- Alternative: Use the "Create Customer - Alternative" request with random data
- Check existing customers using "Debug/Check Existing Customers" request

## Notes

- All timestamps are in UTC format
- Prices are stored as decimal numbers
- UUIDs are used for customer IDs
- Integer IDs are used for products, orders, and order items
- Order calculations include tax (8%) and shipping ($10 under $100, free over $100)
