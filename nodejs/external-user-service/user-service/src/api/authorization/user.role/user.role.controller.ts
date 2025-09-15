import express from 'express';
import { ResponseHandler } from '../../../common/handlers/response.handler';
import { Injector } from '../../../startup/injector';
import { ApiError } from '../../../common/api.error';
import { BaseController } from '../../base.controller';
import { uuid } from '../../../domain.types/miscellaneous/system.types';
import { UserRoleService } from '../../../services/authorization/user.role.service';
import { UserRoleValidator } from './user.role.validator';
import { UserService } from '../../../services/users/user.service';
import { RoleService } from '../../../services/authorization/role.service';

///////////////////////////////////////////////////////////////////////////////////////

export class UserRoleController extends BaseController {

    //#region member variables and constructors

    _service: UserRoleService = Injector.Container.resolve(UserRoleService);

    _userService: UserService = Injector.Container.resolve(UserService);

    _roleService: RoleService = Injector.Container.resolve(RoleService);

    _validator: UserRoleValidator = new UserRoleValidator();

    constructor() {
        super();
    }

    //#endregion

    //#region Action methods

    addUserRole = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            await this._validator.addUserRole(request);
            const userId: uuid = request.params.userId;
            const roleId: uuid = request.params.roleId;
            const user = await this._userService.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found.');
            }
            const tenantId = user.Tenant.id;
            await this.authorizeOne(request, null, tenantId);
            const role = await this._roleService.getById(roleId);
            if (role == null) {
                throw new ApiError(404, 'Role not found.');
            }
            const roleTenantId = role.TenantId;
            if (roleTenantId !== tenantId && role.IsSystemRole === false) {
                throw new ApiError(400, 'Role tenant does not match user tenant.');
            }
            const userRole = await this._service.addUserRole(userId, roleId, tenantId);
            if (userRole == null) {
                throw new ApiError(400, 'Cannot add user role!');
            }
            ResponseHandler.success(request, res, 'User role added successfully!', 201, {
                UserRole : userRole,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    getUserRoles = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            await this._validator.getUserRoles(request);
            const userId: uuid = request.params.userId;
            const userRoles = await this._service.getUserRoles(userId);
            if (userRoles == null) {
                throw new ApiError(404, 'User roles not found.');
            }
            if (userRoles.length === 0) {
                throw new ApiError(404, 'User roles not found.');
            }
            await this.authorizeOne(request, null, userRoles[0].TenantId);
            ResponseHandler.success(request, res, 'User roles retrieved successfully!', 200, {
                UserRoles : userRoles,
            });
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    removeAllUserRoles = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            await this._validator.removeAllUserRoles(request);
            const userId: uuid = request.params.userId;
            const user = await this._userService.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found.');
            }
            const tenantId = user.Tenant.id;
            await this.authorizeOne(request, null, tenantId);
            await this._service.removeAllUserRoles(userId);
            ResponseHandler.success(request, res, 'All user roles removed successfully!', 200);
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    removeUserRole = async (request: express.Request, res: express.Response): Promise<void> => {
        try {
            await this._validator.removeUserRole(request);
            const userId: uuid = request.params.userId;
            const roleId: uuid = request.params.roleId;
            const user = await this._userService.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found.');
            }
            const tenantId = user.Tenant.id;
            await this.authorizeOne(request, null, tenantId);
            const role = await this._roleService.getById(roleId);
            if (role == null) {
                throw new ApiError(404, 'Role not found.');
            }
            const roleTenantId = role.TenantId;
            if (roleTenantId !== tenantId && role.IsSystemRole === false) {
                throw new ApiError(400, 'Role tenant does not match user tenant.');
            }
            await this._service.removeUserRole(userId, roleId);
            ResponseHandler.success(request, res, 'User role removed successfully!', 200);
        } catch (error) {
            ResponseHandler.handleError(request, res, error);
        }
    };

    //#endregion

}
