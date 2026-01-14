import * as cron from 'node-cron';
import * as CronSchedules from '../../seed.data/cron.schedules.json';
import { logger } from '../logger/logger';
import { Injector } from './injector';
import { UserService } from '../services/users/user.service';
import { RunOnceScheduler } from '../modules/run.once.scripts/run.once.scheduler';

///////////////////////////////////////////////////////////////////////////
export class Scheduler {

    //#region Static privates

    private static _instance: Scheduler = null;

    private static _schedules = null;

    private constructor() {
        const env = process.env.NODE_ENV || 'development';
        Scheduler._schedules = CronSchedules[env];
        logger.info('Initializing the schedular.');
    }

    //#endregion

    //#region Publics

    public static instance(): Scheduler {
        return this._instance || (this._instance = new this());
    }

    public schedule = async (): Promise<boolean> => {
        return new Promise((resolve, reject) => {
            try {
                // this.scheduleCurrentTimezoneUpdate();
                const runOnceScheduler = RunOnceScheduler.instance();
                runOnceScheduler.schedule(Scheduler._schedules);

                resolve(true);
            } catch (error) {
                logger.info('Error initializing the scheduler.: ' + error.message);
                reject(false);
            }
        });
    };

    //#endregion

    //#region Privates


    // private scheduleCurrentTimezoneUpdate = () => {
    //     cron.schedule(Scheduler._schedules['ScheduleTimezoneUpdate'], () => {
    //         (async () => {
    //             logger.info('Running scheducled jobs: Update Current timezone...');
    //             var service = Injector.Container.resolve(UserService);
    //             await service.updateCurrentTimezone();
    //         })();
    //     });
    // };

    //#endregion

}
