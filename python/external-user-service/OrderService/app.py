"""
Order Management Service - Python FastAPI Implementation
Equivalent to the .NET OrderService
"""

import os
import logging
from datetime import datetime, timedelta
from typing import List, Optional
import uvicorn
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv('.env')
from fastapi import FastAPI, HTTPException, Depends, status
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager
import jwt
import httpx
from cryptography.hazmat.primitives import serialization
from cryptography.hazmat.primitives.asymmetric import rsa

from database.db_context import get_db_session, init_db
from models.entities import Customer, Product, Order, OrderItem
from models.dtos import (
    CustomerDto, CreateCustomerDto, UpdateCustomerDto,
    ProductDto, CreateProductDto, UpdateProductDto,
    OrderDto, CreateOrderDto, UpdateOrderDto
)
from services.customer_service import CustomerService
from services.product_service import ProductService
from services.order_service import OrderService as OrderServiceImpl

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format="[%(asctime)s %(levelname)s] %(message)s",
    datefmt="%H:%M:%S"
)
logger = logging.getLogger(__name__)

# JWT Configuration
JWT_AUTHORITY = os.getenv("JWT_AUTHORITY", "http://localhost:5000")
JWT_AUDIENCE = os.getenv("JWT_AUDIENCE", "shala")
JWKS_URL = os.getenv("JWT_JWKS_URL", f"{JWT_AUTHORITY}/.well-known/jwks.json")

security = HTTPBearer()


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager"""
    logger.info("🚀 Starting Order Management Service...")
    logger.info(f"📊 Environment: {os.getenv('ENVIRONMENT', 'development')}")
    logger.info(f"🔗 User Service URL: {JWT_AUTHORITY}")
    logger.info("🗄️ Initializing database...")
    
    # Initialize database
    await init_db()
    logger.info("✅ Database initialized successfully")
    
    logger.info("🎉 Order Management Service is ready!")
    logger.info("🌐 Service URLs:")
    logger.info("   • Health Check: http://localhost:5001/health")
    logger.info("   • API Docs: http://localhost:5001/docs")
    logger.info("   • API Base: http://localhost:5001/api")
    logger.info("🔐 JWT Authentication configured with User Service")
    logger.info("📋 Available endpoints:")
    logger.info("   • GET /api/customers - Get all customers")
    logger.info("   • GET /api/products - Get all products")
    logger.info("   • GET /api/orders - Get all orders")
    logger.info("   • POST /api/customers - Create customer")
    logger.info("   • POST /api/products - Create product")
    logger.info("   • POST /api/orders - Create order")
    logger.info("=" * 50)
    logger.info("🚀 Order Service is now running and ready to accept requests!")
    logger.info("=" * 50)
    
    yield
    
    logger.info("🛑 Shutting down Order Management Service...")


app = FastAPI(
    title="Order Management Service API",
    version="1.0.0",
    description="A FastAPI service for order management with CRUD operations for Customers, Products, Orders, and OrderItems",
    lifespan=lifespan
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


async def get_jwks_key(kid: str) -> Optional[str]:
    """Fetch JWKS key from User Service"""
    try:
        logger.info(f"Fetching JWKS from: {JWKS_URL}")
        async with httpx.AsyncClient() as client:
            response = await client.get(JWKS_URL)
            response.raise_for_status()
            jwks = response.json()
            
            logger.info(f"JWKS response received. Looking for key with kid: {kid}")
            logger.info(f"Available keys in JWKS: {len(jwks.get('keys', []))}")
            
            for key in jwks.get("keys", []):
                logger.info(f"Available key: Kid={key.get('kid')}")
                if key.get("kid") == kid:
                    logger.info(f"Found matching key for kid: {kid}")
                    return key
            
            logger.warning(f"No matching key found for kid: {kid}")
            return None
            
    except Exception as ex:
        logger.error(f"Error fetching JWKS from UserService: {ex}")
        return None


async def verify_token(credentials: HTTPAuthorizationCredentials = Depends(security)):
    """Verify JWT token from User Service"""
    try:
        token = credentials.credentials
        
        # Decode token header to get kid
        unverified_header = jwt.get_unverified_header(token)
        kid = unverified_header.get("kid")
        
        if not kid:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Token missing key ID"
            )
        
        # Get public key from JWKS
        jwks_key = await get_jwks_key(kid)
        if not jwks_key:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Unable to find appropriate key"
            )
        
        # Convert JWKS key to RSA public key
        from cryptography.hazmat.primitives.asymmetric.rsa import RSAPublicNumbers
        from cryptography.hazmat.backends import default_backend
        import base64
        
        def base64url_decode(input_str):
            """Decode base64url string"""
            padding = 4 - len(input_str) % 4
            if padding != 4:
                input_str += '=' * padding
            return base64.urlsafe_b64decode(input_str)
        
        n = int.from_bytes(base64url_decode(jwks_key["n"]), byteorder="big")
        e = int.from_bytes(base64url_decode(jwks_key["e"]), byteorder="big")
        
        public_key = RSAPublicNumbers(e, n).public_key(default_backend())
        
        # Verify token
        payload = jwt.decode(
            token,
            public_key,
            algorithms=["RS256"],
            issuer=JWT_AUDIENCE,
            audience=JWT_AUDIENCE,
            options={"verify_exp": True, "verify_iss": True, "verify_aud": True}
        )
        
        logger.info(f"Token verified successfully for user: {payload.get('sub')}")
        return payload
        
    except jwt.ExpiredSignatureError:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Token has expired"
        )
    except jwt.JWTError as e:
        logger.error(f"JWT verification failed: {e}")
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Could not validate credentials"
        )
    except Exception as e:
        logger.error(f"Token verification error: {e}")
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Token verification failed"
        )


# Health check endpoint
@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "timestamp": datetime.utcnow().isoformat()}


# Customer endpoints
@app.get("/api/v1/customers", response_model=List[CustomerDto])
async def get_all_customers(
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Get all customers"""
    service = CustomerService(db_session)
    return await service.get_all_customers()


@app.get("/api/v1/customers/{customer_id}", response_model=CustomerDto)
async def get_customer_by_id(
    customer_id: int,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Get customer by ID"""
    service = CustomerService(db_session)
    customer = await service.get_customer_by_id(customer_id)
    if not customer:
        raise HTTPException(status_code=404, detail="Customer not found")
    return customer


@app.post("/api/v1/customers", response_model=CustomerDto, status_code=201)
async def create_customer(
    customer_data: CreateCustomerDto,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Create a new customer"""
    try:
        service = CustomerService(db_session)
        return await service.create_customer(customer_data)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


@app.put("/api/v1/customers/{customer_id}", response_model=CustomerDto)
async def update_customer(
    customer_id: int,
    customer_data: UpdateCustomerDto,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Update a customer"""
    try:
        service = CustomerService(db_session)
        customer = await service.update_customer(customer_id, customer_data)
        if not customer:
            raise HTTPException(status_code=404, detail="Customer not found")
        return customer
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


# Legacy API endpoints for backward compatibility (without /v1)
@app.get("/api/customers", response_model=List[CustomerDto])
async def get_all_customers_legacy(
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Get all customers (legacy endpoint)"""
    service = CustomerService(db_session)
    return await service.get_all_customers()


@app.get("/api/customers/{customer_id}", response_model=CustomerDto)
async def get_customer_by_id_legacy(
    customer_id: int,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Get customer by ID (legacy endpoint)"""
    service = CustomerService(db_session)
    customer = await service.get_customer_by_id(customer_id)
    if not customer:
        raise HTTPException(status_code=404, detail="Customer not found")
    return customer


@app.post("/api/customers", response_model=CustomerDto, status_code=201)
async def create_customer_legacy(
    customer_data: CreateCustomerDto,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Create a new customer (legacy endpoint)"""
    try:
        service = CustomerService(db_session)
        return await service.create_customer(customer_data)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


@app.put("/api/customers/{customer_id}", response_model=CustomerDto)
async def update_customer_legacy(
    customer_id: int,
    customer_data: UpdateCustomerDto,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Update a customer (legacy endpoint)"""
    try:
        service = CustomerService(db_session)
        customer = await service.update_customer(customer_id, customer_data)
        if not customer:
            raise HTTPException(status_code=404, detail="Customer not found")
        return customer
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


@app.delete("/api/customers/{customer_id}", status_code=204)
async def delete_customer_legacy(
    customer_id: int,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Delete a customer (legacy endpoint)"""
    try:
        service = CustomerService(db_session)
        success = await service.delete_customer(customer_id)
        if not success:
            raise HTTPException(status_code=404, detail="Customer not found")
    except RuntimeError as e:
        raise HTTPException(status_code=400, detail=str(e))


@app.delete("/api/v1/customers/{customer_id}", status_code=204)
async def delete_customer(
    customer_id: int,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Delete a customer"""
    try:
        service = CustomerService(db_session)
        success = await service.delete_customer(customer_id)
        if not success:
            raise HTTPException(status_code=404, detail="Customer not found")
    except RuntimeError as e:
        raise HTTPException(status_code=400, detail=str(e))


# Product endpoints
@app.get("/api/v1/products", response_model=List[ProductDto])
async def get_all_products(
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Get all products"""
    service = ProductService(db_session)
    return await service.get_all_products()


@app.get("/api/v1/products/{product_id}", response_model=ProductDto)
async def get_product_by_id(
    product_id: int,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Get product by ID"""
    service = ProductService(db_session)
    product = await service.get_product_by_id(product_id)
    if not product:
        raise HTTPException(status_code=404, detail="Product not found")
    return product


@app.post("/api/v1/products", response_model=ProductDto, status_code=201)
async def create_product(
    product_data: CreateProductDto,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Create a new product"""
    try:
        service = ProductService(db_session)
        return await service.create_product(product_data)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


@app.put("/api/v1/products/{product_id}", response_model=ProductDto)
async def update_product(
    product_id: int,
    product_data: UpdateProductDto,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Update a product"""
    try:
        service = ProductService(db_session)
        product = await service.update_product(product_id, product_data)
        if not product:
            raise HTTPException(status_code=404, detail="Product not found")
        return product
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


@app.delete("/api/v1/products/{product_id}", status_code=204)
async def delete_product(
    product_id: int,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Delete a product"""
    try:
        service = ProductService(db_session)
        success = await service.delete_product(product_id)
        if not success:
            raise HTTPException(status_code=404, detail="Product not found")
    except RuntimeError as e:
        raise HTTPException(status_code=400, detail=str(e))


# Order endpoints
@app.get("/api/v1/orders", response_model=List[OrderDto])
async def get_all_orders(
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Get all orders"""
    service = OrderServiceImpl(db_session)
    return await service.get_all_orders()


@app.get("/api/v1/orders/{order_id}", response_model=OrderDto)
async def get_order_by_id(
    order_id: int,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Get order by ID"""
    service = OrderServiceImpl(db_session)
    order = await service.get_order_by_id(order_id)
    if not order:
        raise HTTPException(status_code=404, detail="Order not found")
    return order


@app.post("/api/v1/orders", response_model=OrderDto, status_code=201)
async def create_order(
    order_data: CreateOrderDto,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Create a new order"""
    try:
        service = OrderServiceImpl(db_session)
        return await service.create_order(order_data)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


@app.put("/api/v1/orders/{order_id}", response_model=OrderDto)
async def update_order(
    order_id: int,
    order_data: UpdateOrderDto,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Update an order"""
    try:
        service = OrderServiceImpl(db_session)
        order = await service.update_order(order_id, order_data)
        if not order:
            raise HTTPException(status_code=404, detail="Order not found")
        return order
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


@app.delete("/api/v1/orders/{order_id}", status_code=204)
async def delete_order(
    order_id: int,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Delete an order"""
    try:
        service = OrderServiceImpl(db_session)
        success = await service.delete_order(order_id)
        if not success:
            raise HTTPException(status_code=404, detail="Order not found")
    except RuntimeError as e:
        raise HTTPException(status_code=400, detail=str(e))


@app.put("/api/v1/orders/{order_id}/status")
async def update_order_status(
    order_id: int,
    status_update: dict,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Update order status"""
    try:
        service = OrderServiceImpl(db_session)
        order = await service.update_order_status(order_id, status_update.get("status"))
        if not order:
            raise HTTPException(status_code=404, detail="Order not found")
        return order
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


if __name__ == "__main__":
    uvicorn.run(
        "app:app",
        host=os.getenv("HOST", "0.0.0.0"),
        port=int(os.getenv("PORT", "5001")),
        reload=True,
        log_level=os.getenv("LOG_LEVEL", "info").lower()
    )
