import * as cron from 'node-cron';
import { injectable } from 'tsyringe';
import { logger } from '../../logger/logger';
import { Injector } from '../../startup/injector';

////////////////////////////////////////////////////////////////////////////////////////////////////////

@injectable()
export class RunOnceScheduler {

    //#region member variables and constructors

    //#endregion

    static _instance: RunOnceScheduler = null;

    public static instance(): RunOnceScheduler {
        return this._instance || (this._instance = Injector.Container.resolve(RunOnceScheduler));
    }

    public schedule(schedules: any) {
        if (schedules && schedules.length > 0) {
            schedules.forEach((schedule: any) => {
                const cronSchedule = schedule.schedule;
                const task = cron.schedule(cronSchedule, async () => {
                    try {
                        logger.info(`Running scheduled task: ${schedule.name}`);
                        // await this._patientService[schedule.method]();
                    } catch (error) {
                        logger.info(`Error running scheduled task: ${schedule.name}`);
                    }
                });
                task.start();
            });
        } else {
            logger.info('No schedules found to run.');
        }
    }

}
