/**
 * Database seeder for Order Service
 * Seeds initial data similar to the Python version
 */

import { logger } from '../common/logger';
import { Customer, Product } from '../models/entities';

export class Seeder {
    async init(): Promise<void> {
        try {
            logger.info('Starting database seeding...');

            await this.seedCustomers();
            await this.seedProducts();

            logger.info('Database seeding completed successfully');
        } catch (error) {
            logger.error('Database seeding failed:', error);
            throw error;
        }
    }

    private async seedCustomers(): Promise<void> {
        const existingCustomers = await Customer.count();
        if (existingCustomers > 0) {
            logger.info('Customers already exist, skipping customer seeding');
            return;
        }

        const customers = [
            {
                first_name: 'John',
                last_name: 'Doe',
                email: 'john.doe@example.com',
                phone: '+1234567890',
                address: '123 Main St, Anytown, USA'
            },
            {
                first_name: 'Jane',
                last_name: 'Smith',
                email: 'jane.smith@example.com',
                phone: '+0987654321',
                address: '456 Oak Ave, Somewhere, USA'
            }
        ];

        await Customer.bulkCreate(customers);
        logger.info(`Seeded ${customers.length} customers`);
    }

    private async seedProducts(): Promise<void> {
        const existingProducts = await Product.count();
        if (existingProducts > 0) {
            logger.info('Products already exist, skipping product seeding');
            return;
        }

        const products = [
            {
                name: 'Laptop',
                description: 'High-performance laptop for work and gaming',
                price: 1299.99,
                sku: 'LAP001',
                stock_quantity: 50,
                category: 'Electronics',
                is_active: true
            },
            {
                name: 'Wireless Mouse',
                description: 'Ergonomic wireless mouse with precision tracking',
                price: 29.99,
                sku: 'MOU001',
                stock_quantity: 100,
                category: 'Electronics',
                is_active: true
            },
            {
                name: 'Office Chair',
                description: 'Comfortable ergonomic office chair',
                price: 199.99,
                sku: 'CHR001',
                stock_quantity: 25,
                category: 'Furniture',
                is_active: true
            }
        ];

        await Product.bulkCreate(products);
        logger.info(`Seeded ${products.length} products`);
    }
}
