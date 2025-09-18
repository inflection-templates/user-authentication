import 'reflect-metadata';
import { IPrimaryDatabaseConnector } from './database.connector.interface';
import { injectable, inject } from 'tsyringe';
import { logger } from '../logger/logger';

////////////////////////////////////////////////////////////////////////

@injectable()
export class PrimaryDatabaseConnector {

    constructor(@inject('IPrimaryDatabaseConnector') private _db: IPrimaryDatabaseConnector) {}

    public init = async (): Promise<boolean> => {
        try {
            return await this._db.connect();
        } catch (error) {
            logger.info('Create database error: ' + error.message);
            return false;
        }
    };

    public sync = async (): Promise<boolean> => {
        try {
            await this._db.sync();
            return true;
        } catch (error) {
            logger.info('Sync database error: ' + error.message);
            return false;
        }
    };

    public migrate = async (): Promise<boolean> => {
        try {
            await this._db.migrate();
            return true;
        } catch (error) {
            logger.info('Migrate database error: ' + error.message);
            return false;
        }
    };

}

////////////////////////////////////////////////////////////////////////
