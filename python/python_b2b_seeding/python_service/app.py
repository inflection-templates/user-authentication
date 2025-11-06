"""
Main application file.
Similar to the Node.js app.ts and index.ts implementation.
"""
from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware
import uvicorn
from contextlib import asynccontextmanager
from config.config import Config
from api.health import health_routes
from api.user import user_routes
from database.database_connector import DatabaseConnector


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager for startup and shutdown."""
    # Startup
    print("🛢️ Setting up the database...")
    DatabaseConnector.setup()
    
    # Run seeding if enabled (similar to dotnet UseSeeder extension)
    if Config.AUTO_SEED_ON_STARTUP:
        from startup.seeder import Seeder
        is_dev = Config.is_development()
        is_test = Config.is_test()
        Seeder.seed_async(is_dev, is_test)
    
    yield
    
    # Shutdown
    DatabaseConnector.close()


# Create FastAPI application
app = FastAPI(
    title=Config.SERVICE_NAME,
    version=Config.SERVICE_VERSION,
    description="Service Skeleton - Python API",
    lifespan=lifespan
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Configure based on your needs
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# Root endpoint
@app.get("/api/v1/")
async def root(request: Request):
    """
    Root endpoint.
    
    @route   GET /api/v1/
    @desc    Root endpoint
    @access  Public
    """
    return JSONResponse(
        status_code=200,
        content={
            "Status": "success",
            "HttpCode": 200,
            "Message": f"{Config.SERVICE_NAME}-[{Config.NODE_ENV}]",
            "Data": None
        }
    )


# Health check endpoint (alternative route)
@app.get("/health-check")
async def health_check_alt(request: Request):
    """
    Health check endpoint (alternative route).
    
    @route   GET /health-check
    @desc    Check service health
    @access  Public
    """
    from api.health.health_controller import HealthController
    controller = HealthController()
    return await controller.health_check(request)


# Include routers
app.include_router(health_routes.router, prefix="/api/v1")
app.include_router(user_routes.router, prefix="/api/v1")


# Exception handlers
@app.exception_handler(404)
async def not_found_handler(request: Request, exc):
    """Handle 404 Not Found errors."""
    return JSONResponse(
        status_code=404,
        content={
            "Status": "failure",
            "HttpCode": 404,
            "Message": "Resource not found",
            "Data": None
        }
    )


@app.exception_handler(500)
async def internal_server_error_handler(request: Request, exc):
    """Handle 500 Internal Server errors."""
    return JSONResponse(
        status_code=500,
        content={
            "Status": "failure",
            "HttpCode": 500,
            "Message": "Internal server error",
            "Data": None
        }
    )


def start():
    """Start the application server."""
    uvicorn.run(
        "app:app",
        host="0.0.0.0",
        port=Config.PORT,
        reload=Config.is_development(),
        log_level="info"
    )


if __name__ == "__main__":
    print(f"🚀 Starting {Config.SERVICE_NAME}...")
    print(f"📊 Environment: {Config.NODE_ENV}")
    print(f"✅ Health check: http://localhost:{Config.PORT}/health-check")
    start()

