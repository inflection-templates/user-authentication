import express from 'express';
import { RolePermissionService } from '../../../services/authorization/role.permission.service';
import { ResponseHandler } from '../../../common/handlers/response.handler';
import { Injector } from '../../../startup/injector';
import { RolePermissionValidator } from './role.permission.validator';
import { ApiError } from '../../../common/api.error';
import { BaseController } from '../../base.controller';
import { uuid } from '../../../domain.types/miscellaneous/system.types';

///////////////////////////////////////////////////////////////////////////////////////

export class RolePermissionController extends BaseController {

    //#region member variables and constructors

    _service: RolePermissionService = Injector.Container.resolve(RolePermissionService);

    _validator: RolePermissionValidator = new RolePermissionValidator();

    constructor() {
        super();
    }

    //#endregion

    //#region Action methods

    addPermission = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const roleId: uuid = await this._validator.getParamUuid(request, 'roleId');
            const permissionId: uuid = await this._validator.getParamUuid(request, 'permissionId');
            const scope: string = request.body.Scope ?? null;
            await this.authorizeOne(request, null, roleId);
            const rolePermission = await this._service.addPermission(roleId, permissionId, scope);
            if (rolePermission == null) {
                throw new ApiError(400, 'Cannot add permission to role!');
            }
            ResponseHandler.success(request, res, 'Permission added to role successfully!', 201, {
                RolePermission : rolePermission,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    getPermissions = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const roleId: uuid = await this._validator.getParamUuid(request, 'roleId');
            const rolePermissions = await this._service.getPermissionsForRole(roleId);
            if (rolePermissions == null) {
                throw new ApiError(404, 'Role permissions not found.');
            }
            await this.authorizeOne(request, null, roleId);
            ResponseHandler.success(request, res, 'Role permissions retrieved successfully!', 200, {
                RolePermissions : rolePermissions,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    removePermission = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const roleId: uuid = await this._validator.getParamUuid(request, 'roleId');
            const permissionId: uuid = await this._validator.getParamUuid(request, 'permissionId');
            await this.authorizeOne(request, null, roleId);
            const success: boolean = await this._service.removePermission(roleId, permissionId);
            ResponseHandler.success(request, res, 'Permission removed from role successfully!', 200, {
                Success : success,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    enablePermission = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const roleId: uuid = await this._validator.getParamUuid(request, 'roleId');
            const permissionId: uuid = await this._validator.getParamUuid(request, 'permissionId');
            const rolePermission = await this._service.enablePermission(roleId, permissionId);
            if (rolePermission == null) {
                throw new ApiError(400, 'Cannot enable permission for role!');
            }
            await this.authorizeOne(request, null, roleId);
            ResponseHandler.success(request, res, 'Permission enabled for role successfully!', 200, {
                RolePermission : rolePermission,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    disablePermission = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const roleId: uuid = await this._validator.getParamUuid(request, 'roleId');
            const permissionId: uuid = await this._validator.getParamUuid(request, 'permissionId');
            const rolePermission = await this._service.disablePermission(roleId, permissionId);
            if (rolePermission == null) {
                throw new ApiError(400, 'Cannot disable permission for role!');
            }
            await this.authorizeOne(request, null, roleId);
            ResponseHandler.success(request, res, 'Permission disabled for role successfully!', 200, {
                RolePermission : rolePermission,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    updatePermissionScope = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const roleId: uuid = await this._validator.getParamUuid(request, 'roleId');
            const permissionId: uuid = await this._validator.getParamUuid(request, 'permissionId');
            const scope: string = request.body.Scope ?? null;
            const rolePermission = await this._service.updatePermissionScope(roleId, permissionId, scope);
            if (rolePermission == null) {
                throw new ApiError(400, 'Cannot update permission scope for role!');
            }
            await this.authorizeOne(request, null, roleId);
            ResponseHandler.success(request, res, 'Permission scope updated for role successfully!', 200, {
                RolePermission : rolePermission,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    getForTenant = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const tenantId: uuid = await this._validator.getParamUuid(request, 'tenantId');
            const rolePermissions = await this._service.getForTenant(tenantId);
            if (rolePermissions == null) {
                throw new ApiError(404, 'Role permissions not found.');
            }
            await this.authorizeOne(request, null, tenantId);
            ResponseHandler.success(request, res, 'Role permissions retrieved successfully!', 200, {
                RolePermissions : rolePermissions,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    getSystemRolePermissions = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const rolePermissions = await this._service.getSystemRolePermissions();
            if (rolePermissions == null) {
                throw new ApiError(404, 'Role permissions not found.');
            }
            await this.authorizeOne(request, null, null);
            ResponseHandler.success(request, res, 'Role permissions retrieved successfully!', 200, {
                RolePermissions : rolePermissions,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    //#endregion

}
