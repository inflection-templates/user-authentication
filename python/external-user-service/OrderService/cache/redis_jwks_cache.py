"""
Redis JWKS Key Cache Implementation
Redis-based caching for JWT signing keys with distributed support
"""

import json
import base64
from datetime import datetime, timedelta
from typing import Optional
import logging
from cryptography.hazmat.primitives.asymmetric.rsa import RSAPublicNumbers
from cryptography.hazmat.backends import default_backend

try:
    import redis.asyncio as redis
    REDIS_AVAILABLE = True
except ImportError:
    REDIS_AVAILABLE = False
    redis = None

from .jwks_key_cache import IJwksKeyCache, CachedSecurityKey, JwksKey

logger = logging.getLogger(__name__)


class RedisJwksKeyCache(IJwksKeyCache):
    """Redis implementation of JWKS key cache"""
    
    def __init__(self, redis_client=None, redis_url: str = "redis://localhost:6379"):
        if not REDIS_AVAILABLE:
            raise ImportError("Redis is not available. Install redis with: pip install redis")
        
        if redis_client:
            self._redis = redis_client
        else:
            self._redis = redis.from_url(redis_url, decode_responses=True)
        
        self._prefix = "jwks:kid:"
    
    async def try_get_async(self, kid: str) -> Optional[CachedSecurityKey]:
        """Try to get a cached security key by kid"""
        try:
            json_data = await self._redis.get(self._prefix + kid)
            if not json_data:
                return None
            
            # Deserialize JWKS key
            jwk_dict = json.loads(json_data)
            jwk = JwksKey.from_dict(jwk_dict)
            
            # Create security key
            public_key = self._create_security_key(jwk)
            logger.debug(f"Using Redis cached key for kid: {kid}")
            return CachedSecurityKey(public_key, datetime.utcnow())
            
        except Exception as ex:
            logger.error(f"Error retrieving key from Redis cache for kid {kid}: {ex}")
            return None
    
    async def set_async(self, kid: str, jwk: JwksKey, ttl: timedelta):
        """Set a JWKS key in cache with TTL"""
        try:
            # Serialize JWKS key
            json_data = json.dumps(jwk.to_dict())
            
            # Set with TTL
            await self._redis.setex(
                self._prefix + kid, 
                int(ttl.total_seconds()), 
                json_data
            )
            logger.debug(f"Cached key in Redis with kid: {kid}, TTL: {ttl}")
            
        except Exception as ex:
            logger.error(f"Error setting key in Redis cache for kid {kid}: {ex}")
    
    async def remove_async(self, kid: str):
        """Remove a key from cache"""
        try:
            await self._redis.delete(self._prefix + kid)
            logger.debug(f"Removed key from Redis cache for kid: {kid}")
        except Exception as ex:
            logger.error(f"Error removing key from Redis cache for kid {kid}: {ex}")
    
    def _create_security_key(self, jwks_key: JwksKey):
        """Create RSA public key from JWKS key (equivalent to .NET CreateSecurityKeyStatic)"""
        try:
            # Decode base64url encoded values
            n_bytes = self._base64url_decode(jwks_key.n)
            e_bytes = self._base64url_decode(jwks_key.e)
            
            # Convert to integers
            n = int.from_bytes(n_bytes, byteorder="big")
            e = int.from_bytes(e_bytes, byteorder="big")
            
            # Create RSA public key
            public_key = RSAPublicNumbers(e, n).public_key(default_backend())
            
            return public_key
            
        except Exception as ex:
            logger.error(f"Error creating RSA public key from JWKS: {ex}")
            raise
    
    def _base64url_decode(self, input_str: str) -> bytes:
        """Decode base64url string"""
        padding = 4 - len(input_str) % 4
        if padding != 4:
            input_str += '=' * padding
        return base64.urlsafe_b64decode(input_str)
    
    async def close(self):
        """Close Redis connection"""
        try:
            await self._redis.close()
        except Exception as ex:
            logger.error(f"Error closing Redis connection: {ex}")
