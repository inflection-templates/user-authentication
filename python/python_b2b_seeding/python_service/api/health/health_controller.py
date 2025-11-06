"""
Health check controller.
Similar to the Node.js HealthController implementation.
"""
import time
from fastapi import Request
from api.common.response_handler import ResponseHandler
from config.config import Config


class HealthController:
    """Controller for health check endpoints."""
    
    def __init__(self):
        self.start_time = time.time()
    
    async def health_check(self, request: Request):
        """
        Health check endpoint.
        
        Args:
            request: FastAPI request object
            
        Returns:
            Health check response
        """
        try:
            uptime = time.time() - self.start_time
            
            health_data = {
                "status": "OK",
                "timestamp": time.strftime("%Y-%m-%dT%H:%M:%S.%fZ", time.gmtime()),
                "uptime": uptime,
                "environment": Config.NODE_ENV,
                "service": Config.SERVICE_NAME,
                "version": Config.SERVICE_VERSION
            }
            
            return ResponseHandler.success(
                request,
                "Service is healthy",
                200,
                health_data
            )
        except Exception as error:
            return ResponseHandler.failure(
                request,
                "Health check failed",
                500,
                error
            )

