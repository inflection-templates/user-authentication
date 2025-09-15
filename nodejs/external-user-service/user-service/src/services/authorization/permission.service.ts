import { inject, injectable } from "tsyringe";
import { IPermissionRepo } from "../../database/repository.interfaces/authorization/permission.repo.interface";
import { logger } from "../../logger/logger";
import { ITenantRepo } from "../../database/repository.interfaces/tenant/tenant.repo.interface";
import { uuid } from "../../domain.types/miscellaneous/system.types";
import {
    PermissionDto,
    PermissionCreateModel,
    PermissionSearchFilters,
    PermissionSearchResults,
    PermissionUpdateModel
} from "../../domain.types/authorization/permission.types";
import path from "path";
import fs from "fs";

////////////////////////////////////////////////////////////////////////////////////////////////////////

@injectable()
export class PermissionService {

    constructor(
        @inject('IPermissionRepo') private _permissionRepo: IPermissionRepo,
        @inject('ITenantRepo') private _tenantRepo: ITenantRepo,
    ) {}

    create = async (entity: PermissionCreateModel): Promise<PermissionDto> => {
        return await this._permissionRepo.create(entity);
    };

    getById = async (id: uuid): Promise<PermissionDto> => {
        return await this._permissionRepo.getById(id);
    };

    getByName = async (name: string): Promise<PermissionDto> => {
        return await this._permissionRepo.getByName(name);
    };

    delete = async (id: uuid): Promise<boolean> => {
        return await this._permissionRepo.delete(id);
    };

    search = async (filters: PermissionSearchFilters): Promise<PermissionSearchResults> => {
        return await this._permissionRepo.search(filters);
    };

    searchByName = async (name: string): Promise<PermissionDto[]> => {
        return await this._permissionRepo.searchByName(name);
    };

    update = async (id: uuid, entity: PermissionUpdateModel): Promise<PermissionDto> => {
        return await this._permissionRepo.update(id, entity);
    };

    seedDefaultPermissions = async (): Promise<boolean> => {

        const defaultTenant = await this._tenantRepo.getTenantWithCode('default');
        if (defaultTenant == null) {
            return;
        }
        var filepath = path.join(process.cwd(), 'seed.data', 'authorizations', 'permission.details.json');
        var fileBuffer = fs.readFileSync(filepath, 'utf8');
        const permissions = JSON.parse(fileBuffer);

        for await (const r of permissions) {
            const permission = await this._permissionRepo.getByName(r.name);
            if (permission == null) {
                await this._permissionRepo.create({
                    Name        : r.name,
                    Description : r.description,
                    TenantId    : defaultTenant.id,
                    Module      : r.module
                });
            }
        }

        logger.info('Seeded default permissions successfully!');
        return true;
    };

}
