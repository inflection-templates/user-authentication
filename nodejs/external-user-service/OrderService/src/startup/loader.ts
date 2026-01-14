/**
 * Application loader for Order Service
 * Equivalent to the user-service loader
 */

import { logger } from '../common/logger';
import { Injector } from './injector';
import { DatabaseConnector } from '../database/database.connector';
import { Seeder } from './seeder';

export class Loader {
    static seeder: Seeder;

    static async init(): Promise<void> {
        try {
            logger.info('Initializing Order Service modules...');

            // Initialize seeder
            this.seeder = new Seeder();

            // Register services in DI container
            Injector.Container.register('DatabaseConnector', { useClass: DatabaseConnector });

            logger.info('Order Service modules initialized successfully');
        } catch (error) {
            logger.error('Failed to initialize modules:', error);
            throw error;
        }
    }
}
