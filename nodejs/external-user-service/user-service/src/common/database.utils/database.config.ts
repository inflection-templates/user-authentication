import { DatabaseDialect } from "../../domain.types/miscellaneous/system.types";
import * as dotenv from 'dotenv';
import { logger } from "../../logger/logger";
import { ConfigurationManager } from "../../config/configuration.manager";

/////////////////////////////////////////////////////////////////////////////

export enum DatabaseSchemaType {
    Primary = 'Primary',
    EHRInsights = 'EHRInsights',
    AwardsFacts = 'AwardsFacts'
}

export interface DatabaseConfiguration {
    Dialect         : DatabaseDialect;
    DatabaseName    : string;
    Host            : string;
    Port            : number;
    Username        : string;
    Password        : string;
    ConnectionString: string;
    Pool            : {
        Max    : number,
        Min    : number,
        Acquire: number,
        Idle   : number,
    },
    Cache      : boolean;
    Synchronize: boolean;
    Logging    : boolean;
}

export const databaseConfig = (schemaType: DatabaseSchemaType)
    : DatabaseConfiguration => {

    const dialect = process.env.DB_DIALECT as DatabaseDialect;
    const config: DatabaseConfiguration = {
        Dialect          : dialect,
        DatabaseName     : process.env.DB_NAME,
        Host             : process.env.DB_HOST,
        Port             : parseInt(process.env.DB_PORT),
        Username         : process.env.DB_USER_NAME,
        Password         : process.env.DB_USER_PASSWORD,
        ConnectionString : process.env.CONNECTION_STRING ?? null,
        Pool             : {
            Max     : 20,
            Min     : 0,
            Acquire : 30000,
            Idle    : 10000,
        },
        Cache       : true,
        Logging     : true,
        Synchronize : true
    };

    return config;
};

//////////////////////////////////////////////////////////////////////////////////

if (typeof process.env.NODE_ENV === 'undefined') {
    dotenv.config();
}

logger.info('================================================');
logger.info('Environment                : ' + process.env.NODE_ENV);
logger.info('Database dialect           : ' + process.env.DB_DIALECT);
logger.info('Database host              : ' + process.env.DB_HOST);
logger.info('Database port              : ' + process.env.DB_PORT);
logger.info('Database user-name         : ' + process.env.DB_USER_NAME);
logger.info('Primary database name      : ' + process.env.DB_NAME);
logger.info('================================================');

//////////////////////////////////////////////////////////////////////////////////

