"""
JWT Token Service implementation
Equivalent to the .NET shala.api.services.JwtTokenService
"""

import os
import json
import logging
from datetime import datetime, timedelta
from typing import Optional, Dict, Any
from uuid import UUID
from cryptography.hazmat.primitives.asymmetric import rsa
from cryptography.hazmat.primitives import serialization, hashes
from cryptography.hazmat.backends import default_backend
import jwt
import base64
import hashlib

from domain.types.user_types import User

logger = logging.getLogger(__name__)


class JwtTokenService:
    """JWT Token Service for generating and validating tokens"""
    
    def __init__(self):
        self._rsa_key = self._get_or_create_persistent_rsa_key()
        
        # Make keyId persistent by using a hash of the RSA key parameters
        public_key = self._rsa_key.public_key()
        public_numbers = public_key.public_numbers()
        key_hash = hashlib.sha256(str(public_numbers.n).encode()).hexdigest()[:8]
        self._key_id = key_hash
        
        self._issuer = os.getenv("JWT_ISSUER", "shala")
        self._audience = os.getenv("JWT_AUDIENCE", "shala")
        self._access_token_validity_days = int(os.getenv("JWT_ACCESS_TOKEN_VALIDITY_DAYS", "5"))
        self._refresh_token_validity_days = int(os.getenv("JWT_REFRESH_TOKEN_VALIDITY_DAYS", "365"))
        
        logger.info(f"JWT Token Service initialized with persistent key ID: {self._key_id}")
    
    def generate_token(self, user: User, session_id: UUID, role: Optional[str] = None) -> str:
        """Generate JWT access token for authenticated user using asymmetric key"""
        try:
            claims = self._get_user_claims(user, role, session_id)
            
            # Token expiration
            expires_at = datetime.utcnow() + timedelta(days=self._access_token_validity_days)
            claims.update({
                "exp": expires_at,
                "iat": datetime.utcnow(),
                "iss": self._issuer,
                "aud": self._audience
            })
            
            # Generate token
            token = jwt.encode(
                claims,
                self._rsa_key,
                algorithm="RS256",
                headers={"kid": self._key_id}
            )
            
            logger.info(f"Access token generated for user: {user.id}")
            return token
            
        except Exception as ex:
            logger.error(f"Error generating access token: {ex}")
            raise
    
    def generate_refresh_token(self, user: User, session_id: UUID) -> str:
        """Generate JWT refresh token"""
        try:
            claims = {
                "sub": str(user.id),
                "email": user.email,
                "session_id": str(session_id),
                "token_type": "refresh",
                "exp": datetime.utcnow() + timedelta(days=self._refresh_token_validity_days),
                "iat": datetime.utcnow(),
                "iss": self._issuer,
                "aud": self._audience
            }
            
            token = jwt.encode(
                claims,
                self._rsa_key,
                algorithm="RS256",
                headers={"kid": self._key_id}
            )
            
            logger.info(f"Refresh token generated for user: {user.id}")
            return token
            
        except Exception as ex:
            logger.error(f"Error generating refresh token: {ex}")
            raise
    
    def validate_token(self, token: str) -> Dict[str, Any]:
        """Validate JWT token and return claims"""
        try:
            public_key = self._rsa_key.public_key()
            
            # Decode and validate token
            claims = jwt.decode(
                token,
                public_key,
                algorithms=["RS256"],
                issuer=self._issuer,
                audience=self._audience,
                options={"verify_exp": True, "verify_iss": True, "verify_aud": True}
            )
            
            return claims
            
        except jwt.ExpiredSignatureError:
            logger.warning("Token has expired")
            raise
        except jwt.InvalidTokenError as ex:
            logger.warning(f"Invalid token: {ex}")
            raise
        except Exception as ex:
            logger.error(f"Error validating token: {ex}")
            raise
    
    def get_jwks(self) -> Dict[str, Any]:
        """Get JSON Web Key Set (JWKS) for token validation"""
        try:
            public_key = self._rsa_key.public_key()
            public_numbers = public_key.public_numbers()
            
            # Convert to base64url encoding
            def int_to_base64url(value: int) -> str:
                byte_length = (value.bit_length() + 7) // 8
                value_bytes = value.to_bytes(byte_length, byteorder='big')
                return base64.urlsafe_b64encode(value_bytes).decode('ascii').rstrip('=')
            
            n = int_to_base64url(public_numbers.n)
            e = int_to_base64url(public_numbers.e)
            
            jwks = {
                "keys": [
                    {
                        "kty": "RSA",
                        "use": "sig",
                        "kid": self._key_id,
                        "n": n,
                        "e": e,
                        "alg": "RS256"
                    }
                ]
            }
            
            logger.debug(f"JWKS generated with key ID: {self._key_id}")
            return jwks
            
        except Exception as ex:
            logger.error(f"Error generating JWKS: {ex}")
            raise
    
    def _get_or_create_persistent_rsa_key(self) -> rsa.RSAPrivateKey:
        """Get or create persistent RSA key"""
        key_file_path = "rsa_key.pem"
        
        try:
            # Try to load existing key
            if os.path.exists(key_file_path):
                with open(key_file_path, "rb") as key_file:
                    private_key = serialization.load_pem_private_key(
                        key_file.read(),
                        password=None,
                        backend=default_backend()
                    )
                logger.info("Loaded existing RSA key from file")
                return private_key
            
            # Generate new key if not exists
            logger.info("Generating new RSA key pair...")
            private_key = rsa.generate_private_key(
                public_exponent=65537,
                key_size=2048,
                backend=default_backend()
            )
            
            # Save key to file
            pem = private_key.private_bytes(
                encoding=serialization.Encoding.PEM,
                format=serialization.PrivateFormat.PKCS8,
                encryption_algorithm=serialization.NoEncryption()
            )
            
            with open(key_file_path, "wb") as key_file:
                key_file.write(pem)
            
            logger.info("Generated and saved new RSA key to file")
            return private_key
            
        except Exception as ex:
            logger.error(f"Error managing RSA key: {ex}")
            # Fallback: generate in-memory key
            logger.info("Falling back to in-memory RSA key generation")
            return rsa.generate_private_key(
                public_exponent=65537,
                key_size=2048,
                backend=default_backend()
            )
    
    def _get_user_claims(self, user: User, role: Optional[str], session_id: UUID) -> Dict[str, Any]:
        """Get user claims for JWT token"""
        claims = {
            "sub": str(user.id),
            "email": user.email,
            "session_id": str(session_id),
            "token_type": "access"
        }
        
        # Add optional claims
        if user.username:
            claims["username"] = user.username
        
        if user.first_name:
            claims["given_name"] = user.first_name
        
        if user.last_name:
            claims["family_name"] = user.last_name
        
        if role:
            claims["role"] = role
        
        if user.tenant_id:
            claims["tenant_id"] = str(user.tenant_id)
        
        claims["timezone"] = user.default_timezone
        claims["is_active"] = user.is_active
        claims["status"] = user.status.value
        
        return claims
    
    @property
    def key_id(self) -> str:
        """Get the key ID"""
        return self._key_id
    
    @property
    def access_token_validity_days(self) -> int:
        """Get access token validity in days"""
        return self._access_token_validity_days
    
    @property
    def refresh_token_validity_days(self) -> int:
        """Get refresh token validity in days"""
        return self._refresh_token_validity_days
