import express from 'express';
import { RolePermissionService } from '../../../services/authorization/role.permission.service';
import { Injector } from '../../../startup/injector';
import { ActionScope, RequestType, ResourceOwnership } from "../auth.types";
import { CurrentUser } from "../../../domain.types/miscellaneous/current.user";
import { DefaultRoleTypes } from "../../../domain.types/authorization/enums";
import { logger } from "../../../logger/logger";
import { PermissionService } from '../../../services/authorization/permission.service';

////////////////////////////////////////////////////////////////////////////////////////

export class PermissionHandler {

    public static checkRoleBasedPermissions = async (request: express.Request): Promise<boolean> => {
        const currentUser = request.currentUser;
        const roles = currentUser.Roles;
        const context = request.context;
        const rolePermissionService = Injector.Container.resolve(RolePermissionService);
        const permissionService = Injector.Container.resolve(PermissionService);
        for (const role of roles) {
            const permission = await permissionService.getByName(context);
            if (!permission) {
                return false;
            }
            const hasPermission = await rolePermissionService.hasPermissionForRole(role.id, permission.id);
            if (hasPermission) {
                return true;
            }
        }
        return false;
    };

    // Check permissions by ownership, action scope and consent
    public static checkFineGrained = async (request: express.Request): Promise<boolean> => {

        const currentUser = request.currentUser ?? null;
        const roles = currentUser.Roles;
        const roleNames = roles.map((role) => role.Name);

        // 2. SuperAdmin (System Admin) has access to all resources
        if (roleNames.includes(DefaultRoleTypes.SystemAdmin)) {
            return true;
        }

        // 3. SystemUser
        // System user access has already been checked for role based permissions
        if (roleNames.includes(DefaultRoleTypes.SystemUser)) {
            const msg = `System User access has already been checked for role based permissions`;
            logger.info(msg);
            return true;
        }

        if (roleNames.includes(DefaultRoleTypes.TenantAdmin)) {
            // Tenant Admin has access to all resources in the tenant scope
            if (request.resourceTenantId === currentUser.TenantId
              && request.actionScope === ActionScope.Tenant) {
                return true;
            }
        }

        return await this.checkByRequestType(request, currentUser);
    };

    private static checkScopeWithOwnership = (
        ownership: ResourceOwnership,
        actionScope: ActionScope,
        isOwner: boolean,
        areTenantsSame: boolean,
    ): boolean => {
        //visible to the individual owner only
        if (ownership === ResourceOwnership.Owner) {
            if (actionScope === ActionScope.Owner) {
                return isOwner;
            }
            if (actionScope === ActionScope.Tenant) {
                return areTenantsSame;
            }
            if (actionScope === ActionScope.System) {
                return true;
            }
            return false;
        } else if (ownership === ResourceOwnership.Tenant) {
            return areTenantsSame;
        } else if (ownership === ResourceOwnership.System) {
            return true;
        }
        return false;
    };

    private static checkByRequestType = async (
        request: express.Request,
        currentUser: CurrentUser) => {

        const requestType         = request.requestType;
        const ownership           = request.ownership;
        const actionScope         = request.actionScope;
        const isOwner             = request.resourceOwnerUserId === currentUser.UserId;
        const areTenantsSame      = request.resourceTenantId    === currentUser.TenantId;
        const customAuthorization = request.customAuthorization;

        if (request.optionalUserAuth) {
            // The resources may or may not require user authentication
            // Will be checked specific to the resource visibility...
            // Some resources of a given type may be publicly visible and some may not be
            // For example, File resource - a user profile image file may be publicly visible,
            // but the user document files may not be
            return true;
        }

        //Check if it is single resource request...
        if (
            requestType === RequestType.CreateOne  ||
            requestType === RequestType.GetOne     ||
            requestType === RequestType.UpdateOne  ||
            requestType === RequestType.DeleteOne  ||
            requestType === RequestType.CreateMany ||
            requestType === RequestType.GetMany    ||
            requestType === RequestType.UpdateMany ||
            requestType === RequestType.DeleteMany
        ) {
            return this.checkScopeWithOwnership(ownership, actionScope, isOwner, areTenantsSame);
        }
        if (requestType === RequestType.Search) {
            // Search -> Resources to be filtered according to the ownership and action scope
            // inside the filter settings in controllers
            return true;
        }

        return customAuthorization;
    };

}
