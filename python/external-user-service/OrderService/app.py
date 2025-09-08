"""
Order Management Service - Python FastAPI Implementation
Equivalent to the .NET OrderService
"""

import os
import logging
from datetime import datetime
from typing import List
import uvicorn
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv('.env')
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager

from database.db_context import init_db
from api.customer.routes import router as customer_router
from api.product.routes import router as product_router
from api.order.routes import router as order_router

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format="[%(asctime)s %(levelname)s] %(message)s",
    datefmt="%H:%M:%S"
)
logger = logging.getLogger(__name__)


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager"""
    logger.info("Starting Order Management Service...")
    logger.info("Environment: {os.getenv('ENVIRONMENT', 'development')}")
    logger.info("User Service URL: {os.getenv('JWT_AUTHORITY', 'http://localhost:5000')}")
    logger.info("Initializing database...")
    
    # Initialize database
    await init_db()
    logger.info("Database initialized successfully")
    
    logger.info("Order Management Service is ready!")   
    logger.info("Order Service is running and listening on port 5001")
    
    yield
    
    logger.info("Shutting down Order Management Service...")

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

# Include routers
app.include_router(customer_router)
app.include_router(product_router)
app.include_router(order_router)

# Health check endpoint
@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "timestamp": datetime.utcnow().isoformat()}

if __name__ == "__main__":
    uvicorn.run(
        "app:app",
        host=os.getenv("HOST", "0.0.0.0"),
        port=int(os.getenv("PORT", "5001")),
        reload=True,
        log_level=os.getenv("LOG_LEVEL", "info").lower()
    )
