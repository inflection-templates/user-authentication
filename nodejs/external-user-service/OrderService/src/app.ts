import cors from 'cors';
import express from 'express';
import helmet from 'helmet';
import "reflect-metadata";
import { logger } from './common/logger';
import { DatabaseConnector } from './database/database.connector';
import { Loader } from './startup/loader';
import { Injector } from './startup/injector';
import { errorHandlerMiddleware } from './middlewares/error.handling.middleware';
import { Router } from './api/router';
import { getJwtConfiguration } from './auth/jwt.configuration';

/////////////////////////////////////////////////////////////////////////

export default class Application {

    public _app: express.Application = null;

    private _router: Router = null;

    private static _instance: Application = null;

    private constructor() {
        this._app = express();
        this._router = new Router(this._app);
    }

    public static instance(): Application {
        return this._instance || (this._instance = new this());
    }

    public app(): express.Application {
        return this._app;
    }

    public start = async(): Promise<void> => {
        try {

            //Load the modules
            await Loader.init();

            //Connect databases
            await this.connectDatabase();

            //Initialize JWT authentication with caching
            await this.initializeJwtAuthentication();

            //Set-up middlewares
            await this.setupMiddlewares();

            //Set the routes
            await this._router.init();

            //Seed the service
            await Loader.seeder.init();

            if (process.env.NODE_ENV !== 'test') {
                //Set-up cron jobs (if needed)
                // await Loader.scheduler.schedule();
            }

            this._app.use(errorHandlerMiddleware);

            //Handle unhandled rejections
            process.on('unhandledRejection', (reason, promise) => {
                logger.info('Unhandled Rejection!');
                promise.catch(error => {
                    logger.info(`Unhandled Rejection at: ${error.message}`);
                });
            });

            process.on('exit', code => {
                logger.info(`Process exited with code: ${code}`);
            });

            process.on('SIGINT', async () => {
                logger.info('Received SIGINT. Graceful shutdown...');
                await this.shutdown();
                process.exit(0);
            });

            process.on('SIGTERM', async () => {
                logger.info('Received SIGTERM. Graceful shutdown...');
                await this.shutdown();
                process.exit(0);
            });

            //Start listening
            await this.listen();

        }
        catch (error){
            logger.info('An error occurred while starting order service.' + error.message);
        }
    };

    private setupMiddlewares = async (): Promise<boolean> => {

        return new Promise((resolve, reject) => {
            try {
                this._app.use(express.urlencoded({ limit: '50mb', extended: true }));
                this._app.use(express.json( { limit: '50mb' }));
                this._app.use(helmet());
                this._app.use(cors());

                resolve(true);
            }
            catch (error) {
                reject(error);
            }
        });
    };

    private initializeJwtAuthentication = async (): Promise<void> => {
        try {
            logger.info('Initializing JWT authentication with caching...');
            const jwtConfig = getJwtConfiguration();
            await jwtConfig.startBackgroundServices();
            logger.info('JWT authentication services initialized');
        } catch (error) {
            logger.error(`Error initializing JWT authentication: ${error.message}`);
            throw error;
        }
    };

    private listen = () => {
        return new Promise((resolve, reject) => {
            try {
                const port = process.env.PORT || 5001;
                const server = this._app.listen(port, () => {
                    const serviceName = 'Order Service' + '-' + (process.env.NODE_ENV || 'development');
                    logger.info(`Operating system: ${process.platform}`);
                    logger.info(serviceName + ' is up and listening on port ' + port.toString());
                    logger.info(`Health check: http://localhost:${port}/health`);
                    logger.info(`API Documentation: http://localhost:${port}/api-docs`);
                    this._app.emit("server_started");
                });
                module.exports.server = server;
                resolve(this._app);
            }
            catch (error) {
                reject(error);
            }
        });
    };

    private async connectDatabase() {
        if (process.env.NODE_ENV === 'test') {
            // Drop test database if needed
        }
        const databaseConnector = Injector.Container.resolve(DatabaseConnector);
        await databaseConnector.init();
    }

    private shutdown = async (): Promise<void> => {
        try {
            logger.info('Shutting down Order Management Service...');
            
            // Stop JWT background services
            const jwtConfig = getJwtConfiguration();
            await jwtConfig.stopBackgroundServices();
            logger.info('JWT background services stopped');
            
            // Close database connections
            const databaseConnector = Injector.Container.resolve(DatabaseConnector);
            await databaseConnector.close();
            logger.info('Database connections closed');
            
            logger.info('Order Management Service shutdown complete');
        } catch (error) {
            logger.error(`Error during shutdown: ${error.message}`);
        }
    };

}
