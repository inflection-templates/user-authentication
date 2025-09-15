import { inject, injectable } from "tsyringe";
import { IMessagingService } from "./messaging.service.interface";
import needle = require('needle');
import { logger } from "../../../logger/logger";

////////////////////////////////////////////////////////////////////////

@injectable()
export class MessagingService {

    constructor(@inject('IMessagingService') private _service: IMessagingService) {}

    init = (): boolean => {
        return this._service.init();
    };

    sendSMS = async (toPhone: string, message: string): Promise<boolean> => {
        return await this._service.sendSMS(toPhone, message);
    };

    sendWhatsappMessage = async (toPhone: string, message: string): Promise<boolean> => {
        return await this._service.sendWhatsappMessage(toPhone, message);
    };

    sendTelegramMessage = async (telegramChatId: string, message: string, clientName: string,
        buttonsIds): Promise<boolean> => {
        await this.sendMessage(clientName, "telegram", telegramChatId, "inline_keyboard", null, message, buttonsIds);
        return true;
    };

    sendWhatsappWithReanBot = async (toPhone: string, message: any, provider:string,
        type:string, PlanCode:string, payload = null, channelName?: string ): Promise<boolean> => {

        let templateName = null;
        const channel = await this.getBotChannel(channelName);

        toPhone = toPhone.replace(/\D/g, '');
        message = JSON.parse(message);
        if (message.Variables || message.TemplateVariables) {
            if (message.TemplateVariables) {
                message.Variables = message.TemplateVariables;
                const buttonIds = message.TemplateButtonIds;
                if ((buttonIds != null && (Array.isArray(buttonIds))) ? buttonIds.length : false) {
                    message.ButtonsIds = message.TemplateButtonIds;
                }
            }
            templateName = type;
            type = "template";
        }
        message = JSON.stringify(message);
        await this.sendMessage(provider, channel, toPhone, type, templateName, message, payload);

        return true;
    };

    public async sendMessage (provider, channel, toPhone, type, templateName, message, payload) {
        const reanBotBaseUrl = process.env.REANBOT_BACKEND_BASE_URL;
        const urlToken = process.env.REANBOT_WEBHOOK_CLIENT_URL_TOKEN;
        const client = await this.getClientByProvider(provider);
        const headers = {
            'authentication' : process.env.REANBOT_WEBHOOK_CLIENT_HEADER_TOKEN,
        };
        const options = {
            headers : headers
        };

        const url = `${reanBotBaseUrl}${client}/${channel}/${urlToken}/send`;
        logger.info(`URL: ${url}`);
        logger.info(`PhoneNumber: ${toPhone}`);
        const obj = {
            userId       : toPhone,
            agentName    : "User service",
            provider     : provider,
            type         : type,
            templateName : templateName,
            message      : message,
            payload      : payload
        };
        logger.info(`Body of request: ${JSON.stringify(obj)}`);
        const resp1 = await needle('post', url, obj, options);
        if (resp1.statusCode !== 200) {
            logger.info(`Failed to send message to phone number: ${toPhone}`);
            return false;
        } else {
            return true;
        }
    }

    private getClientByProvider (provider) {
        const clientName = {
            "REAN_BW"        : "BLOOD_WARRIORS",
            "REAN"           : "MATERNAL_BOT",
            "KENYA_MATERNAL" : "KENYA_MATERNAL",
            "REAN_BOT"       : "REAN_BOT",
            "GMU"            : "GMU",
        };
        return clientName[provider] ?? provider;
    }

    private getBotChannel (channel) {
        const channnelName = {
            "WhatsApp"     : "whatsappMeta",
            "WhatsappWati" : "whatsappWati"
        };
        return channnelName[channel] ?? "whatsappMeta";
    }

}
