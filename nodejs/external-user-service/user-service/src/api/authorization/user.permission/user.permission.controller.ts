import express from 'express';
import { UserPermissionService } from '../../../services/authorization/user.permission.service';
import { ResponseHandler } from '../../../common/handlers/response.handler';
import { Injector } from '../../../startup/injector';
import { UserPermissionValidator } from './user.permission.validator';
import { ApiError } from '../../../common/api.error';
import { BaseController } from '../../base.controller';
import { uuid } from '../../../domain.types/miscellaneous/system.types';
import { UserService } from '../../../services/users/user.service';

///////////////////////////////////////////////////////////////////////////////////////

export class UserPermissionController extends BaseController {

    //#region member variables and constructors

    _service: UserPermissionService = Injector.Container.resolve(UserPermissionService);

    _userService: UserService = Injector.Container.resolve(UserService);

    _validator: UserPermissionValidator = new UserPermissionValidator();

    constructor() {
        super();
    }

    //#endregion

    //#region Action methods

    add = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const userId: uuid = await this._validator.getParamUuid(request, 'userId');
            const permissionId: uuid = await this._validator.getParamUuid(request, 'permissionId');
            const scope: string = request.body.Scope ?? null;
            const user = await this._userService.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found!');
            }
            await this.authorizeOne(request, null, user.Tenant.id);
            const userPermission = await this._service.addAddonPermission(userId, permissionId, scope);
            if (userPermission == null) {
                throw new ApiError(400, 'Cannot add permission!');
            }
            ResponseHandler.success(request, res, 'Permission added successfully!', 201, {
                UserPermission : userPermission,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    remove = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const userId: uuid = await this._validator.getParamUuid(request, 'userId');
            const permissionId: uuid = await this._validator.getParamUuid(request, 'permissionId');
            const user = await this._userService.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found!');
            }
            await this.authorizeOne(request, null, user.Tenant.id);
            const success: boolean = await this._service.removeAddonPermission(userId, permissionId);
            ResponseHandler.success(request, res, 'Permission removed successfully!', 200, {
                Success : success,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    getAddonPermissions = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const userId: uuid = await this._validator.getParamUuid(request, 'userId');
            const user = await this._userService.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found!');
            }
            await this.authorizeOne(request, null, user.Tenant.id);
            const permissions = await this._service.getAddonPermissions(userId);
            if (permissions == null) {
                throw new ApiError(404, 'User permissions not found.');
            }
            ResponseHandler.success(request, res, 'User permissions retrieved successfully!', 200, {
                UserAddonPermissions : permissions,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    grant = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const userId: uuid = await this._validator.getParamUuid(request, 'userId');
            const permissionId: uuid = await this._validator.getParamUuid(request, 'permissionId');
            const user = await this._userService.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found!');
            }
            await this.authorizeOne(request, null, user.Tenant.id);
            const userPermission = await this._service.grant(userId, permissionId);
            if (userPermission == null) {
                throw new ApiError(400, 'Cannot grant permission!');
            }
            ResponseHandler.success(request, res, 'Permission granted successfully!', 200, {
                UserPermission : userPermission,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    revoke = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const userId: uuid = await this._validator.getParamUuid(request, 'userId');
            const permissionId: uuid = await this._validator.getParamUuid(request, 'permissionId');
            const userPermission = await this._service.revoke(userId, permissionId);
            if (userPermission == null) {
                throw new ApiError(400, 'Cannot revoke permission!');
            }
            await this.authorizeOne(request, null, userId);
            ResponseHandler.success(request, res, 'Permission revoked successfully!', 200, {
                UserPermission : userPermission,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    updateScope = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const userId: uuid = await this._validator.getParamUuid(request, 'userId');
            const permissionId: uuid = await this._validator.getParamUuid(request, 'permissionId');
            const scope: string = request.body.Scope ?? null;
            const userPermission = await this._service.updatePermissionScope(userId, permissionId, scope);
            if (userPermission == null) {
                throw new ApiError(400, 'Cannot update permission scope!');
            }
            await this.authorizeOne(request, null, userId);
            ResponseHandler.success(request, res, 'Permission scope updated successfully!', 200, {
                UserPermission : userPermission,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    getGranted = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const userId: uuid = await this._validator.getParamUuid(request, 'userId');
            const user = await this._userService.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found!');
            }
            await this.authorizeOne(request, null, user.Tenant.id);
            const userPermissions = await this._service.getGranted(userId);
            ResponseHandler.success(request, res, 'Granted user permissions retrieved successfully!', 200, {
                GrantedPermissions : userPermissions,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    getRevoked = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const userId: uuid = await this._validator.getParamUuid(request, 'userId');
            const user = await this._userService.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found!');
            }
            await this.authorizeOne(request, null, user.Tenant.id);
            const userPermissions = await this._service.getRevoked(userId);
            ResponseHandler.success(request, res, 'Revoked user permissions retrieved successfully!', 200, {
                RevokedPermissions : userPermissions,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    getRoleBased = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const userId: uuid = await this._validator.getParamUuid(request, 'userId');
            await this.authorizeOne(request, null, userId);
            const roleBasedPermissions = await this._service.getRoleBased(userId);
            ResponseHandler.success(request, res, 'Role-based permissions retrieved successfully!', 200, {
                RoleBasedPermissions : roleBasedPermissions,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    getAll = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            const userId: uuid = await this._validator.getParamUuid(request, 'userId');
            await this.authorizeOne(request, null, userId);
            const allPermissions = await this._service.getAll(userId);
            ResponseHandler.success(request, res, 'All user permissions retrieved successfully!', 200, {
                AllPermissions : allPermissions,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    //#endregion

}
