/**
 * Database connector for Order Service
 * Equivalent to the user-service database connector
 */

import { Sequelize } from 'sequelize-typescript';
import { logger } from '../common/logger';
import { Customer, Product, Order, OrderItem } from '../models/entities';
import { injectable } from 'tsyringe';
import { MysqlClient } from './mysql.client';

@injectable()
export class DatabaseConnector {
    private _sequelize: Sequelize | null = null;

    public get sequelize(): Sequelize {
        return this._sequelize!;
    }

    async init(): Promise<void> {
        try {
            logger.info('Initializing database connection...');

            // Create database client for database creation
            const mysqlClient = MysqlClient.getInstance();
            const dbConfig = mysqlClient.getConfig();

            logger.info(`Connecting to database: ${dbConfig.databaseName} on ${dbConfig.host}:${dbConfig.port}`);
            
            // Create database if it doesn't exist
            logger.info('Ensuring database exists...');
            const dbCreated = await mysqlClient.createDb();
            if (dbCreated) {
                logger.info(`Database '${dbConfig.databaseName}' is ready`);
            } else {
                logger.warn(`Could not create database '${dbConfig.databaseName}', it may already exist`);
            }
            
            // Initialize Sequelize with the existing/created database
            this._sequelize = new Sequelize(dbConfig.databaseName, dbConfig.username, dbConfig.password, {
                host: dbConfig.host,
                port: dbConfig.port,
                dialect: dbConfig.dialect as 'mysql' | 'postgres' | 'sqlite' | 'mariadb' | 'mssql',
                logging: process.env.NODE_ENV === 'development' ? (msg) => logger.debug(msg) : false,
                models: [Customer, Product, Order, OrderItem],
                pool: {
                    max: 10,
                    min: 0,
                    acquire: 30000,
                    idle: 10000
                },
                define: {
                    underscored: true,
                    freezeTableName: true
                }
            });

            // Test the connection
            await this._sequelize.authenticate();
            logger.info('Database connection established successfully');

            // Sync database schema
            await this._sequelize.sync({ 
                force: process.env.NODE_ENV === 'test',
                alter: process.env.NODE_ENV === 'development'
            });
            logger.info('Database schema synchronized');

        } catch (error) {
            logger.error('Unable to connect to the database:', error);
            throw error;
        }
    }

    async close(): Promise<void> {
        if (this._sequelize) {
            await this._sequelize.close();
            logger.info('Database connection closed');
        }
        
        // Also close the MySQL client connection
        const mysqlClient = MysqlClient.getInstance();
        await mysqlClient.closeDbConnection();
    }

    async dropDatabase(): Promise<void> {
        if (this._sequelize) {
            await this._sequelize.drop();
            logger.info('Database dropped');
        }
        
        // Also drop using MySQL client
        const mysqlClient = MysqlClient.getInstance();
        await mysqlClient.dropDb();
    }
}
