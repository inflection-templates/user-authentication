/**
 * Well-known endpoints - JWKS and OpenID configuration
 * Node.js equivalent to Python wellknown routes
 */

import express from 'express';
import { JwtRsaTokenService } from '../../services/jwt.rsa.token.service';
import { logger } from '../../logger/logger';

const router = express.Router();
const jwtService = new JwtRsaTokenService();

/**
 * GET /.well-known/jwks.json
 * Returns JSON Web Key Set for JWT token validation
 */
router.get('/jwks.json', (req, res) => {
    try {
        logger.info('JWKS endpoint called');
        const jwks = jwtService.getJwks();
        
        res.setHeader('Content-Type', 'application/json');
        res.setHeader('Cache-Control', 'public, max-age=3600'); // Cache for 1 hour
        res.json(jwks);
        
        logger.info('JWKS response sent successfully');
    } catch (error) {
        logger.error(`JWKS error: ${error.message}`);
        res.status(500).json({
            error: 'Internal server error',
            message: 'Unable to retrieve JWKS'
        });
    }
});

/**
 * GET /.well-known/jwks/health
 * Health check for JWKS service
 */
router.get('/jwks/health', (req, res) => {
    try {
        logger.info('JWKS health check called');
        const healthInfo = jwtService.getKeyInfo();
        res.json(healthInfo);
    } catch (error) {
        logger.error(`JWKS health check error: ${error.message}`);
        res.status(500).json({
            Status: 'Unhealthy',
            Error: error.message,
            Timestamp: new Date().toISOString()
        });
    }
});

/**
 * GET /.well-known/openid-configuration
 * OpenID Connect discovery document
 */
router.get('/openid-configuration', (req, res) => {
    try {
        const baseUrl = process.env.BASE_URL || `${req.protocol}://${req.get('host')}`;
        
        const config = {
            issuer: baseUrl,
            authorization_endpoint: `${baseUrl}/api/auth/authorize`,
            token_endpoint: `${baseUrl}/api/auth/token`,
            userinfo_endpoint: `${baseUrl}/api/users/me`,
            jwks_uri: `${baseUrl}/.well-known/jwks.json`,
            response_types_supported: ['code', 'token'],
            subject_types_supported: ['public'],
            id_token_signing_alg_values_supported: ['RS256'],
            scopes_supported: ['openid', 'profile', 'email'],
            token_endpoint_auth_methods_supported: ['client_secret_basic', 'client_secret_post'],
            claims_supported: [
                'sub', 'email', 'given_name', 'family_name', 'username',
                'role', 'tenant_id', 'timezone', 'is_active', 'status'
            ]
        };

        res.json(config);
        logger.info('OpenID configuration sent successfully');
    } catch (error) {
        logger.error(`OpenID configuration error: ${error.message}`);
        res.status(500).json({
            error: 'Internal server error',
            message: 'Unable to retrieve OpenID configuration'
        });
    }
});

export { router as wellKnownRoutes };
