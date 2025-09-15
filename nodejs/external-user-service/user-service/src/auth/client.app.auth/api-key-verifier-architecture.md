# API Key Verifier Architecture

This document explains the interface-based API key verification system that allows for easy switching between internal and external verification strategies.

## Architecture Overview

The API key verification system is now built using the **Strategy Pattern** with **Dependency Injection**, making it highly modular and configurable. The system automatically selects the appropriate verification strategy based on configuration.

```
┌─────────────────────────────────────┐
│        ApiKeyAuthenticator          │
│  (Main entry point - simplified)    │
└────────────────┬────────────────────┘
                 │ uses
                 ▼
┌─────────────────────────────────────┐
│        IApiKeyVerifier              │
│         (Interface)                 │
└─────────────┬───────────────────────┘
              │ implemented by
              ▼
┌─────────────────────┐  ┌─────────────────────┐
│ InternalApiKey      │  │ ExternalApiKey      │
│ Verifier            │  │ Verifier            │
│ (Database-based)    │  │ (External API +     │
│                     │  │  Caching)           │
└─────────────────────┘  └─────────────────────┘
```

## Core Components

### 1. IApiKeyVerifier Interface

**File**: `src/client.app.auth/interfaces/api.key.verifier.interface.ts`

Defines the contract that all API key verifiers must implement:

```typescript
export interface IApiKeyVerifier {
    verify(apiKey: string): Promise<CurrentClient | null>;
    clearCache?(): Promise<void>;
    getVerifierType(): string;
}
```

### 2. InternalApiKeyVerifier

**File**: `src/client.app.auth/verifiers/internal.api.key.verifier.ts`

- Uses the existing database-based verification
- Leverages `ClientAppService` for validation
- No caching required (direct database access)
- Injectable service with proper dependency injection

### 3. ExternalApiKeyVerifier

**File**: `src/client.app.auth/verifiers/external.api.key.verifier.ts`

- Calls external API for validation
- Implements intelligent caching with expiration
- Handles network errors gracefully
- Supports configurable cache TTL
- Injectable service

### 4. ApiKeyVerifierInjector

**File**: `src/client.app.auth/api.key.verifier.injector.ts`

- Handles dependency injection registration
- Automatically selects verifier based on `InternalClientAppManagement` configuration
- Integrates with the main DI container

### 5. ApiKeyAuthenticator (Refactored)

**File**: `src/client.app.auth/api.key.authenticator.ts`

- Simplified to use dependency injection
- No longer contains verification logic
- Acts as a facade/entry point
- Provides utility methods for cache management

## Configuration-Based Selection

The system automatically selects the appropriate verifier based on the `InternalClientAppManagement` configuration:

```json
{
  "InternalClientAppManagement": true   // Uses InternalApiKeyVerifier
}
```

```json
{
  "InternalClientAppManagement": false  // Uses ExternalApiKeyVerifier
}
```

## Benefits of the New Architecture

### 1. **Separation of Concerns**
- Each verifier handles only its specific verification logic
- Clear boundaries between internal and external verification
- Easier to maintain and debug

### 2. **Easy Extension**
- Add new verification strategies by implementing `IApiKeyVerifier`
- No need to modify existing code
- Support for multiple external providers

### 3. **Testability**
- Each verifier can be unit tested independently
- Easy to mock dependencies
- Clear interfaces for testing

### 4. **Configuration-Driven**
- Switch between strategies without code changes
- Runtime configuration support
- Environment-specific setups

### 5. **Dependency Injection**
- Proper IoC container integration
- Automatic dependency resolution
- Lifecycle management

## Usage Examples

### Basic Authentication
```typescript
// The authenticator automatically uses the configured verifier
const client = await ApiKeyAuthenticator.authenticate(apiKey);
```

### Cache Management
```typescript
// Clear cache (only works with verifiers that support caching)
await ApiKeyAuthenticator.clearCache();
```

### Get Current Verifier Type
```typescript
// For monitoring/debugging
const verifierType = ApiKeyAuthenticator.getVerifierType(); // "Internal" or "External"
```

## Adding New Verifiers

To add a new verification strategy:

### 1. Create the Verifier Class
```typescript
@injectable()
export class CustomApiKeyVerifier implements IApiKeyVerifier {

    public verify = async (apiKey: string): Promise<CurrentClient | null> => {
        // Your custom verification logic
    };

    public getVerifierType = (): string => {
        return 'Custom';
    };
}
```

### 2. Update the Injector
```typescript
export class ApiKeyVerifierInjector {
    static registerInjections(container: DependencyContainer) {
        const verifierType = ConfigurationManager.ApiKeyVerifierType();

        switch (verifierType) {
            case 'internal':
                container.register<IApiKeyVerifier>('IApiKeyVerifier', InternalApiKeyVerifier);
                break;
            case 'external':
                container.register<IApiKeyVerifier>('IApiKeyVerifier', ExternalApiKeyVerifier);
                break;
            case 'custom':
                container.register<IApiKeyVerifier>('IApiKeyVerifier', CustomApiKeyVerifier);
                break;
        }
    }
}
```

## Environment Configuration

### For Internal Verification
```bash
# In your .env or config
INTERNAL_CLIENT_APP_MANAGEMENT=true
```

### For External Verification
```bash
# In your .env or config
INTERNAL_CLIENT_APP_MANAGEMENT=false
EXTERNAL_API_KEY_VALIDATION_ENDPOINT=https://api.example.com/v1/validate-api-key
```

## Monitoring and Logging

The system provides comprehensive logging:

```
Using External API key verifier for key: 12345678...
API key authentication successful using External verifier
Cache cleared for External verifier
```

## Migration from Old System

The migration is seamless:

1. **Existing Code**: No changes required to calling code
2. **Configuration**: Uses existing `InternalClientAppManagement` setting
3. **Functionality**: All existing features preserved
4. **Performance**: Same or better performance with improved caching

## Testing Strategy

### Unit Testing Individual Verifiers
```typescript
describe('InternalApiKeyVerifier', () => {
    it('should verify valid API key', async () => {
        const verifier = new InternalApiKeyVerifier(mockClientAppService);
        const result = await verifier.verify('valid-key');
        expect(result).toBeTruthy();
    });
});
```

### Integration Testing
```typescript
describe('ApiKeyAuthenticator', () => {
    it('should use correct verifier based on config', async () => {
        // Test with different configurations
    });
});
```

## Performance Considerations

### Internal Verifier
- Direct database access
- No network latency
- Immediate consistency

### External Verifier
- Network latency for first request
- Cached responses for subsequent requests
- Configurable cache TTL (default: 60 minutes)
- Automatic cache expiration handling

## Security Features

1. **API Key Hashing**: Cache keys are SHA-256 hashes of API keys
2. **Secure Logging**: Only first 8 characters of API keys are logged
3. **Timeout Protection**: 10-second timeout for external API calls
4. **Error Isolation**: Failures in one verifier don't affect the system

## Troubleshooting

### Check Current Verifier
```typescript
console.log(`Current verifier: ${ApiKeyAuthenticator.getVerifierType()}`);
```

### Common Issues

1. **Wrong verifier being used**
   - Check `InternalClientAppManagement` configuration
   - Verify injector registration

2. **External API not working**
   - Check `EXTERNAL_API_KEY_VALIDATION_ENDPOINT` environment variable
   - Verify network connectivity
   - Check external API response format

3. **Caching issues**
   - Use `ApiKeyAuthenticator.clearCache()` to reset
   - Check cache service configuration

This architecture provides a robust, scalable, and maintainable solution for API key verification with the flexibility to support various verification strategies.
