/* eslint-disable no-console */
import { injectable } from "tsyringe";
import { Helper } from "../common/helper";
import { logger } from "../logger/logger";
import { ClientAppService } from "../services/client.apps/client.app.service";
import { UserService } from "../services/users/user.service";
import { Injector } from "./injector";
import { TenantService } from "../services/tenant/tenant.service";

//////////////////////////////////////////////////////////////////////////////

@injectable()
export class Seeder {

    _tenantService = Injector.Container.resolve(TenantService);

    _clientAppService: ClientAppService = Injector.Container.resolve(ClientAppService);

    _userService: UserService = Injector.Container.resolve(UserService);


    public init = async (): Promise<void> => {
        try {
            await this.createTempFolders();
            await this._tenantService.seedDefaultTenant();
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
