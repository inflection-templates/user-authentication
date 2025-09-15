import express from 'express';
import { PermissionService } from '../../../services/authorization/permission.service';
import { ResponseHandler } from '../../../common/handlers/response.handler';
import { Injector } from '../../../startup/injector';
import { PermissionValidator } from './permission.validator';
import { ApiError } from '../../../common/api.error';
import { PermissionCreateModel } from "../../../domain.types/authorization/permission.types";
import { BaseController } from '../../base.controller';
import { uuid } from '../../../domain.types/miscellaneous/system.types';

///////////////////////////////////////////////////////////////////////////////////////

export class PermissionController extends BaseController {

    //#region member variables and constructors

    _service: PermissionService = Injector.Container.resolve(PermissionService);

    _validator: PermissionValidator = new PermissionValidator();

    constructor() {
        super();
    }

    //#endregion

    //#region Action methods

    create = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const model: PermissionCreateModel = await this._validator.create(request);
            await this.authorizeOne(request, null, model.TenantId);
            const permission = await this._service.create(model);
            if (permission == null) {
                throw new ApiError(400, 'Cannot create role!');
            }
            ResponseHandler.success(request, res, 'Permission created successfully!', 201, {
                Permission : permission,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    getById = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const permissionId: uuid = await this._validator.getParamUuid(request, 'id');
            const permission = await this._service.getById(permissionId);
            if (permission == null) {
                throw new ApiError(404, 'Permission not found.');
            }
            await this.authorizeOne(request, null, permission.TenantId);
            ResponseHandler.success(request, res, 'Permission retrieved successfully!', 200, {
                Permission : permission,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    search = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const filters = await this._validator.search(request);
            const permissions = await this._service.search(filters);
            ResponseHandler.success(request, res, 'Permissions retrieved successfully!', 200, {
                Permissions : permissions,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    update = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const permissionId: uuid = await this._validator.getParamUuid(request, 'id');
            const model = await this._validator.update(request);
            const permission = await this._service.getById(permissionId);
            if (permission == null) {
                throw new ApiError(404, 'Permission not found.');
            }
            await this.authorizeOne(request, null, permission.TenantId);
            const updatedPermission = await this._service.update(permissionId, model);
            if (updatedPermission == null) {
                throw new ApiError(400, 'Unable to update role!');
            }
            ResponseHandler.success(request, res, 'Permission updated successfully!', 200, {
                Permission : updatedPermission,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    delete = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const permissionId: uuid = await this._validator.getParamUuid(request, 'id');
            const permission = await this._service.getById(permissionId);
            if (permission == null) {
                throw new ApiError(404, 'Permission not found.');
            }
            await this.authorizeOne(request, null, permission.TenantId);
            const success: boolean = await this._service.delete(permissionId);
            if (!success) {
                throw new ApiError(400, 'Permission cannot be deleted.');
            }
            ResponseHandler.success(request, res, 'Permission deleted successfully!', 200, {
                success : success,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    //#endregion

}
