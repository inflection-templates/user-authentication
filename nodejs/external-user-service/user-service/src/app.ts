import cors from 'cors';
import express from 'express';
import helmet from 'helmet';
import "reflect-metadata";
import { Router } from './api/router';
import { Helper } from './common/helper';
import { logger } from './logger/logger';
import { PrimaryDatabaseConnector } from './database/database.connector';
import { Loader } from './startup/loader';
import { DatabaseClient } from './common/database.utils/dialect.clients/database.client';
import { DatabaseSchemaType } from './common/database.utils/database.config';
import { Injector } from './startup/injector';
import { errorHandlerMiddleware } from './middlewares/error.handling.middleware';

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
            await connectDatabase_Primary();

            //Set-up middlewares
            await this.setupMiddlewares();

            //Set the routes
            await this._router.init();

            //Seed the service
            await Loader.seeder.init();

            if (process.env.NODE_ENV !== 'test') {
                //Set-up cron jobs
                await Loader.scheduler.schedule();
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

            //Start listening
            await this.listen();

        }
        catch (error){
            logger.info('An error occurred while starting user service.' + error.message);
        }
    };

    private setupMiddlewares = async (): Promise<boolean> => {

        return new Promise((resolve, reject) => {
            try {
                this._app.use(express.urlencoded({ limit: '50mb', extended: true }));
                this._app.use(express.json( { limit: '50mb' }));
                this._app.use(helmet());
                this._app.use(cors());

                //TODO: Move this to upload specific routes. Use router.use() method
                // this.useFileUploadMiddleware();

                resolve(true);
            }
            catch (error) {
                reject(error);
            }
        });
    };

    private listen = () => {
        return new Promise((resolve, reject) => {
            try {
                const port = process.env.PORT;
                const server = this._app.listen(port, () => {
                    const serviceName = 'User Service' + '-' + process.env.NODE_ENV;
                    const osType = Helper.getOSType();
                    logger.info(`Operating system: ${osType}`);
                    logger.info(serviceName + ' is up and listening on port ' + process.env.PORT.toString());
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

}

async function connectDatabase_Primary() {
    if (process.env.NODE_ENV === 'test') {
        const databaseClient = Injector.Container.resolve(DatabaseClient);
        await databaseClient.dropDb(DatabaseSchemaType.Primary);
    }
    const primaryDatabaseConnector = Injector.Container.resolve(PrimaryDatabaseConnector);
    await primaryDatabaseConnector.init();
}
