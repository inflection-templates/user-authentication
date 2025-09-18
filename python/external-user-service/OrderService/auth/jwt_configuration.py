"""
JWT Authentication Configuration
Configuration for JWT authentication with caching support
"""

import os
import logging
from typing import Optional
from enum import Enum

from cache.jwt_authentication_service import JwtAuthenticationService
from cache.in_memory_jwks_cache import InMemoryJwksKeyCache
from cache.redis_jwks_cache import RedisJwksKeyCache
from cache.jwks_refresh_background_service import JwksRefreshBackgroundService

logger = logging.getLogger(__name__)


class CacheType(str, Enum):
    """Cache type enumeration"""
    IN_MEMORY = "in_memory"
    REDIS = "redis"


class JwtAuthenticationConfiguration:
    """Configuration class for JWT authentication"""
    
    def __init__(self):
        self._jwt_service: Optional[JwtAuthenticationService] = None
        self._background_service: Optional[JwksRefreshBackgroundService] = None
        
        # Configuration from environment
        self.jwt_authority = os.getenv("JWT_AUTHORITY", "http://localhost:5000")
        self.jwt_audience = os.getenv("JWT_AUDIENCE", "shala")
        self.jwks_url = os.getenv("JWT_JWKS_URL", f"{self.jwt_authority}/.well-known/jwks.json")
        self.cache_type = os.getenv("JWKS_CACHE_TYPE", CacheType.IN_MEMORY.value)
        self.redis_url = os.getenv("REDIS_URL", "redis://localhost:6379")
        self.refresh_interval_minutes = int(os.getenv("JWKS_REFRESH_INTERVAL_MINUTES", "5"))
        self.enable_background_refresh = os.getenv("JWKS_ENABLE_BACKGROUND_REFRESH", "true").lower() == "true"
    
    def get_jwt_service(self) -> JwtAuthenticationService:
        """Get or create JWT authentication service"""
        if self._jwt_service is None:
            self._jwt_service = self._create_jwt_service()
        return self._jwt_service
    
    def _create_jwt_service(self) -> JwtAuthenticationService:
        """Create JWT authentication service with appropriate cache"""
        try:
            # Create cache based on configuration
            if self.cache_type == CacheType.REDIS.value:
                logger.info("Using Redis JWKS cache")
                try:
                    cache = RedisJwksKeyCache(redis_url=self.redis_url)
                except ImportError as ex:
                    logger.warning(f"Redis not available, falling back to in-memory cache: {ex}")
                    cache = InMemoryJwksKeyCache()
            else:
                logger.info("Using in-memory JWKS cache")
                cache = InMemoryJwksKeyCache()
            
            # Create JWT service
            jwt_service = JwtAuthenticationService(
                jwks_cache=cache,
                jwks_url=self.jwks_url,
                refresh_interval_minutes=self.refresh_interval_minutes
            )
            
            logger.info(f"JWT authentication service configured with {self.cache_type} cache")
            return jwt_service
            
        except Exception as ex:
            logger.error(f"Error creating JWT authentication service: {ex}")
            # Fallback to in-memory cache
            cache = InMemoryJwksKeyCache()
            return JwtAuthenticationService(
                jwks_cache=cache,
                jwks_url=self.jwks_url,
                refresh_interval_minutes=self.refresh_interval_minutes
            )
    
    async def start_background_services(self):
        """Start background services if enabled"""
        if self.enable_background_refresh:
            jwt_service = self.get_jwt_service()
            self._background_service = JwksRefreshBackgroundService(
                jwt_service=jwt_service,
                refresh_interval_minutes=self.refresh_interval_minutes
            )
            await self._background_service.start_async()
            logger.info("JWKS background refresh service started")
    
    async def stop_background_services(self):
        """Stop background services"""
        if self._background_service:
            await self._background_service.stop_async()
            logger.info("JWKS background refresh service stopped")


# Global configuration instance
_jwt_config: Optional[JwtAuthenticationConfiguration] = None

def get_jwt_configuration() -> JwtAuthenticationConfiguration:
    """Get or create JWT configuration singleton"""
    global _jwt_config
    if _jwt_config is None:
        _jwt_config = JwtAuthenticationConfiguration()
    return _jwt_config
