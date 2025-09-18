"""
Well-known endpoints for JWKS and other discovery
Equivalent to the .NET shala.api.api.wellknown
"""

from fastapi import APIRouter, Depends
from services.jwt_token_service import JwtTokenService
from .handlers import WellKnownHandler

wellknown_router = APIRouter()

# Dependencies
def get_jwt_service() -> JwtTokenService:
    return JwtTokenService()

def get_wellknown_handler(jwt_service: JwtTokenService = Depends(get_jwt_service)) -> WellKnownHandler:
    return WellKnownHandler(jwt_service)


@wellknown_router.get("/.well-known/jwks.json")
async def get_jwks(handler: WellKnownHandler = Depends(get_wellknown_handler)):
    """
    Get JSON Web Key Set (JWKS) for token validation
    This endpoint is used by other services to validate JWT tokens
    """
    return await handler.get_jwks()


@wellknown_router.get("/.well-known/openid_configuration")
async def get_openid_configuration(handler: WellKnownHandler = Depends(get_wellknown_handler)):
    """
    Get OpenID Connect configuration
    """
    return await handler.get_openid_configuration()


@wellknown_router.get("/health")
async def health_check(handler: WellKnownHandler = Depends(get_wellknown_handler)):
    """
    Health check endpoint for JWKS service
    """
    return await handler.health_check()