/**
 * JWKS Key Cache Interface
 * TypeScript equivalent to Python JWKS cache interface
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

export interface IJwksKeyCache {
    /**
     * Try to get a JWKS key from cache
     * @param kid Key ID
     * @returns JwksKey if found, null otherwise
     */
    tryGetAsync(kid: string): Promise<JwksKey | null>;

    /**
     * Set a JWKS key in cache with TTL
     * @param kid Key ID
     * @param jwk JWKS key
     * @param ttlMinutes Time to live in minutes
     */
    setAsync(kid: string, jwk: JwksKey, ttlMinutes: number): Promise<void>;

    /**
     * Remove a JWKS key from cache
     * @param kid Key ID
     */
    removeAsync(kid: string): Promise<void>;

    /**
     * Clear all keys from cache
     */
    clearAsync(): Promise<void>;
}
