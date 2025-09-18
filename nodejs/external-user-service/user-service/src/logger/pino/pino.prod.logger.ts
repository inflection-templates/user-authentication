import pino from 'pino';
import { AbstrctPinoLogger } from './abstract.pino.logger';
import dayjs from 'dayjs';

///////////////////////////////////////////////////////////////////////

export class PinoProdLogger extends AbstrctPinoLogger {

    constructor() {
        super();

        this._logger = pino({
            transport : {
                target : 'pino-pretty',
                //target : this._logFile
            },
            timestamp : () => `,"time":"${dayjs().format('YYYY-MM-DD HH:mm:ss')}"`,
        }
        //, pino.destination(this._logFile) // Another way to specify logfile
        );
        this._logger = pino({
            level     : 'warn',
            transport : {
                target  : 'pino-pretty',
                //target : this._logFile
                options : {
                    colorize        : true,
                    append          : true,
                    colorizeObjects : true,
                },
            },
            // transport : {
            //     target : 'pino-pretty',
            //     //target : this._logFile
            // },
        }
        //, pino.destination(this._logFile) // Another way to specify logfile
        );
    }

    info = (str: string) => {
        this._logger?.info(str);
    };

    error = (str: string) => {
        this._logger?.error(str);
    };

    warn = (str: string) => {
        this._logger?.warn(str);
    };

    debug = (str: string) => {
        this._logger?.debug(str);
    };

}
