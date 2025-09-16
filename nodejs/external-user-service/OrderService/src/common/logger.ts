/**
 * Logger configuration for Order Service
 * Equivalent to the user-service logger implementation
 */

import winston from 'winston';
import DailyRotateFile from 'winston-daily-rotate-file';

const logFormat = winston.format.combine(
    winston.format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss' }),
    winston.format.errors({ stack: true }),
    winston.format.printf(({ timestamp, level, message, stack }) => {
        return `[${timestamp}] ${level.toUpperCase()}: ${stack || message}`;
    })
);

const transports: winston.transport[] = [
    new winston.transports.Console({
        format: winston.format.combine(
            winston.format.colorize(),
            logFormat
        )
    })
];

// Add file transport for non-test environments
if (process.env.NODE_ENV !== 'test') {
    transports.push(
        new DailyRotateFile({
            filename: 'logs/order-service-%DATE%.log',
            datePattern: 'YYYY-MM-DD',
            zippedArchive: true,
            maxSize: '20m',
            maxFiles: '14d',
            format: logFormat
        })
    );
}

export const logger = winston.createLogger({
    level: process.env.LOG_LEVEL || 'info',
    format: logFormat,
    transports,
    exitOnError: false
});

// Create logs directory if it doesn't exist
import { existsSync, mkdirSync } from 'fs';
if (!existsSync('logs')) {
    mkdirSync('logs');
}
