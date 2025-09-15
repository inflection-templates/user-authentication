import { inject, injectable } from "tsyringe";
import { IRoleRepo } from "../../database/repository.interfaces/authorization/role.repo.interface";
import { RoleDto, RoleCreateModel, RoleSearchFilters, RoleSearchResults, RoleUpdateModel } from "../../domain.types/authorization/role.types";
import { logger } from "../../logger/logger";
import { DefaultRoles } from "../../domain.types/authorization/role.types";
import { ITenantRepo } from "../../database/repository.interfaces/tenant/tenant.repo.interface";
import { uuid } from "../../domain.types/miscellaneous/system.types";

////////////////////////////////////////////////////////////////////////////////////////////////////////

@injectable()
export class RoleService {

    constructor(
        @inject('IRoleRepo') private _roleRepo: IRoleRepo,
        @inject('ITenantRepo') private _tenantRepo: ITenantRepo,
    ) {}

    create = async (entity: RoleCreateModel): Promise<RoleDto> => {
        return await this._roleRepo.create(entity);
    };

    getById = async (id: uuid): Promise<RoleDto> => {
        return await this._roleRepo.getById(id);
    };

    getByName = async (name: string): Promise<RoleDto> => {
        return await this._roleRepo.getByName(name);
    };

    delete = async (id: uuid): Promise<boolean> => {
        return await this._roleRepo.delete(id);
    };

    search = async (filters: RoleSearchFilters): Promise<RoleSearchResults> => {
        return await this._roleRepo.search(filters);
    };

    searchByName = async (name: string): Promise<RoleDto[]> => {
        return await this._roleRepo.searchByName(name);
    };

    update = async (id: uuid, entity: RoleUpdateModel): Promise<RoleDto> => {
        return await this._roleRepo.update(id, entity);
    };

    seedDefaultRoles = async (): Promise<boolean> => {

        const defaultTenant = await this._tenantRepo.getTenantWithCode('default');
        if (defaultTenant == null) {
            return;
        }

        for await (const r of DefaultRoles) {
            const role = await this._roleRepo.getByName(r.Role);
            if (role == null) {
                await this._roleRepo.create({
                    Name         : r.Role,
                    Description  : r.Description,
                    TenantId     : defaultTenant.id,
                    IsSystemRole : false
                });
            }
        }

        logger.info('Seeded default roles successfully!');
        return true;
    };

}
