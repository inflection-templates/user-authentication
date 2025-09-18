/**
 * Error handling middleware for Order Service
 * Equivalent to the user-service error handling
 */

import { Request, Response, NextFunction } from 'express';
import { logger } from '../common/logger';

export interface ApiError extends Error {
    statusCode?: number;
    details?: any;
}

export const errorHandlerMiddleware = (
    error: ApiError,
    req: Request,
    res: Response,
    next: NextFunction
): void => {
    logger.error(`Error: ${error.message}`, { 
        stack: error.stack,
        url: req.url,
        method: req.method,
        ip: req.ip
    });

    // JWT authentication errors
    if (error.name === 'UnauthorizedError' || error.name === 'JsonWebTokenError') {
        res.status(401).json({
            success: false,
            message: 'Invalid or missing authentication token',
            httpcode: 401
        });
        return;
    }

    // Validation errors
    if (error.name === 'ValidationError') {
        res.status(400).json({
            success: false,
            message: error.message,
            httpcode: 400,
            details: error.details
        });
        return;
    }

    // Sequelize validation errors
    if (error.name === 'SequelizeValidationError') {
        res.status(400).json({
            success: false,
            message: 'Database validation error',
            httpcode: 400,
            details: error.details
        });
        return;
    }

    // Database unique constraint errors
    if (error.name === 'SequelizeUniqueConstraintError') {
        res.status(409).json({
            success: false,
            message: 'Resource already exists',
            httpcode: 409
        });
        return;
    }

    // Not found errors
    if (error.statusCode === 404) {
        res.status(404).json({
            success: false,
            message: error.message || 'Resource not found',
            httpcode: 404
        });
        return;
    }

    // Default server error
    const statusCode = error.statusCode || 500;
    res.status(statusCode).json({
        success: false,
        message: process.env.NODE_ENV === 'production' ? 'Internal server error' : error.message,
        httpcode: statusCode
    });
};
