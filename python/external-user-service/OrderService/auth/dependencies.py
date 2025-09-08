"""
Authentication dependencies for FastAPI
Contains JWT token verification and authentication utilities
"""

import os
import logging
from typing import Optional
from fastapi import HTTPException, Depends, status
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
import jwt
import httpx
from cryptography.hazmat.primitives.asymmetric.rsa import RSAPublicNumbers
from cryptography.hazmat.backends import default_backend
import base64

# Configure logging
logger = logging.getLogger(__name__)

# JWT Configuration
JWT_AUTHORITY = os.getenv("JWT_AUTHORITY", "http://localhost:5000")
JWT_AUDIENCE = os.getenv("JWT_AUDIENCE", "shala")
JWKS_URL = os.getenv("JWT_JWKS_URL", f"{JWT_AUTHORITY}/.well-known/jwks.json")

security = HTTPBearer()


async def get_jwks_key(kid: str) -> Optional[str]:
    """Fetch JWKS key from User Service"""
    try:
        logger.info(f"Fetching JWKS from: {JWKS_URL}")
        async with httpx.AsyncClient() as client:
            response = await client.get(JWKS_URL)
            response.raise_for_status()
            jwks = response.json()
            
            logger.info(f"JWKS response received. Looking for key with kid: {kid}")
            logger.info(f"Available keys in JWKS: {len(jwks.get('keys', []))}")
            
            for key in jwks.get("keys", []):
                logger.info(f"Available key: Kid={key.get('kid')}")
                if key.get("kid") == kid:
                    logger.info(f"Found matching key for kid: {kid}")
                    return key
            
            logger.warning(f"No matching key found for kid: {kid}")
            return None
            
    except Exception as ex:
        logger.error(f"Error fetching JWKS from UserService: {ex}")
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
        
        # Verify token
        payload = jwt.decode(
            token,
            public_key,
            algorithms=["RS256"],
            issuer=JWT_AUDIENCE,
            audience=JWT_AUDIENCE,
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
