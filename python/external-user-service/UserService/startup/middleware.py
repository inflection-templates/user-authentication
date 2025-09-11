"""
Middleware configuration
Equivalent to the .NET shala.api.startup.middlewares
"""

import logging
from fastapi import FastAPI, Request, Response
from fastapi.middleware.cors import CORSMiddleware
from fastapi.middleware.trustedhost import TrustedHostMiddleware
from starlette.middleware.base import BaseHTTPMiddleware
import time
import uuid

logger = logging.getLogger(__name__)


class RequestLoggingMiddleware(BaseHTTPMiddleware):
    """Middleware for request logging"""
    
    async def dispatch(self, request: Request, call_next):
        # Generate request ID
        request_id = str(uuid.uuid4())[:8]
        request.state.request_id = request_id
        
        # Log request start
        start_time = time.time()
        logger.info(f"[{request_id}] {request.method} {request.url.path} - Started")
        
        # Process request
        try:
            response = await call_next(request)
            
            # Log request completion
            process_time = time.time() - start_time
            logger.info(
                f"[{request_id}] {request.method} {request.url.path} - "
                f"Completed {response.status_code} in {process_time:.3f}s"
            )
            
            # Add request ID to response headers
            response.headers["X-Request-ID"] = request_id
            return response
            
        except Exception as e:
            process_time = time.time() - start_time
            logger.error(
                f"[{request_id}] {request.method} {request.url.path} - "
                f"Failed in {process_time:.3f}s: {str(e)}"
            )
            raise


class SecurityHeadersMiddleware(BaseHTTPMiddleware):
    """Middleware for adding security headers"""
    
    async def dispatch(self, request: Request, call_next):
        response = await call_next(request)
        
        # Add security headers
        response.headers["X-Content-Type-Options"] = "nosniff"
        response.headers["X-Frame-Options"] = "DENY"
        response.headers["X-XSS-Protection"] = "1; mode=block"
        response.headers["Referrer-Policy"] = "strict-origin-when-cross-origin"
        
        # Add CSP header for non-API routes
        if not request.url.path.startswith("/api/"):
            response.headers["Content-Security-Policy"] = (
                "default-src 'self'; "
                "script-src 'self' 'unsafe-inline'; "
                "style-src 'self' 'unsafe-inline'; "
                "img-src 'self' data: https:; "
                "font-src 'self'"
            )
        
        return response


def configure_middleware(app: FastAPI):
    """
    Configure application middleware
    Equivalent to the .NET app.UseConfigs()
    """
    
    # Add CORS middleware
    app.add_middleware(
        CORSMiddleware,
        allow_origins=["*"],  # Configure appropriately for production
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
        expose_headers=["X-Request-ID"]
    )
    
    # Add trusted host middleware (configure for production)
    # app.add_middleware(
    #     TrustedHostMiddleware,
    #     allowed_hosts=["localhost", "127.0.0.1", "*.yourdomain.com"]
    # )
    
    # Add custom middleware
    app.add_middleware(SecurityHeadersMiddleware)
    app.add_middleware(RequestLoggingMiddleware)
    
    logger.info("Middleware configured")
    logger.info("   • CORS enabled for all origins")
    logger.info("   • Request logging enabled")
    logger.info("   • Security headers enabled")
