# Postman Setup Guide for Order Service

## 🚀 Quick Setup (5 minutes)

### Step 1: Create New Collection
1. Open Postman
2. Click **"New"** → **"Collection"**
3. Name it: **"Order Management Service API"**

### Step 2: Create Environment
1. Click **"Environments"** in left sidebar
2. Click **"New"** → **"Environment"**
3. Name it: **"Order Service Environment"**
4. Add these variables:
   - `user_service_url`: `http://localhost:5000`
   - `order_service_url`: `https://localhost:7000`
   - `jwt_token`: (leave empty)

### Step 3: Add Authentication Request
1. In your collection, click **"Add Request"**
2. Name: **"Get JWT Token"**
3. Method: **POST**
4. URL: `{{user_service_url}}/api/users/auth/login-with-password`
5. Headers: `Content-Type: application/json`
6. Body (raw JSON):
```json
{
  "email": "admin@example.com",
  "password": "your-password"
}
```
7. In **Tests** tab, add this script:
```javascript
if (pm.response.code === 200) {
    const response = pm.response.json();
    pm.environment.set('jwt_token', response.token);
    console.log('JWT Token saved:', response.token);
}
```

### Step 4: Add Health Check
1. **Add Request** → Name: **"Health Check"**
2. Method: **GET**
3. URL: `{{order_service_url}}/health`

### Step 5: Add Customer Requests
1. **Add Request** → Name: **"Get All Customers"**
2. Method: **GET**
3. URL: `{{order_service_url}}/api/customers`
4. Headers: `Authorization: Bearer {{jwt_token}}`

### Step 6: Add Product Requests
1. **Add Request** → Name: **"Get All Products"**
2. Method: **GET**
3. URL: `{{order_service_url}}/api/products`
4. Headers: `Authorization: Bearer {{jwt_token}}`

### Step 7: Add Order Requests
1. **Add Request** → Name: **"Get All Orders"**
2. Method: **GET**
3. URL: `{{order_service_url}}/api/orders`
4. Headers: `Authorization: Bearer {{jwt_token}}`

## 🧪 Testing Steps

1. **Select Environment**: Choose "Order Service Environment"
2. **Get Token**: Run "Get JWT Token" request
3. **Test Health**: Run "Health Check" request
4. **Test APIs**: Run other requests (they'll use the saved token)

## 📝 Sample Request Bodies

### Create Customer
```json
{
  "firstName": "Alice",
  "lastName": "Johnson",
  "email": "alice.johnson@example.com",
  "phone": "+1234567892",
  "address": "789 Pine St, City, State 12347"
}
```

### Create Product
```json
{
  "name": "Gaming Keyboard",
  "description": "Mechanical gaming keyboard with RGB lighting",
  "price": 89.99,
  "sku": "KEYBOARD-001",
  "stockQuantity": 100,
  "category": "Electronics"
}
```

### Create Order
```json
{
  "customerId": 1,
  "notes": "Please handle with care",
  "shippingAddress": "123 Main St, City, State 12345",
  "orderItems": [
    {
      "productId": 1,
      "quantity": 1
    },
    {
      "productId": 2,
      "quantity": 2
    }
  ]
}
```

## 🔑 Important Notes

- Make sure both services are running:
  - User Service: `http://localhost:5000`
  - Order Service: `https://localhost:7000`
- Always run "Get JWT Token" first
- The token is automatically saved and used in other requests
- Health check doesn't require authentication
