import express from "express";
import { logger } from "../logger/logger";
import { register as registerRoleRoutes } from "./authorization/role/role.routes";
import { register as registerPermissionRoutes } from "./authorization/permission/permission.routes";
import { register as registerUserRoleRoutes } from "./authorization/user.role/user.role.routes";
import { register as registerUserPermissionRoutes } from "./authorization/user.permission/user.permission.routes";
import { register as registerRolePermissionRoutes } from "./authorization/role.permission/role.permission.routes";
import { register as registerClientRoutes } from "./client.apps/client.app.routes";
import { register as registerFileResourceRoutes } from './general/file.resource/file.resource.routes';
import { register as registerUserDeviceDetailsRoutes } from './users/device.details/user.device.details.routes';
import { register as registerUserRoutes } from "./users/user/user.routes";
import { register as registerTenantRoutes } from './tenant/tenants/tenant.routes';
import { register as registerUserMetadataRoutes } from './users/metadata/user.metadata.routes';
import { register as registerUserAuthRoutes } from './users/auth/user.auth.routes';
import { wellKnownRoutes } from './wellknown/wellknown.routes';

////////////////////////////////////////////////////////////////////////////////////

export class Router {

    private _app = null;

    constructor(app: express.Application) {
        this._app = app;
    }

    public init = async (): Promise<boolean> => {
        return new Promise((resolve, reject) => {
            try {

                //Handling the base route
                this._app.get('/api/v1/', (req, res) => {
                    res.send({
                        message : `User Service [Version ${process.env.API_VERSION}]`,
                    });
                });

                // Well-known endpoints (JWKS, OpenID configuration)
                this._app.use('/.well-known', wellKnownRoutes);
                
                registerUserRoutes(this._app);
                registerUserAuthRoutes(this._app);
                registerRoleRoutes(this._app);
                registerPermissionRoutes(this._app);
                registerUserRoleRoutes(this._app);
                registerUserPermissionRoutes(this._app);
                registerRolePermissionRoutes(this._app);
                registerClientRoutes(this._app);
                registerTenantRoutes(this._app);
                registerUserDeviceDetailsRoutes(this._app);
                registerFileResourceRoutes(this._app);
                registerTenantRoutes(this._app);
                registerUserMetadataRoutes(this._app);
                resolve(true);

            } catch (error) {
                logger.error('Error initializing the router: ' + error.message);
                reject(false);
            }
        });
    };

}
