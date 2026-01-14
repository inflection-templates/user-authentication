import 'reflect-metadata';
import { container } from 'tsyringe';
import { logger } from '../logger/logger';
import { MessagingService } from '../modules/communication/messaging.service/messaging.service';
import { Injector } from './injector';
import { Scheduler } from './scheduler';
import { Seeder } from './seeder';
import { ConfigurationManager } from '../config/configuration.manager';

//////////////////////////////////////////////////////////////////////////////////////////////////

export class Loader {

    private static _seeder: Seeder = null;

    private static _scheduler: Scheduler = Scheduler.instance();

    private static _messagingService: MessagingService = null;

    public static get seeder() {
        return Loader._seeder;
    }

    public static get scheduler() {
        return Loader._scheduler;
    }
 public static get messagingService() {
        return Loader._messagingService;
    }

    public static init = async (): Promise<boolean> => {
        try {

            //Register injections here...
            Injector.registerInjections();

            Loader._seeder = container.resolve(Seeder);

            Loader._messagingService = container.resolve(MessagingService);
            Loader._messagingService.init();

            return true;

        } catch (error) {
            logger.info(error.message);
            return false;
        }
    };

}
