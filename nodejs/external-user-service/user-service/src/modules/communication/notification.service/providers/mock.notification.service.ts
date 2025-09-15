import { logger } from '../../../../logger/logger';
import { INotificationService } from '../notification.service.interface';

///////////////////////////////////////////////////////////////////////////////////

export class MockNotificationService implements INotificationService {

    init = () => {
        return true;
    };

    sendNotificationToDevice = async (deviceToken: string, message: any): Promise<string> => {
        try {
            message.token = deviceToken;
            if (deviceToken == null) {
                logger.info('Invalid device token!');
                return `Invalid device token`;
            }
            logger.info(`Successfully sent notification to token:  ${deviceToken}.`);
            return `Sent notification successfully`;
        } catch (error) {
            var errorMessage = `Error sending notification to token: ${deviceToken}.`;
            logger.error(errorMessage);
        }
    };

    sendNotificationToMultipleDevice = async (
        deviceTokens: string[],
        message: any): Promise<any> => {
        try {
            message.tokens = deviceTokens;
            logger.info(`Sending notification to tokens: ${deviceTokens}.`);
            logger.info(`Successfully sent notification to token: ${deviceTokens}.`);
            return `Sent notfication to multiple devices`;
        } catch (error) {
            var errorMessage = `Error sending notification to token: ${deviceTokens}.`;
            logger.error(errorMessage);
        }
    };

    sendMessageToTopic = async (topic: string, message: any): Promise<string> => {
        try {
            message.topic = topic;
            logger.info(`Sending notification to topic: ${topic}.`);
            logger.info(`Successfully sent notification to topic: ${topic}.`);
            return `Sent notification to topic successfully`;
        } catch (error) {
            var errorMessage = 'Error sending notification to topic: ' + topic;
            logger.error(errorMessage);
        }
    };

    formatNotificationMessage = (notificationType: string, title: string, body: any): any => {
        return {
            type  : notificationType,
            title : title,
            body  : body
        };
    };

    formatNotificationMessageWithData = (notificationType: string, title: string, body: any, customData: any): any => {
        return {
            type  : notificationType,
            title : title,
            body  : body,
            data  : customData
        };
    };

}
