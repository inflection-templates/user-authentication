
import express from "express";
import { uuid } from "../domain.types/miscellaneous/system.types";
import { ApiError } from "../common/api.error";
import { TenantService } from "../services/tenant/tenant.service";
import { Injector } from "../startup/injector";
import { UserService } from "../services/users/user.service";

///////////////////////////////////////////////////////////////////////////////////////

export class BaseController {

    public authorizeOne = async (
        request: express.Request,
        resourceOwnerUserId?: uuid,
        resourceTenantId?: uuid): Promise<void> => {

        if (request.currentClient?.IsPrivileged) {
            return;
        }

        let ownerUserId = resourceOwnerUserId ?? null;
        let tenantId = resourceTenantId ?? null;

        if (ownerUserId) {
            const userService = Injector.Container.resolve(UserService);
            const user = await userService.getById(ownerUserId);
            if (user) {
                ownerUserId = user.id;
                tenantId = tenantId ?? user.Tenant?.id;
            }
        }

        if (tenantId == null) {
            // If tenant is not provided, get the default tenant
            const tenantService = Injector.Container.resolve(TenantService);
            const tenant = await tenantService.getTenantWithCode('default');
            if (tenant) {
                tenantId = tenant.id;
            }
        }

        request.resourceOwnerUserId = ownerUserId;
        request.resourceTenantId = tenantId;

        // Authorization simplified - only check if user is authenticated
        if (!request.currentUser) {
            throw new ApiError(403, 'User must be authenticated.');
        }
    };

}
