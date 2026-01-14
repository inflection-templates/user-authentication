"""
Health check routes.
Similar to the Node.js health.routes.ts implementation.
"""
from fastapi import APIRouter, Request
from api.health.health_controller import HealthController

router = APIRouter(prefix="/health", tags=["health"])
controller = HealthController()


@router.get("")
async def health_check(request: Request):
    """
    Health check endpoint.
    
    @route   GET /api/v1/health
    @desc    Check service health
    @access  Public
    """
    return await controller.health_check(request)


@router.get("/check")
async def health_check_alt(request: Request):
    """
    Alternative health check endpoint.
    
    @route   GET /health-check
    @desc    Check service health (alternative route)
    @access  Public
    """
    return await controller.health_check(request)

