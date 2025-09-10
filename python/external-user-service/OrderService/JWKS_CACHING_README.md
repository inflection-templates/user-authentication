# JWKS Key Caching Implementation

This document describes the JWKS (JSON Web Key Set) caching implementation for the Python OrderService, which mirrors the functionality of the .NET version.

## Overview

The caching system provides:
- **In-Memory Caching**: Fast, local caching using Python dictionaries
- **Redis Caching**: Distributed caching for multi-instance deployments
- **Background Refresh**: Automatic periodic refresh of JWKS keys
- **Thread-Safe Operations**: Async-safe caching operations
- **Configurable TTL**: Customizable time-to-live for cached keys

## Architecture

The implementation follows the same pattern as the .NET version:

```
IJwksKeyCache (Interface)
├── InMemoryJwksKeyCache
└── RedisJwksKeyCache

JwtAuthenticationService
├── Uses IJwksKeyCache for caching
└── Handles JWKS endpoint communication

JwksRefreshBackgroundService
└── Periodic refresh of keys
```

## File Structure

```
services/
├── jwks_key_cache.py              # Interface and base classes
├── in_memory_jwks_cache.py        # In-memory implementation
├── redis_jwks_cache.py            # Redis implementation
├── jwt_authentication_service.py  # Main JWT service with caching
└── jwks_refresh_background_service.py  # Background refresh service

auth/
├── dependencies.py                # FastAPI dependencies (updated)
└── jwt_configuration.py          # Configuration management
```

## Configuration

### Environment Variables

```bash
# JWT Configuration
JWT_AUTHORITY=http://localhost:5000
JWT_AUDIENCE=shala
JWT_JWKS_URL=http://localhost:5000/.well-known/jwks.json

# Cache Configuration
JWKS_CACHE_TYPE=in_memory          # "in_memory" or "redis"
REDIS_URL=redis://localhost:6379   # Redis connection string
JWKS_REFRESH_INTERVAL_MINUTES=5    # Refresh interval
JWKS_ENABLE_BACKGROUND_REFRESH=true # Enable background refresh
```

### Cache Types

#### In-Memory Cache
- Default option
- Fast access
- Single-instance only
- No external dependencies

#### Redis Cache
- Distributed caching
- Multi-instance support
- Requires Redis server
- Automatic serialization/deserialization

## Usage

### Basic Usage

The caching is automatically integrated into the existing authentication flow:

```python
from auth.dependencies import verify_token
from fastapi import Depends

@app.get("/protected")
async def protected_endpoint(token_payload = Depends(verify_token)):
    # Token verification now uses caching automatically
    return {"user": token_payload["sub"]}
```

### Manual Service Usage

```python
from auth.jwt_configuration import get_jwt_configuration

# Get JWT service with caching
config = get_jwt_configuration()
jwt_service = config.get_jwt_service()

# Get signing keys (with caching)
keys = await jwt_service.get_signing_keys_async("key-id")
```

### Custom Cache Implementation

```python
from services import IJwksKeyCache, JwtAuthenticationService

class CustomCache(IJwksKeyCache):
    async def try_get_async(self, kid: str):
        # Custom implementation
        pass
    
    async def set_async(self, kid: str, jwk: JwksKey, ttl: timedelta):
        # Custom implementation
        pass
    
    async def remove_async(self, kid: str):
        # Custom implementation
        pass

# Use custom cache
jwt_service = JwtAuthenticationService(jwks_cache=CustomCache())
```

## Key Features

### 1. Automatic Caching
- Keys are automatically cached on first use
- Cache misses trigger JWKS endpoint calls
- Expired keys are automatically removed

### 2. Background Refresh
- Periodic refresh of all keys
- Configurable refresh interval
- Graceful error handling

### 3. Thread Safety
- Async locks for in-memory cache
- Redis operations are naturally thread-safe
- No race conditions in concurrent scenarios

### 4. Error Handling
- Graceful fallback on cache failures
- Logging for debugging
- Automatic retry mechanisms

## Performance Benefits

### Compared to No Caching:
- **Reduced Network Calls**: Keys cached for 10 minutes by default
- **Faster Token Verification**: No JWKS endpoint call on cache hit
- **Better Reliability**: Cached keys available during network issues

### Cache Hit Rates:
- **Typical Scenarios**: 95%+ hit rate after initial warm-up
- **High Traffic**: Significant reduction in JWKS endpoint load

## Monitoring and Debugging

### Logging
The implementation provides detailed logging:

```python
import logging
logging.getLogger("services.jwt_authentication_service").setLevel(logging.DEBUG)
```

### Log Messages:
- Cache hits/misses
- Key refresh operations
- Error conditions
- Background service status

## Migration from Non-Cached Version

The caching implementation is backward compatible. No changes are required to existing code that uses the authentication dependencies.

## Dependencies

### Required Packages
```
redis[hiredis]==5.0.1  # For Redis caching (optional)
cryptography>=40.0.0   # For RSA key operations
httpx==0.25.2          # For HTTP requests
```

### Installation
```bash
pip install -r requirements.txt
```

## Troubleshooting

### Common Issues

1. **Redis Connection Errors**
   - Check REDIS_URL configuration
   - Ensure Redis server is running
   - Falls back to in-memory cache automatically

2. **Cache Miss Issues**
   - Check JWKS endpoint availability
   - Verify JWT_JWKS_URL configuration
   - Check network connectivity

3. **Background Service Not Starting**
   - Verify JWKS_ENABLE_BACKGROUND_REFRESH=true
   - Check application startup logs
   - Ensure proper async context

## Comparison with .NET Implementation

| Feature | .NET | Python |
|---------|------|--------|
| In-Memory Cache | ✅ ConcurrentDictionary | ✅ Dict with AsyncLock |
| Redis Cache | ✅ StackExchange.Redis | ✅ redis-py async |
| Background Service | ✅ BackgroundService | ✅ AsyncIO Task |
| Interface Pattern | ✅ IJwksKeyCache | ✅ ABC IJwksKeyCache |
| Configuration | ✅ IConfiguration | ✅ Environment Variables |
| Dependency Injection | ✅ Built-in DI | ✅ Singleton Pattern |

The Python implementation maintains feature parity with the .NET version while following Python-specific patterns and conventions.
