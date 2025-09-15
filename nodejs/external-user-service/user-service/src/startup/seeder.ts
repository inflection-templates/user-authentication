/* eslint-disable no-console */
import { injectable } from "tsyringe";
import { Helper } from "../common/helper";
import { logger } from "../logger/logger";
import { ClientAppService } from "../services/client.apps/client.app.service";
import { RoleService } from "../services/authorization/role.service";
import { UserService } from "../services/users/user.service";
import { Injector } from "./injector";
import { TenantService } from "../services/tenant/tenant.service";
import { RolePermissionService } from "../services/authorization/role.permission.service";
import { PermissionService } from "../services/authorization/permission.service";

//////////////////////////////////////////////////////////////////////////////

@injectable()
export class Seeder {

    _tenantService = Injector.Container.resolve(TenantService);

    _clientAppService: ClientAppService = Injector.Container.resolve(ClientAppService);

    _userService: UserService = Injector.Container.resolve(UserService);

    _roleService: RoleService = Injector.Container.resolve(RoleService);

    _rolePermissionService: RolePermissionService = Injector.Container.resolve(RolePermissionService);

    _permissionService: PermissionService = Injector.Container.resolve(PermissionService);

    public init = async (): Promise<void> => {
        try {
            await this.createTempFolders();
            await this._tenantService.seedDefaultTenant();
            await this._roleService.seedDefaultRoles();
            await this._permissionService.seedDefaultPermissions();
            await this._rolePermissionService.seedRolePermissions();
            await this._clientAppService.seedDefaultClients();
            await this._userService.seedSystemAdmin();
        } catch (error) {
            logger.info(error.message);
        }
    };

    private createTempFolders = async () => {
        await Helper.createTempDownloadFolder();
        await Helper.createTempUploadFolder();
    };

}
