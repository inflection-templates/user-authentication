"""
Well-known endpoints for JWKS and other discovery
Equivalent to the .NET shala.api.api.wellknown
"""

from fastapi import APIRouter, Depends
from services.jwt_token_service import JwtTokenService

wellknown_router = APIRouter()

# Dependency to get JWT token service
def get_jwt_service() -> JwtTokenService:
    return JwtTokenService()


@wellknown_router.get("/.well-known/jwks.json")
async def get_jwks(jwt_service: JwtTokenService = Depends(get_jwt_service)):
    """
    Get JSON Web Key Set (JWKS) for token validation
    This endpoint is used by other services to validate JWT tokens
    """
    return jwt_service.get_jwks()


@wellknown_router.get("/.well-known/openid_configuration")
async def get_openid_configuration():
    """
    Get OpenID Connect configuration
    """
    import os
    base_url = os.getenv("BASE_URL", "http://localhost:5000")
    
    return {
        "issuer": base_url,
        "authorization_endpoint": f"{base_url}/api/auth/authorize",
        "token_endpoint": f"{base_url}/api/auth/token",
        "userinfo_endpoint": f"{base_url}/api/users/me",
        "jwks_uri": f"{base_url}/.well-known/jwks.json",
        "response_types_supported": ["code", "token"],
        "subject_types_supported": ["public"],
        "id_token_signing_alg_values_supported": ["RS256"],
        "scopes_supported": ["openid", "profile", "email"],
        "token_endpoint_auth_methods_supported": ["client_secret_basic", "client_secret_post"],
        "claims_supported": [
            "sub", "email", "given_name", "family_name", "username",
            "role", "tenant_id", "timezone", "is_active", "status"
        ]
    }
