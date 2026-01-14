/**
 * JWT Authentication Middleware
 * TypeScript equivalent to .NET JWT middleware with JWKS caching
 */

import { Request, Response, NextFunction } from 'express';
import jwt from 'jsonwebtoken';
import crypto from 'crypto';
import { getJwtConfiguration } from './JwtAuthenticationConfiguration';
import { JwksKey } from '../cache/IJwksKeyCache';

export interface AuthenticatedRequest extends Request {
    user?: any;
}

/**
 * Convert JWK to PEM format for JWT verification
 */
function jwkToPem(jwk: JwksKey): string {
    try {
        // Create RSA public key using Node.js crypto
        const publicKey = crypto.createPublicKey({
            key: {
                kty: 'RSA',
                n: jwk.n,
                e: jwk.e,
            },
            format: 'jwk'
        });
        
        // Convert to PEM format
        return publicKey.export({ type: 'spki', format: 'pem' }) as string;
        
    } catch (error: any) {
        console.error(`Error converting JWK to PEM: ${error.message}`);
        throw error;
    }
}

/**
 * Get JWKS key using the caching service
 */
async function getJwksKey(kid: string): Promise<JwksKey | null> {
    try {
        const config = getJwtConfiguration();
        const jwtService = config.getJwtService();
        
        return await jwtService.getJwksKeyAsync(kid);
    } catch (error: any) {
        console.error(`Error getting JWKS key: ${error.message}`);
        return null;
    }
}

/**
 * JWT Token Verification Middleware
 * Equivalent to .NET [Authorize] attribute
 */
export const verifyToken = async (req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> => {
    try {
        const authHeader = req.headers.authorization;
        
        if (!authHeader || !authHeader.startsWith('Bearer ')) {
            res.status(401).json({
                success: false,
                message: 'Authorization header missing or invalid',
                httpcode: 401
            });
            return;
        }
        
        const token = authHeader.substring(7); // Remove 'Bearer ' prefix
        
        // Decode token header to get kid (Key ID)
        const unverifiedHeader = jwt.decode(token, { complete: true })?.header;
        const kid = unverifiedHeader?.kid;
        
        if (!kid) {
            res.status(401).json({
                success: false,
                message: 'Token missing key ID',
                httpcode: 401
            });
            return;
        }
        
        // Get public key from JWKS cache
        const jwksKey = await getJwksKey(kid);
        if (!jwksKey) {
            res.status(401).json({
                success: false,
                message: 'Unable to find appropriate key',
                httpcode: 401
            });
            return;
        }
        
        // Convert JWKS key to PEM format for verification
        const publicKeyPem = jwkToPem(jwksKey);
        
        // Get JWT configuration for audience and issuer
        const config = getJwtConfiguration();
        
        // Verify token with RSA public key
        const payload = jwt.verify(token, publicKeyPem, {
            algorithms: ['RS256'],
            issuer: config.audience,
            audience: config.audience,
        }) as any;
        
        console.log(`✅ Token verified successfully for user: ${payload.sub}`);
        req.user = payload;
        next();
        
    } catch (error: any) {
        console.error(`❌ Token verification error: ${error.message}`);
        
        if (error.name === 'TokenExpiredError') {
            res.status(401).json({
                success: false,
                message: 'Token has expired',
                httpcode: 401
            });
            return;
        }
        
        if (error.name === 'JsonWebTokenError') {
            res.status(401).json({
                success: false,
                message: 'Could not validate credentials',
                httpcode: 401
            });
            return;
        }
        
        res.status(401).json({
            success: false,
            message: 'Token verification failed',
            httpcode: 401
        });
    }
};

/**
 * Optional middleware to require authentication
 */
export const requireAuth = (req: AuthenticatedRequest, res: Response, next: NextFunction): void => {
    if (!req.user) {
        res.status(401).json({
            success: false,
            message: 'Authentication required',
            httpcode: 401
        });
        return;
    }
    next();
};
