/**
 * Main router for Order Service API
 * Equivalent to the user-service router
 */

import express from 'express';
import { logger } from '../common/logger';
import { verifyToken } from '../auth/auth.middleware';
import { CustomerRoutes } from './customer/customer.routes';
import { ProductRoutes } from './product/product.routes';
import { OrderRoutes } from './order/order.routes';

export class Router {
    private _app: express.Application;

    constructor(app: express.Application) {
        this._app = app;
    }

    async init(): Promise<void> {
        try {
            logger.info('Initializing API routes...');

            // Health check endpoint
            this._app.get('/health', (req, res) => {
                res.json({
                    success: true,
                    message: 'Order Service is healthy',
                    httpcode: 200,
                    timestamp: new Date().toISOString(),
                    service: 'Order Management Service',
                    version: '1.0.0'
                });
            });

            // API routes with JWT authentication (like Python FastAPI Depends(verify_token))
            const customerRoutes = new CustomerRoutes();
            const productRoutes = new ProductRoutes();
            const orderRoutes = new OrderRoutes();

            // Apply JWT authentication to all API routes
            this._app.use('/api/customers', verifyToken, customerRoutes.router);
            this._app.use('/api/products', verifyToken, productRoutes.router);
            this._app.use('/api/orders', verifyToken, orderRoutes.router);

            // 404 handler
            this._app.use('*', (req, res) => {
                res.status(404).json({
                    success: false,
                    message: `Route ${req.method} ${req.originalUrl} not found`,
                    httpcode: 404
                });
            });

            logger.info('API routes initialized successfully');
        } catch (error) {
            logger.error('Failed to initialize routes:', error);
            throw error;
        }
    }
}
