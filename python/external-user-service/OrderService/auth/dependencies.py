"""
Authentication dependencies for FastAPI
Contains JWT token verification and authentication utilities with caching support
"""

import os
import logging
from typing import Optional
from fastapi import HTTPException, Depends, status
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
import jwt
from cryptography.hazmat.primitives.asymmetric.rsa import RSAPublicNumbers
from cryptography.hazmat.backends import default_backend
import base64

from .jwt_configuration import get_jwt_configuration

# Configure logging
logger = logging.getLogger(__name__)

security = HTTPBearer()

def get_jwt_service():
    """Get JWT authentication service from configuration"""
    config = get_jwt_configuration()
    return config.get_jwt_service()


async def get_jwks_key(kid: str) -> Optional[dict]:
    """Fetch JWKS key from User Service with caching"""
    jwt_service = get_jwt_service()
    jwks_key = await jwt_service.get_jwks_key_async(kid)
    
    if jwks_key:
        return {
            "kty": jwks_key.kty,
            "use": jwks_key.use,
            "kid": jwks_key.kid,
            "n": jwks_key.n,
            "e": jwks_key.e
        }
    
    return None


async def verify_token(credentials: HTTPAuthorizationCredentials = Depends(security)):
    """Verify JWT token from User Service"""
    try:
        token = credentials.credentials
        
        # Decode token header to get kid
        unverified_header = jwt.get_unverified_header(token)
        kid = unverified_header.get("kid")
        
        if not kid:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Token missing key ID"
            )
        
        # Get public key from JWKS
        jwks_key = await get_jwks_key(kid)
        if not jwks_key:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Unable to find appropriate key"
            )
        
        # Convert JWKS key to RSA public key
        def base64url_decode(input_str):
            """Decode base64url string"""
            padding = 4 - len(input_str) % 4
            if padding != 4:
                input_str += '=' * padding
            return base64.urlsafe_b64decode(input_str)
        
        n = int.from_bytes(base64url_decode(jwks_key["n"]), byteorder="big")
        e = int.from_bytes(base64url_decode(jwks_key["e"]), byteorder="big")
        
        public_key = RSAPublicNumbers(e, n).public_key(default_backend())
        
        # Get JWT configuration for audience and issuer
        config = get_jwt_configuration()
        
        # Verify token
        payload = jwt.decode(
            token,
            public_key,
            algorithms=["RS256"],
            issuer=config.jwt_audience,
            audience=config.jwt_audience,
            options={"verify_exp": True, "verify_iss": True, "verify_aud": True}
        )
        
        logger.info(f"Token verified successfully for user: {payload.get('sub')}")
        return payload
        
    except jwt.ExpiredSignatureError:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Token has expired"
        )
    except jwt.InvalidTokenError as e:
        logger.error(f"JWT verification failed: {e}")
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Could not validate credentials"
        )
    except Exception as e:
        logger.error(f"Token verification error: {e}")
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Token verification failed"
        )
