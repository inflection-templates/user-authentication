import express from 'express';
import { logger } from '../../../logger/logger';
import { IUserAuthorizer } from '../interfaces/user.authorizer.interface';
import { Injector } from '../../../startup/injector';
import { UserService } from '../../../services/users/user.service';
import { RolePermissionService } from '../../../services/authorization/role.permission.service';
import { PermissionHandler } from './permission.handler';
import { ActionScope } from '../auth.types';

//////////////////////////////////////////////////////////////

export class CustomUserAuthorizer implements IUserAuthorizer {

    _userService: UserService = null;

    _rolePermissionService: RolePermissionService = null;

    constructor() {
        this._userService = Injector.Container.resolve(UserService);
        this._rolePermissionService = Injector.Container.resolve(RolePermissionService);
    }

    public authorize = async (request: express.Request): Promise<boolean> => {
        try {

            const context = request.context;
            if (context == null || context === 'undefined') {
                logger.info('Authorizer: Request context is not set.');
                return false;
            }

            // Temp solution - Needs to be refined
            if (request.currentClient?.IsPrivileged) {
                // If the client is privileged, then allow access to all resources
                logger.info('Authorizer: Privileged client access granted.');
                return true;
            }

            const publicAccess = request.actionScope === ActionScope.Public;
            const optionalUserAuth = request.optionalUserAuth;
            const allowUnauthorized = publicAccess || optionalUserAuth;

            const currentUser = request.currentUser ?? null;
            if (!currentUser) {
                //If the user is not authenticated, then check if the resource access is public
                if (allowUnauthorized) {
                    // To check whether a particular resource is available for public access, e.g. a profile image download
                    logger.info('Authorizer: Public access granted.');
                    return true;
                }
                // If the resource is not public, then the user must be authenticated
                return false;
            }

            const hasPermission = await PermissionHandler.checkRoleBasedPermissions(request);
            if (!hasPermission) {
                logger.info('Authorizer: User does not have required permissions.');
                return false;
            }
            return hasPermission;

        } catch (error) {
            logger.info(error.message);
        }
        return false;
    };

}
