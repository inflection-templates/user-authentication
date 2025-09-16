/**
 * JWT Authentication Middleware
 * TypeScript equivalent to Python auth dependencies
 */

import { Request, Response, NextFunction } from 'express';
import jwt from 'jsonwebtoken';
import crypto from 'crypto';
import { logger } from '../common/logger';
import { getJwtConfiguration } from './jwt.configuration';

export interface AuthenticatedRequest extends Request {
    user?: any;
}

export interface JwksKey {
    kty: string;
    use: string;
    kid: string;
    n: string;
    e: string;
}

function base64urlDecode(input: string): Buffer {
    const padding = 4 - (input.length % 4);
    if (padding !== 4) {
        input += '='.repeat(padding);
    }
    return Buffer.from(input.replace(/-/g, '+').replace(/_/g, '/'), 'base64');
}

function jwkToPem(jwk: JwksKey): string {
    try {
        const n = base64urlDecode(jwk.n);
        const e = base64urlDecode(jwk.e);
        
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
        
    } catch (error) {
        logger.error(`Error converting JWK to PEM: ${error}`);
        throw error;
    }
}

async function getJwksKey(kid: string): Promise<JwksKey | null> {
    try {
        const config = getJwtConfiguration();
        const jwtService = config.getJwtService();
        const jwksKey = await jwtService.getJwksKeyAsync(kid);
        
        if (jwksKey) {
            return {
                kty: jwksKey.kty,
                use: jwksKey.use,
                kid: jwksKey.kid,
                n: jwksKey.n,
                e: jwksKey.e
            };
        }
        
        return null;
    } catch (error) {
        logger.error(`Error getting JWKS key: ${error}`);
        return null;
    }
}

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
        
        // Decode token header to get kid
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
        
        // Get public key from JWKS
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
        
        // Verify token with RSA public key (like Python service)
        const payload = jwt.verify(token, publicKeyPem, {
            algorithms: ['RS256'],
            issuer: config.jwtAudience,
            audience: config.jwtAudience,
        }) as any;
        
        logger.info(`Token verified successfully for user: ${payload.sub}`);
        req.user = payload;
        next();
        
    } catch (error: any) {
        logger.error(`Token verification error: ${error.message}`);
        
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

// Optional middleware - can be used to protect routes that require authentication
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

