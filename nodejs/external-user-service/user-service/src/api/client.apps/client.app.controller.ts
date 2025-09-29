import express from 'express';

import { ClientAppService } from '../../services/client.apps/client.app.service';
import { ResponseHandler } from '../../common/handlers/response.handler';
import { ClientAppValidator } from './client.app.validator';
import { ApiError } from '../../common/api.error';
import { Injector } from '../../startup/injector';
import { ClientAppDto, ClientAppCreateModel } from '../../domain.types/client.apps/client.app.types';
import { BaseController } from '../base.controller';

///////////////////////////////////////////////////////////////////////////////////////

export class ClientAppController extends BaseController {

    //#region member variables and constructors

    _service: ClientAppService = Injector.Container.resolve(ClientAppService);

    constructor() {
        super();
    }

    //#endregion

    create = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const model: ClientAppCreateModel = await ClientAppValidator.create(request);
            await this.authorizeOne(request);
            const record = await this._service.create(model);
            if (record == null) {
                throw new ApiError(400, 'Unable to create client app.');
            }
            ResponseHandler.success(request, response, 'Client app added successfully!', 201, {
                Client : record,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    getById = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const id: string = await ClientAppValidator.getById(request);
            const record = await this._service.getById(id);
            if (record == null) {
                throw new ApiError(404, 'Client not found.');
            }
            await this.authorizeOne(request);
            ResponseHandler.success(request, response, 'Client app retrieved successfully!', 200, {
                Client : record,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    search = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const filters = await ClientAppValidator.search(request);
            this.authorizeSearch(request);
            const searchResults = await this._service.search(filters);
            const count = searchResults.Items.length;
            const message =
                count === 0 ? 'No records found!' : `Total ${count} client app records retrieved successfully!`;
            ResponseHandler.success(request, response, message, 200, {
                ClientAppRecords : searchResults,
            });

        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    update = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const id: string = await ClientAppValidator.getById(request);
            const domainModel = await ClientAppValidator.update(request);
            await this.authorizeOne(request);
            const record = await this._service.update(id, domainModel);
            if (record == null) {
                throw new ApiError(404, 'Client app not found.');
            }
            ResponseHandler.success(request, response, 'Client app updated successfully!', 200, {
                Client : record,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    delete = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const id: string = await ClientAppValidator.getById(request);
            await this.authorizeOne(request);
            await this._service.delete(id);
            ResponseHandler.success(request, response, 'Client app deleted successfully!', 200, null);
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    getCurrentApiKey = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const verificationModel = await ClientAppValidator.authenticateClientPassword(request);
            const apiKeyDto = await this._service.getApiKey(verificationModel);
            if (apiKeyDto == null) {
                throw new ApiError(400, 'Unable to retrieve client app api key.');
            }
            ResponseHandler.success(request, response, 'Client app api keys retrieved successfully!', 200, {
                ApiKeyDetails : apiKeyDto,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    authenticateClientPassword = async (request: express.Request): Promise<ClientAppDto> => {
        const verificationModel = await ClientAppValidator.authenticateClientPassword(request);
        const client = await this._service.authenticateClientPassword(verificationModel);
        return client;
    };

    renewApiKey = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const verificationModel = await ClientAppValidator.authenticateClientPassword(request);
            if (verificationModel.ValidFrom == null) {
                verificationModel.ValidFrom = new Date();
            }
            if (verificationModel.ValidTill == null) {
                const d = new Date();
                d.setFullYear(d.getFullYear() + 1);
                verificationModel.ValidTill = d;
            }
            const apiKeyDto = await this._service.renewApiKey(verificationModel);
            if (apiKeyDto == null) {
                throw new ApiError(400, 'Unable to renew client app api key.');
            }
            ResponseHandler.success(request, response, 'Client app api keys renewed successfully!', 200, {
                ApiKeyDetails : apiKeyDto,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    authorizeSearch = (request: express.Request) => {
        // Simplified authorization - just check if user is authenticated
        const currentUser = request.currentUser;
        if (!currentUser) {
            throw new ApiError(403, `Unauthorized`);
        }
    };

}
