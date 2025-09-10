"""
In-Memory JWKS Key Cache Implementation
Thread-safe in-memory caching for JWT signing keys
"""

import asyncio
from datetime import datetime, timedelta
from typing import Optional, Dict
from concurrent.futures import ThreadPoolExecutor
import logging
import base64
from cryptography.hazmat.primitives.asymmetric.rsa import RSAPublicNumbers
from cryptography.hazmat.backends import default_backend

from .jwks_key_cache import IJwksKeyCache, CachedSecurityKey, JwksKey

logger = logging.getLogger(__name__)


class CacheEntry:
    """Internal cache entry with expiration"""
    
    def __init__(self, jwk: JwksKey, expires_at: datetime):
        self.jwk = jwk
        self.expires_at = expires_at


class InMemoryJwksKeyCache(IJwksKeyCache):
    """In-memory implementation of JWKS key cache"""
    
    def __init__(self):
        self._cache: Dict[str, CacheEntry] = {}
        self._lock = asyncio.Lock()
    
    async def try_get_async(self, kid: str) -> Optional[CachedSecurityKey]:
        """Try to get a cached security key by kid"""
        async with self._lock:
            entry = self._cache.get(kid)
            
            if entry:
                if datetime.utcnow() <= entry.expires_at:
                    # Create security key from cached JWKS key
                    try:
                        public_key = self._create_security_key(entry.jwk)
                        logger.debug(f"Using cached key for kid: {kid}")
                        return CachedSecurityKey(public_key, datetime.utcnow())
                    except Exception as ex:
                        logger.error(f"Error creating security key from cached JWKS: {ex}")
                        # Remove invalid entry
                        del self._cache[kid]
                else:
                    # Entry expired, remove it
                    del self._cache[kid]
                    logger.debug(f"Cached key expired for kid: {kid}")
            
            return None
    
    async def set_async(self, kid: str, jwk: JwksKey, ttl: timedelta):
        """Set a JWKS key in cache with TTL"""
        async with self._lock:
            expires_at = datetime.utcnow() + ttl
            self._cache[kid] = CacheEntry(jwk, expires_at)
            logger.debug(f"Cached key with kid: {kid}, expires at: {expires_at}")
    
    async def remove_async(self, kid: str):
        """Remove a key from cache"""
        async with self._lock:
            if kid in self._cache:
                del self._cache[kid]
                logger.debug(f"Removed cached key for kid: {kid}")
    
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
