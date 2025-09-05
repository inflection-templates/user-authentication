"""
Well-known endpoints handlers
Business logic for JWKS and discovery endpoints
"""

import logging
import os
from services.jwt_token_service import JwtTokenService

logger = logging.getLogger(__name__)

class WellKnownHandler:
    def __init__(self, jwt_service: JwtTokenService):
        self.jwt_service = jwt_service

    async def get_jwks(self) -> dict:
        """
        Get JSON Web Key Set (JWKS) for token validation
        This endpoint is used by other services to validate JWT tokens
        """
        try:
            logger.info("Fetching JWKS")
            return self.jwt_service.get_jwks()
        except Exception as e:
            logger.error(f"JWKS error: {e}")
            raise

    async def get_openid_configuration(self) -> dict:
        """
        Get OpenID Connect configuration
        """
        try:
            base_url = os.getenv("BASE_URL", "http://localhost:5000")
            
            logger.info("Fetching OpenID configuration")
            
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
        except Exception as e:
            logger.error(f"OpenID configuration error: {e}")
            raise

    async def health_check(self) -> dict:
        """
        Health check endpoint for JWKS service
        """
        try:
            logger.info("JWKS health check")
            return {
                "status": "healthy",
                "service": "jwks",
                "timestamp": os.getenv("TIMESTAMP", "unknown")
            }
        except Exception as e:
            logger.error(f"Health check error: {e}")
            return {
                "status": "unhealthy",
                "service": "jwks",
                "error": str(e)
            }
