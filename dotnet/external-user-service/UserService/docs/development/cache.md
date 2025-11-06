# Cache

## Overview

Cache is being used to store the results of data fetched from database. This is done to reduce the number of queries to the database and to improve the performance of the application.

In this API service, we are using cache at the `services` level. This means that the `get and search` type of methods will have their results cached. The cache is being invalidated when the data is updated or deleted. For search, we are taking a hash of the search parameters and using that as the key for the cache.

## Configuration

The cache configuration as defined in the `appsettings.json` file is as follows:

```json
    "Cache": {
        "Enabled": true,
        "ExpirationMinutes": 60,
        "Provider": "Memory",
        "Redis": {
            "ConnectionString": "<connection-string>"
        },
        "Memory": {
            "CacheSize": 1000,
        }
    }
```

The `Enabled` property is used to enable or disable the cache. The `ExpirationMinutes` property is used to set the expiration time for the cache. The `Provider` property is used to set the cache provider. The `Redis` and `Memory` properties are used to set the configuration for the respective cache providers.

## Cache Interface and Providers

The cache is treated as a module and is storesd in `/modules/cache` folder. The cache is mainly being used in the `services` layer, but could be utilized for other caching requirements.

The `ICacheService` interface is used to interact with the cache. The current provider implementations are `MemoryCacheService` and `RedisCacheService`.

## Cache Injections and Usage

The cache is injected into the builder's service collection in file `BuilderCacheExtensions.cs` in `/startup/configurations/builder.extensions` folder. The cache is injected as a singleton service.
