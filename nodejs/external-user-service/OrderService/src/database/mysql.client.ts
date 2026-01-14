/**
 * MySQL Database Client for OrderService
 * Based on user-service MySQL client pattern
 */

import mysql, { Connection } from 'mysql2/promise';
import { logger } from '../common/logger';

export interface DatabaseConfig {
    dialect: string;
    databaseName: string;
    host: string;
    port: number;
    username: string;
    password: string;
}

export class MysqlClient {
    private connection: Connection | null = null;
    private static instance: MysqlClient | null = null;

    private constructor() {}

    public static getInstance(): MysqlClient {
        return this.instance || (this.instance = new MysqlClient());
    }

    public getConfig(): DatabaseConfig {
        return {
            dialect: process.env.DB_DIALECT || 'mysql',
            databaseName: process.env.DB_NAME || 'auth_order_node',
            host: process.env.DB_HOST || 'localhost',
            port: parseInt(process.env.DB_PORT || '3306'),
            username: process.env.DB_USER_NAME || 'root',
            password: process.env.DB_USER_PASSWORD || 'root'
        };
    }

    public connect = async (): Promise<void> => {
        try {
            const config = this.getConfig();
            this.connection = await mysql.createConnection({
                database: config.databaseName,
                host: config.host,
                port: config.port,
                user: config.username,
                password: config.password,
            });
            logger.info(`Connected to MySQL database: ${config.databaseName}`);
        } catch (error) {
            logger.error(`Error connecting to database: ${error.message}`);
            throw error;
        }
    };

    public createDb = async (): Promise<boolean> => {
        try {
            const config = this.getConfig();
            const query = `CREATE DATABASE IF NOT EXISTS ${config.databaseName}`;
            const result = await this.execute(query);
            if (result) {
                logger.info(`Database '${config.databaseName}' created or already exists`);
            }
            return result;
        } catch (error) {
            logger.error(`Error creating database: ${error.message}`);
            return false;
        }
    };

    public dropDb = async (): Promise<boolean> => {
        try {
            const config = this.getConfig();
            const query = `DROP DATABASE IF EXISTS ${config.databaseName}`;
            const result = await this.execute(query);
            if (result) {
                logger.info(`Database '${config.databaseName}' dropped`);
            }
            return result;
        } catch (error) {
            logger.error(`Error dropping database: ${error.message}`);
            return false;
        }
    };

    public execute = async (query: string): Promise<boolean> => {
        let connection: Connection | null = null;
        try {
            const config = this.getConfig();

            // Create connection without specifying database for CREATE/DROP operations
            connection = await mysql.createConnection({
                host: config.host,
                port: config.port,
                user: config.username,
                password: config.password,
            });

            await connection.query(query);
            logger.debug(`Executed query: ${query}`);
            return true;
        } catch (error) {
            logger.error(`Error executing query: ${error.message}`);
            return false;
        } finally {
            if (connection) {
                await connection.end();
            }
        }
    };

    public executeQuery = async (query: string): Promise<any> => {
        try {
            if (!this.connection) {
                await this.connect();
            }
            const result = await this.connection!.query(query);
            return result;
        } catch (error) {
            logger.error(`Error executing query: ${error.message}`);
            throw error;
        }
    };

    public closeDbConnection = async (): Promise<void> => {
        try {
            if (this.connection) {
                await this.connection.end();
                this.connection = null;
                logger.info('Database connection closed');
            }
        } catch (error) {
            logger.error(`Error closing database connection: ${error.message}`);
        }
    };
}
