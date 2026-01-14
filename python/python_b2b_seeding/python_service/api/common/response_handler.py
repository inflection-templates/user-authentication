"""
Response handler for standardized API responses.
Similar to the Node.js ResponseHandler implementation.
"""
from typing import Optional, Dict, Any
from fastapi import Response, Request
from fastapi.responses import JSONResponse


class ResponseHandler:
    """Handler for standardized API responses."""
    
    @staticmethod
    def success(
        request: Request,
        message: str = "Success",
        status_code: int = 200,
        data: Optional[Any] = None
    ) -> JSONResponse:
        """
        Send a successful response.
        
        Args:
            request: FastAPI request object
            message: Success message
            status_code: HTTP status code
            data: Response data
            
        Returns:
            JSONResponse with success format
        """
        response_data = {
            "Status": "success",
            "HttpCode": status_code,
            "Message": message,
            "Data": data
        }
        return JSONResponse(content=response_data, status_code=status_code)
    
    @staticmethod
    def failure(
        request: Request,
        message: str = "Error",
        status_code: int = 500,
        error: Optional[Any] = None
    ) -> JSONResponse:
        """
        Send a failure/error response.
        
        Args:
            request: FastAPI request object
            message: Error message
            status_code: HTTP status code
            error: Error details
            
        Returns:
            JSONResponse with error format
        """
        response_data = {
            "Status": "failure",
            "HttpCode": status_code,
            "Message": message,
            "Data": None
        }
        
        if error and isinstance(error, Exception):
            if hasattr(error, 'message'):
                response_data["Error"] = str(error.message)
            else:
                response_data["Error"] = str(error)
        elif error:
            response_data["Error"] = error
            
        return JSONResponse(content=response_data, status_code=status_code)
    
    @staticmethod
    def not_found(request: Request, message: str = "Resource not found") -> JSONResponse:
        """Send a 404 not found response."""
        return ResponseHandler.failure(request, message, 404)
    
    @staticmethod
    def unauthorized(request: Request, message: str = "Unauthorized") -> JSONResponse:
        """Send a 401 unauthorized response."""
        return ResponseHandler.failure(request, message, 401)
    
    @staticmethod
    def forbidden(request: Request, message: str = "Forbidden") -> JSONResponse:
        """Send a 403 forbidden response."""
        return ResponseHandler.failure(request, message, 403)
    
    @staticmethod
    def bad_request(request: Request, message: str = "Bad request") -> JSONResponse:
        """Send a 400 bad request response."""
        return ResponseHandler.failure(request, message, 400)

