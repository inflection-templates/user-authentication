import pino from 'pino';
import { AbstrctPinoLogger } from './abstract.pino.logger';
import dayjs from 'dayjs';

//Please refer for further customization:
// https://betterstack.com/community/guides/logging/how-to-install-setup-and-use-pino-to-log-node-js-applications/

///////////////////////////////////////////////////////////////////////

export class PinoDebugLogger extends AbstrctPinoLogger {

    constructor() {
        super();

        this._logger = pino({
            level     : 'debug',
            transport : {
                target  : 'pino-pretty',
                //target : this._logFile
                options : {
                    colorize        : true,
                    append          : true,
                    colorizeObjects : true,
                },
            },
            // formatters : {
            //     level : (label) => {
            //         return { level: label };
            //     },
            // },
            timestamp : () => `,"time":"${dayjs().format('YYYY-MM-DD HH:mm:ss')}"`,
            // serializers : {
            //     req : (req) => {
            //         return {
            //             method : req.method,
            //             url    : req.url,
            //             params : req.params,
            //             query  : req.query,
            //             client : req.currentClient,
            //             user   : req.currentUser,
            //             ips    : req.ips,
            //         };
            //     },
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
