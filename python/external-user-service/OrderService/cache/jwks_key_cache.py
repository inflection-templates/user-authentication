"""
JWKS Key Cache Interface and Implementations
Provides caching functionality for JWT signing keys similar to .NET implementation
"""

from abc import ABC, abstractmethod
from typing import Optional
from datetime import datetime, timedelta
import json
import asyncio
from concurrent.futures import ThreadPoolExecutor
import logging

logger = logging.getLogger(__name__)


class JwksKey:
    """Represents a JWKS key"""
    
    def __init__(self, kty: str = "", use: str = "", kid: str = "", n: str = "", e: str = ""):
        self.kty = kty
        self.use = use
        self.kid = kid
        self.n = n
        self.e = e
    
    def to_dict(self) -> dict:
        """Convert to dictionary for serialization"""
        return {
            "kty": self.kty,
            "use": self.use,
            "kid": self.kid,
            "n": self.n,
            "e": self.e
        }
    
    @classmethod
    def from_dict(cls, data: dict) -> 'JwksKey':
        """Create from dictionary"""
        return cls(
            kty=data.get("kty", ""),
            use=data.get("use", ""),
            kid=data.get("kid", ""),
            n=data.get("n", ""),
            e=data.get("e", "")
        )


class CachedSecurityKey:
    """Represents a cached security key with expiration"""
    
    def __init__(self, public_key, cached_at: datetime):
        self.public_key = public_key
        self.cached_at = cached_at
        self.expires_at = cached_at + timedelta(minutes=10)  # Keys expire after 10 minutes
    
    @property
    def is_expired(self) -> bool:
        """Check if the cached key has expired"""
        return datetime.utcnow() > self.expires_at


class IJwksKeyCache(ABC):
    """Interface for JWKS key caching implementations"""
    
    @abstractmethod
    async def try_get_async(self, kid: str) -> Optional[CachedSecurityKey]:
        """Try to get a cached security key by kid"""
        pass
    
    @abstractmethod
    async def set_async(self, kid: str, jwk: JwksKey, ttl: timedelta):
        """Set a JWKS key in cache with TTL"""
        pass
    
    @abstractmethod
    async def remove_async(self, kid: str):
        """Remove a key from cache"""
        pass
