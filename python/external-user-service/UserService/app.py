"""
User Service - Python FastAPI Implementation
Equivalent to the .NET shala.api UserService
"""

import os
import logging
from datetime import datetime
from contextlib import asynccontextmanager
import uvicorn
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv('.env')
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.middleware.trustedhost import TrustedHostMiddleware

from api.router import api_router
from database.db_context import init_db
from startup.configurations import configure_services
from startup.middleware import configure_middleware

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S"
)
logger = logging.getLogger(__name__)


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager"""
    print("Starting User Service...")
    print(f"Environment: {os.getenv('ENVIRONMENT', 'development')}")
    print("Loading configurations...")
    
    # Configure services
    await configure_services(app)
    
    print("Initializing database...")
    await init_db()
    print("Database initialized successfully")
    
    print("Configuring application...")
    
    # print("User Service configuration complete! ")
    # print("=" * 50)
    print("User Service is up and running on port 5000")
    # print("=" * 50)
    
    yield
    
    print("Shutting down User Service...")


app = FastAPI(
    title="User Authentication Service API",
    version="1.0.0",
    description="A comprehensive user authentication and management service with JWT tokens, OAuth, MFA, and more",
    lifespan=lifespan
)

# Configure middleware before application startup
configure_middleware(app)

# Include API routes
app.include_router(api_router, prefix="/api/v1")

# Well-known endpoints (no prefix)
from api.wellknown.routes import wellknown_router
app.include_router(wellknown_router)

# Health check endpoint
@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "timestamp": datetime.utcnow().isoformat()}


# Root endpoint
@app.get("/")
async def root():
    """Root endpoint"""
    return {
        "service": "User Authentication Service",
        "version": "1.0.0",
        "status": "running",
        "endpoints": {
            "health": "/health",
            "docs": "/docs",
            "api": "/api",
            "jwks": "/.well-known/jwks.json"
        }
    }


if __name__ == "__main__":
    # Set URLs explicitly
    port = int(os.getenv("PORT", "5000"))
    host = os.getenv("HOST", "0.0.0.0")
    
    print(f"🎯 User Service will run on: http://{host}:{port}")
    print(f"📚 API Documentation available at: http://{host}:{port}/docs")
    print(f"🔑 JWKS endpoint available at: http://{host}:{port}/.well-known/jwks.json")
    
    uvicorn.run(
        "app:app",
        host=host,
        port=port,
        reload=True,
        log_level="info"
    )
