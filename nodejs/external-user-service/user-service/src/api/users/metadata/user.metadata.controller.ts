import express from 'express';
import { UserService } from "../../../services/users/user.service";
import { ResponseHandler } from "../../../common/handlers/response.handler";
import { BaseController } from "../../base.controller";
import { Injector } from "../../../startup/injector";
import { UserAuthService } from '../../../services/users/user.auth.service';
import { UserDeviceDetailsService } from '../../../services/users/user.device.details.service';
import { TenantService } from '../../../services/tenant/tenant.service';
import { UserMetadataService } from '../../../services/users/user.metadata.service';

////////////////////////////////////////////////////////////////

export class UserMetadataController extends BaseController {

    //#region member variables and constructors

    _service: UserAuthService = Injector.Container.resolve(UserAuthService);

    _userService: UserService = Injector.Container.resolve(UserService);

    _userDeviceDetailsService = Injector.Container.resolve(UserDeviceDetailsService);

    _tenantService: TenantService = Injector.Container.resolve(TenantService);

    _metadataService: UserMetadataService = Injector.Container.resolve(UserMetadataService);

    constructor() {
        super();
    }

    //#endregion

    getUserMetadata = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            const result = await this._metadataService.getUserMetadata(userId);
            if (result == null) {
                ResponseHandler.failure(request, response, 'User not found!', 404);
                return;
            }
            ResponseHandler.success(request, response, 'User metadata retrieved successfully!', 200, result, true);
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    updateUserMetadata = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            const updates = request.body;
            const result = await this._metadataService.update(userId, updates);
            ResponseHandler.success(request, response, 'User metadata updated successfully!', 200, result, true);
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    getCurrentTimeZone = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            const result = await this._metadataService.getCurrentTimeZone(userId);
            ResponseHandler.success(request, response, 'Current time zone retrieved successfully!', 200, result, true);
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    getDefaultTimeZone = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            const result = await this._metadataService.getDefaultTimeZone(userId);
            ResponseHandler.success(request, response, 'Default time zone retrieved successfully!', 200, result, true);
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    getPreferredLanguage = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            const result = await this._metadataService.getPreferredLanguage(userId);
            ResponseHandler.success(request, response, 'Preferred language retrieved successfully!', 200, result, true);
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    getUserSettings = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            const result = await this._metadataService.getUserSettings(userId);
            ResponseHandler.success(request, response, 'User settings retrieved successfully!', 200, result, true);
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    getDisplayName = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            const result = await this._metadataService.getDisplayName(userId);
            ResponseHandler.success(request, response, 'Display name retrieved successfully!', 200, result, true);
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    getLastLogin = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            const result = await this._metadataService.getLastLogin(userId);
            ResponseHandler.success(request, response, 'Last login retrieved successfully!', 200, result, true);
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    isTestUser = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            const result = await this._metadataService.isTestUser(userId);
            ResponseHandler.success(request, response, 'Is test user retrieved successfully!', 200, result, true);
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    updateCurrentTimeZone = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            const currentTimeZone = this.sanitizeTimezone(request.body.CurrentTimeZone);
            const result = await this._metadataService.updateCurrentTimeZone(userId, currentTimeZone);
            ResponseHandler.success(request, response, 'Current time zone updated successfully!', 200, result, true);
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    updateDefaultTimeZone = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            const defaultTimeZone = this.sanitizeTimezone(request.body.DefaultTimeZone);
            const result = await this._metadataService.updateDefaultTimeZone(userId, defaultTimeZone);
            ResponseHandler.success(request, response, 'Default time zone updated successfully!', 200, result, true);
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    updateDisplayName = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            const displayName = request.body.DisplayName;
            const result = await this._metadataService.updateDisplayName(userId, displayName);
            ResponseHandler.success(request, response, 'Display name updated successfully!', 200, result, true);
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    updatePreferredLanguage = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            const preferredLanguage = request.body.PreferredLanguage;
            const result = await this._metadataService.updatePreferredLanguage(userId, preferredLanguage);
            ResponseHandler.success(request, response, 'Preferred language updated successfully!', 200, result, true);
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    updateUserSettings = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            const settings = request.body;
            const result = await this._metadataService.updateUserSettings(userId, settings);
            ResponseHandler.success(request, response, 'User settings updated successfully!', 200, result, true);
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    deleteUserMetadata = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.userId;
            await this._metadataService.delete(userId);
            ResponseHandler.success(request, response, 'User metadata deleted successfully!', 200, null, true);
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    private sanitizeTimezone = (inputString: string): string => {
        const parts = inputString.split(':');

        if (parts.length < 3) {
            return inputString;
        }

        const extractedString = parts.slice(0, 2).join(':');
        return extractedString;
    };

}
