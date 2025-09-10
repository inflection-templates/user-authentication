"""
JWT Authentication Service with Caching
Provides JWT token verification with JWKS key caching similar to .NET implementation
"""

import os
import json
import asyncio
from datetime import datetime, timedelta
from typing import List, Dict, Any, Optional
import logging
import httpx

from .jwks_key_cache import IJwksKeyCache, JwksKey
from .in_memory_jwks_cache import InMemoryJwksKeyCache

logger = logging.getLogger(__name__)


class JwksResponse:
    """Represents a JWKS response"""
    
    def __init__(self, keys: List[Dict[str, Any]] = None):
        self.keys = [JwksKey.from_dict(key) for key in (keys or [])]


class JwtAuthenticationService:
    """JWT Authentication Service with caching support"""
    
    def __init__(self, 
                 jwks_cache: IJwksKeyCache = None,
                 jwks_url: str = None,
                 refresh_interval_minutes: int = 5):
        """
        Initialize JWT Authentication Service
        
        Args:
            jwks_cache: Cache implementation (defaults to InMemoryJwksKeyCache)
            jwks_url: JWKS endpoint URL
            refresh_interval_minutes: How often to refresh keys from JWKS endpoint
        """
        self._jwks_cache = jwks_cache or InMemoryJwksKeyCache()
        self._jwks_url = jwks_url or os.getenv("JWT_JWKS_URL", "http://localhost:5000/.well-known/jwks.json")
        self._refresh_interval = timedelta(minutes=refresh_interval_minutes)
        self._refresh_semaphore = asyncio.Semaphore(1)
        self._last_refresh = datetime.min
    
    async def get_signing_keys_async(self, kid: str) -> List[Any]:
        """
        Get signing keys for the specified key ID
        
        Args:
            kid: Key ID to look for
            
        Returns:
            List of security keys (public keys)
        """
        # Check cache first
        cached_key = await self._jwks_cache.try_get_async(kid)
        if cached_key and not cached_key.is_expired:
            logger.debug(f"Using cached key for kid: {kid}")
            return [cached_key.public_key]
        
        # Check if we need to refresh keys
        if self._should_refresh_keys():
            await self.refresh_keys_async()
        
        # Try cache again after refresh
        cached_key = await self._jwks_cache.try_get_async(kid)
        if cached_key and not cached_key.is_expired:
            logger.debug(f"Using refreshed cached key for kid: {kid}")
            return [cached_key.public_key]
        
        logger.warning(f"No valid key found for kid: {kid}")
        return []
    
    async def refresh_keys_async(self):
        """Refresh JWKS keys from the endpoint"""
        async with self._refresh_semaphore:
            try:
                logger.info(f"Refreshing JWKS keys from: {self._jwks_url}")
                
                async with httpx.AsyncClient() as client:
                    response = await client.get(self._jwks_url)
                    response.raise_for_status()
                    
                    jwks_data = response.json()
                    jwks = JwksResponse(jwks_data.get("keys", []))
                    
                    logger.info(f"Received {len(jwks.keys)} keys from JWKS")
                    
                    # Add or update keys in cache
                    for jwks_key in jwks.keys:
                        await self._jwks_cache.set_async(
                            jwks_key.kid, 
                            jwks_key, 
                            timedelta(minutes=10)  # Cache for 10 minutes
                        )
                        logger.debug(f"Cached key with kid: {jwks_key.kid}")
                    
                    self._last_refresh = datetime.utcnow()
                    logger.info("Successfully refreshed and cached keys")
                    
            except Exception as ex:
                logger.error(f"Error refreshing JWKS keys: {ex}")
    
    def _should_refresh_keys(self) -> bool:
        """Check if keys should be refreshed based on interval"""
        return datetime.utcnow() - self._last_refresh > self._refresh_interval
    
    async def get_jwks_key_async(self, kid: str) -> Optional[JwksKey]:
        """
        Get JWKS key for the specified key ID (for backward compatibility)
        
        Args:
            kid: Key ID to look for
            
        Returns:
            JwksKey if found, None otherwise
        """
        try:
            # First try to get from cache
            cached_key = await self._jwks_cache.try_get_async(kid)
            if cached_key and not cached_key.is_expired:
                # We need to get the original JwksKey, but our cache stores the public key
                # Let's fetch fresh from JWKS endpoint
                pass
            
            # Fetch from JWKS endpoint
            logger.info(f"Fetching JWKS from: {self._jwks_url}")
            async with httpx.AsyncClient() as client:
                response = await client.get(self._jwks_url)
                response.raise_for_status()
                jwks_data = response.json()
                
                logger.info(f"JWKS response received. Looking for key with kid: {kid}")
                logger.info(f"Available keys in JWKS: {len(jwks_data.get('keys', []))}")
                
                for key_data in jwks_data.get("keys", []):
                    logger.info(f"Available key: Kid={key_data.get('kid')}")
                    if key_data.get("kid") == kid:
                        logger.info(f"Found matching key for kid: {kid}")
                        return JwksKey.from_dict(key_data)
                
                logger.warning(f"No matching key found for kid: {kid}")
                return None
                
        except Exception as ex:
            logger.error(f"Error fetching JWKS from UserService: {ex}")
            return None
