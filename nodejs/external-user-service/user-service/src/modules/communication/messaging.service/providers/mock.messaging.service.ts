import { logger } from '../../../../logger/logger';
import { IMessagingService } from '../messaging.service.interface';

///////////////////////////////////////////////////////////////////////////////////

export class MockMessagingService implements IMessagingService {

    init(): boolean {
        logger.info(`Initialized mock messaging service!`);
        return true;
    }

    sendSMS = async (toPhone: string, message: string): Promise<boolean> => {
        try {
            logger.info(`To phone: '${toPhone}', Message: '${message}'`);
            return Promise.resolve(true);
        } catch (error) {
            logger.info(error.message);
            return false;
        }
    };

    sendWhatsappMessage = async (toPhone: string, message: string): Promise<boolean> => {
        try {
            logger.info(`To phone: '${toPhone}', Message: '${message}'`);
            return Promise.resolve(true);
        } catch (error) {
            logger.info(error.message);
            return false;
        }
    };

}
