import { Twilio } from 'twilio';
import { logger } from '../../../../logger/logger';
import { IMessagingService } from '../messaging.service.interface';

///////////////////////////////////////////////////////////////////////////////////

export class TwilioMessagingService implements IMessagingService {

    private _client: Twilio = null;

    init(): boolean {
        try {
            const account_sid = process.env.TWILIO_ACCOUNT_SID;
            const auth_token = process.env.TWILIO_AUTH_TOKEN;
            this._client = new Twilio(account_sid, auth_token);
        }
        catch (error) {
            logger.info(error.message);
            return false;
        }
        return true;
    }

    sendSMS = async (toPhone: string, message: string): Promise<boolean> => {
        try {
            logger.info(`To phone: '${toPhone}', Message: '${message}'`);

            // eslint-disable-next-line @typescript-eslint/no-unused-vars

            var from_phone_tmp = process.env.SYSTEM_INTERNATIONAL_PHONE_NUMBER;

            //If we are sending to US, use the US phone number to send
            var to_phone_tmp = toPhone.trim();
            if (to_phone_tmp.startsWith('+1')) {
                from_phone_tmp = process.env.SYSTEM_US_PHONE_NUMBER;
            }

            const smsResponse = await this._client.messages.create({
                body : message,
                from : from_phone_tmp,
                to   : to_phone_tmp,
            });

            logger.info(`SMS sent response: ${JSON.stringify(smsResponse, null, 2)}`);

            return true;

        } catch (error) {
            logger.info(error.message);
            return false;
        }
    };

    sendWhatsappMessage = async (toPhone: string, message: string): Promise<boolean> => {
        try {
            logger.info(`To phone: '${toPhone}', Message: '${message}'`);

            var from_phone_tmp = process.env.SYSTEM_INTERNATIONAL_WHATSAPP_NUMBER;

            //If we are sending to US, use the US phone number to send

            const smsResponse = await this._client.messages.create({
                to   : `whatsapp:${toPhone}`,
                body : message,
                from : `whatsapp:${from_phone_tmp}`
            });

            logger.info(`SMS sent response: ${JSON.stringify(smsResponse, null, 2)}`);

            // eslint-disable-next-line @typescript-eslint/no-unused-vars
            return new Promise((resolve, reject) => {
                logger.info('Twilio access details not available');
                resolve(true);
            });

        } catch (error) {
            logger.info(error.message);
            return false;
        }
    };

}
