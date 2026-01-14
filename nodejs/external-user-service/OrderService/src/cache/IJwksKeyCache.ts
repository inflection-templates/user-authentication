/**
 * JWKS Key Cache Interface
 * TypeScript equivalent to .NET IJwksKeyCache interface
 */

export interface JwksKey {
    kty: string;
    use: string;
    kid: string;
    n: string;
    e: string;
    alg?: string;
    x5c?: string[];
    x5t?: string;
    'x5t#S256'?: string;
}

export interface CachedSecurityKey {
    jwksKey: JwksKey;
    cachedAt: Date;
    expiresAt: Date;
    isExpired: boolean;
}

export interface IJwksKeyCache {
    /**
     * Try to get a cached security key by key ID
     * @param kid Key ID
     * @returns CachedSecurityKey if found and not expired, null otherwise
     */
    tryGetAsync(kid: string): Promise<CachedSecurityKey | null>;

    /**
     * Set a JWKS key in cache with TTL
     * @param kid Key ID
     * @param jwk JWKS key
     * @param ttl Time to live (Duration object or milliseconds)
     */
    setAsync(kid: string, jwk: JwksKey, ttl: number): Promise<void>;

    /**
     * Remove a JWKS key from cache
     * @param kid Key ID
     */
    removeAsync(kid: string): Promise<void>;
}
